using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HDistCore;

namespace HCopy
{
    public partial class MainForm : Form
    {
        private static class NativeMethods
        {
            [DllImport("User32.dll")]
            public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        }
        private string _destinationDir;
        private string _sourceDir;
        private string _compressDir;
        private string _statusText;
        private List<string> _logStore;
        private object _logStoreLock = new object();
        private const uint UM_STATUS = 0x4560;
        private const uint UM_LOG = 0x4561;
        private const uint UM_FINISH = 0x4562;
        private const uint UM_UPDATEBUTTON = 0x4563;
        private bool _umStatusPosted = false;
        private bool _umLogPosted = false;

        public string WaitFile { get; set; }
        public string RunFile { get; set; }
        public string RunParam { get; set; }
        public string DestinationDir
        {
            get { return _destinationDir; }
            set
            {
                if (_destinationDir == value)
                {
                    return;
                }
                _destinationDir = value;
                UpdateLabels();
            }
        }
        public string SourceDir
        {
            get { return _sourceDir; }
            set
            {
                if (_sourceDir == value)
                {
                    return;
                }
                _sourceDir = value;
                UpdateLabels();
            }
        }
        public string CompressDir
        {
            get { return _compressDir; }
            set
            {
                if (_compressDir == value)
                {
                    return;
                }
                _compressDir = value;
                UpdateLabels();
            }
        }

        private static string DisplayText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Properties.Resources.EmptyText;
            }
            return value;
        }

        private void UpdateLabels()
        {
            labelSourceDir.Text = DisplayText(_sourceDir);
            labelCompressDir.Text = DisplayText(_compressDir);
            labelDestDir.Text = DisplayText(_destinationDir);
        }

        private void UpdateStatus()
        {
            _umStatusPosted = false;
            labelStatus.Text = _statusText;
        }

        private void UpdateLog()
        {
            _umLogPosted = false;
            if (_logStore.Count == 0)
            {
                return;
            }
            List<string> l = _logStore;
            lock (_logStoreLock)
            {
                _logStore = new List<string>();
            }
            foreach (string s in l)
            {
                textBoxLog.AppendText(s + Environment.NewLine);
            }
            textBoxLog.Select(textBoxLog.Text.Length, 0);
            textBoxLog.ScrollToCaret();
        }

        private bool _quitOnFinish = false;
        private bool _inAutoQuit = false;
        private DateTime _autoQuitTime;
        private void StartAutoQuit()
        {
            _autoQuitTime = DateTime.Now.AddSeconds(6);
            _inAutoQuit = true;
            timerAutoQuit.Start();
            UpdateButtonQuitText();
        }

        private void StopAutoQuit()
        {
            timerAutoQuit.Stop();
            _autoQuitTime = DateTime.MaxValue;
            _inAutoQuit = false;
            UpdateButtonQuitText();
        }

        private void UpdateButtonQuitText()
        {
            if (_inAutoQuit)
            {
                int sec = (int)(_autoQuitTime - DateTime.Now).TotalSeconds;
                sec = Math.Max(0, sec);
                buttonQuit.Text = string.Format("終了中断({0})", sec);
                buttonQuit.Tag = 1;
            }
            else
            {
                buttonQuit.Text = "終了";
                buttonQuit.Tag = 0;
            }
        }

        private void UpdateButtonPauseText()
        {
            buttonPause.Enabled = (_executingTask != null && !_executingTask.IsCompleted);
            buttonPause.Text = (_executingFileList != null && _executingFileList.Pausing) ? "再開" : "中断";
        }

        private void DelayedUpdateButtonText()
        {
            NativeMethods.PostMessage(Handle, UM_UPDATEBUTTON, IntPtr.Zero, IntPtr.Zero);
        }

        private void OnEndThread()
        {
            Log(LogStatus.Information, LogCategory.Finished, null, null);
            if (_quitOnFinish)
            {
                Close();
            }
            else
            {
                StartAutoQuit();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == UM_STATUS)
            {
                UpdateStatus();
                m.Result = new IntPtr(1);
                return;
            }
            if (m.Msg == UM_LOG)
            {
                UpdateLog();
                m.Result = new IntPtr(1);
                return;
            }
            if (m.Msg == UM_FINISH)
            {
                OnEndThread();
                m.Result = new IntPtr(1);
                return;
            }
            if (m.Msg == UM_UPDATEBUTTON)
            {
                UpdateButtonPauseText();
                UpdateButtonQuitText();
                m.Result = new IntPtr(1);
                return;
            }
            base.WndProc(ref m);
        }

        private void ShowUsage()
        {
            MessageBox.Show(this, Properties.Resources.Usage, Properties.Resources.MessageBox_Usage, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Error(string message)
        {
            MessageBox.Show(this, message, Properties.Resources.MessageBox_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private void AddLogStore(string message)
        {
            lock (_logStoreLock)
            {
                _logStore.Add(message);
            }
            if (!_umLogPosted)
            {
                NativeMethods.PostMessage(Handle, UM_LOG, IntPtr.Zero, IntPtr.Zero);
                _umLogPosted = true;
            }
        }

        private void SetStatusText(string text)
        {
            _statusText = text;
            if (!_umStatusPosted)
            {
                NativeMethods.PostMessage(Handle, UM_STATUS, IntPtr.Zero, IntPtr.Zero);
                _umStatusPosted = true;
            }
        }

        private void Log(LogStatus status, LogCategory category, string filename, string message)
        {
            string msg = LogResource.GetMessage(category, filename, message);
            switch (status)
            {
                case LogStatus.Information:
                case LogStatus.Warning:
                case LogStatus.Error:
                    AddLogStore(msg);
                    break;
                case LogStatus.Progress:
                    SetStatusText(msg);
                    break;
                default:
                    break;
            }
        }

        private void FileList_Log(object sender, LogEventArgs e)
        {
            Log(e.Status, e.Category, e.FileName, e.Message);
        }

        private void WaitLocked()
        {
            if (string.IsNullOrEmpty(WaitFile))
            {
                return;
            }
            string path = Path.Combine(DestinationDir, WaitFile);
            if (!File.Exists(path))
            {
                return;
            }
            bool locked;
            do
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                    locked = false;
                }
                catch (IOException)
                {
                    locked = true;
                    Thread.Sleep(100);
                }
            } while (locked);
        }

        private void RunProgram()
        {
            if (string.IsNullOrEmpty(RunFile))
            {
                return;
            }
            try
            {
                Process.Start(RunFile, RunParam);
            }
            catch (Exception t)
            {
                Log(LogStatus.Error, LogCategory.Exception, RunFile, t.Message);
                Environment.Exit(1);
            }
        }

        public void InitByArgs(string[] args)
        {
            List<string> paths = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                try
                {
                    switch (a)
                    {
                        case "--wait":
                        case "-w":
                            i++;
                            WaitFile = args[i];
                            break;
                        case "--run":
                        case "-r":
                            i++;
                            RunFile = args[i];
                            break;
                        case "--param":
                        case "-p":
                            i++;
                            RunParam = args[i];
                            break;
                        case "--compress":
                        case "-c":
                            i++;
                            CompressDir = args[i];
                            break;
                        case "--help":
                            ShowUsage();
                            break;
                        default:
                            if (a.StartsWith("-"))
                            {
                                Error(string.Format("不明なオプション: {0}", a));
                            }
                            paths.Add(a);
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Error(string.Format("{0}に続くパラメータがありません", a));
                }
            }
            if (paths.Count != 2)
            {
                ShowUsage();
            }
            SourceDir = paths[0];
            DestinationDir = paths[1];
        }

        private void StartThread()
        {
            //AddLogStore("コピーを開始します");
        }

        private void EndThread()
        {
            NativeMethods.PostMessage(Handle, UM_FINISH, IntPtr.Zero, IntPtr.Zero);
            _executingFileList = null;
            _executingTask = null;
        }

        private FileList _executingFileList = null;
        private Task _executingTask = null;

        public void Run()
        {
            _executingFileList = FileList.LoadChecksum(SourceDir);
            _executingFileList.WaitUnlocked(DestinationDir, WaitFile);
            _executingFileList.CompressedDirectory = CompressDir;
            _executingFileList.Log += FileList_Log;
            _executingTask = Task.Run(() => { StartThread(); _executingFileList.UpdateFiles(DestinationDir); EndThread(); });
            UpdateButtonPauseText();
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateLabels();
            textBoxLog.Clear();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RunProgram();
        }

        private void timerAutoQuit_Tick(object sender, EventArgs e)
        {
            UpdateButtonQuitText();
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (_executingFileList == null)
            {
                return;
            }
            if (_executingFileList.Pausing)
            {
                _executingFileList.Pause();
            }
            else
            {
                _executingFileList.Resume();
            }
            DelayedUpdateButtonText();
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            if (_inAutoQuit)
            {
                StopAutoQuit();
                DelayedUpdateButtonText();
                return;
            }
            _quitOnFinish = true;
            _executingFileList?.Abort();
            DelayedUpdateButtonText();
        }
    }
}
