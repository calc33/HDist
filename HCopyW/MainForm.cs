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
        private bool _isRan = false;
        private List<string> _logStore = new List<string>();
        private object _logStoreLock = new object();
        private const uint UM_STATUS = 0x4560;
        private const uint UM_LOG = 0x4561;
        private const uint UM_FINISH = 0x4562;
        private const uint UM_UPDATEBUTTON = 0x4563;
        private const uint UM_START = 0x4564;

        private bool _umStatusPosted = false;
        private bool _umLogPosted = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WaitFile { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RunFile { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RunParam { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        private string _logFile = null;
        private string _actualLogFile = null;
        private TextWriter _logWriter = null;
        private int _processId;
        private static Dictionary<char, string> ParamCharToValue = new Dictionary<char, string>()
        {
            {'Y', DateTime.Today.Year.ToString("D4") },
            {'M', DateTime.Today.Month.ToString("D2") },
            {'D', DateTime.Today.Day.ToString("D2") },
            {'H', DateTime.Now.Hour.ToString("D2") },
            {'N', DateTime.Now.Minute.ToString("D2") },
            {'S', DateTime.Now.Second.ToString("D2") },
            {'P', Process.GetCurrentProcess().Id.ToString() },
            {'%', "%" },
        };
        private void UpdateActualLogFile()
        {
            StringBuilder buf = new StringBuilder();
            if (string.IsNullOrEmpty(_logFile))
            {
                _actualLogFile = null;
            }
            bool wasPercent = false;
            foreach (char c in _logFile)
            {
                if (wasPercent)
                {
                    string s;
                    if (ParamCharToValue.TryGetValue(char.ToUpper(c), out s))
                    {
                        buf.Append(s);
                    }
                    else
                    {
                        buf.Append('%');
                        buf.Append(c);
                    }
                    wasPercent = false;
                    continue;
                }
                if (c == '%')
                {
                    wasPercent = true;
                }
                else
                {
                    wasPercent = false;
                    buf.Append(c);
                }
            }
            _actualLogFile = buf.ToString();
        }
        
        private void UpdateLogStream()
        {
            if (_logWriter != null)
            {
                _logWriter.Dispose();
                _logWriter = null;
            }
            if (string.IsNullOrEmpty(_actualLogFile))
            {
                return;
            }
            string dir = Path.GetDirectoryName(_actualLogFile);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _logWriter = new StreamWriter(new FileStream(_actualLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            Process p = Process.GetCurrentProcess();
            _processId = p.Id;
            _logWriter.WriteLine(string.Format("[{0:HH:mm:ss}({1})] Start {2} {3}", DateTime.Now, _processId, p.StartInfo.FileName, p.StartInfo.Arguments));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LogFile
        {
            get { return _logFile; }
            set
            {
                if (_logFile == value)
                {
                    return;
                }
                _logFile = value;
                UpdateActualLogFile();
                UpdateLogStream();
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
            _logWriter?.Flush();
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
                buttonQuit.Text = string.Format(Properties.Resources.ButtonQuit_AutoQuitFmt, sec);
                buttonQuit.Tag = 1;
            }
            else
            {
                buttonQuit.Text = Properties.Resources.ButtonQuit_Quit;
                buttonQuit.Tag = 0;
            }
        }

        private void UpdateButtonPauseText()
        {
            buttonPause.Enabled = (_executingTask != null && !_executingTask.IsCompleted);
            buttonPause.Text = (_executingFileList != null && _executingFileList.Pausing) ? Properties.Resources.ButtonPause_Resume : Properties.Resources.ButtonPause_Pause;
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
            if (m.Msg == UM_START)
            {
                if (!_isRan)
                {
                    _isRan = true;
                    Run();
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Handleはマルチスレッドでアクセスできないので_handleにコピーして参照
        /// </summary>
        private IntPtr _handle;

        protected override void OnHandleCreated(EventArgs e)
        {
            _handle = Handle;
            base.OnHandleCreated(e);
        }


        private void PostMsg(uint Msg)
        {
            NativeMethods.PostMessage(_handle, Msg, IntPtr.Zero, IntPtr.Zero);
        }

        private void ShowUsage()
        {
            MessageBox.Show(this, Properties.Resources.Usage, Properties.Resources.MessageBox_Usage, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
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
                PostMsg(UM_LOG);
                _umLogPosted = true;
            }
            _logWriter?.WriteLine(string.Format("[{0:HH:mm:ss}({1})] {2}", DateTime.Now, _processId, message));
        }

        private void SetStatusText(string text)
        {
            _statusText = text;
            if (!_umStatusPosted)
            {
                PostMsg(UM_STATUS);
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
                        case "--log":
                        case "-l":
                            i++;
                            LogFile = args[i];
                            break;
                        case "--help":
                        case "/?":
                            ShowUsage();

                            break;
                        default:
                            if (a.StartsWith("-"))
                            {
                                Error(string.Format(Properties.Resources.InvalidOptionFmt, a));
                            }
                            paths.Add(a);
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Error(string.Format(Properties.Resources.ParameterRequiedFmt, a));
                }
            }
            switch (paths.Count)
            {
                case 1:
                    SourceDir = paths[0];
                    DestinationDir = Directory.GetCurrentDirectory();
                    break;
                case 2:
                    SourceDir = paths[0];
                    DestinationDir = paths[1];
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private void StartThread()
        {
            //AddLogStore("コピーを開始します");
        }

        private void EndThread()
        {
            PostMsg(UM_FINISH);
            _executingFileList = null;
            _executingTask = null;
        }

        private FileList _executingFileList = null;
        private Task _executingTask = null;

        public void Run()
        {
            try
            {
                if (FileList.IsDisabled(DestinationDir))
                {
                    Log(LogStatus.Information, LogCategory.SuppressUpdating, null, null);
                    StartAutoQuit();
                    return;
                }
                try
                {
                    _executingFileList = FileList.LoadChecksum(SourceDir, CompressDir, DestinationDir);
                }
                catch (ApplicationException t)
                {
                    Log(LogStatus.Error, LogCategory.Exception, t.Message, null);
                    StartAutoQuit();
                    return;
                }
                _executingFileList.Log += FileList_Log;
                _executingFileList.WaitUnlocked(WaitFile);
                _executingTask = Task.Run(() => { StartThread(); _executingFileList.UpdateFiles(); EndThread(); });
            }
            finally
            {
                UpdateButtonPauseText();
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] cmds = Environment.GetCommandLineArgs();
            string[] args = new string[cmds.Length - 1];
            Array.Copy(cmds, 1, args, 0, args.Length);
            InitByArgs(args);
            UpdateLabels();
            textBoxLog.Clear();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            PostMsg(UM_START);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                return;
            }
            Task task = _executingTask;
            if (task == null || task.IsCompleted)
            {
                return;
            }
            DialogResult ret = MessageBox.Show(this, Properties.Resources.MessageBox_AbortCopy, Properties.Resources.MessageBox_Confirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (ret != DialogResult.Yes)
            {
                e.Cancel = true;
            }
            _quitOnFinish = true;
            FileList list = _executingFileList;
            if (list != null)
            {
                list.Abort();
                e.Cancel = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RunProgram();
        }

        private void timerAutoQuit_Tick(object sender, EventArgs e)
        {
            UpdateButtonQuitText();
            if (_autoQuitTime <= DateTime.Now)
            {
                Close();
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            FileList list = _executingFileList;
            if (list == null)
            {
                return;
            }
            if (list.Pausing)
            {
                list.Pause();
            }
            else
            {
                list.Resume();
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
            FileList list = _executingFileList;
            if (list != null)
            {
                list.Abort();
                DelayedUpdateButtonText();
            }
            else
            {
                Close();
            }
        }
    }
}
