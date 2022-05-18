using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HDistCore;

namespace HCopy
{
    public class MainModule
    {
        private string WaitFile;
        private string RunFile;
        private string RunParam;
        private string DestinationDir;
        private string SourceDir;
        private string CompressDir;
        
        private void ShowUsage()
        {
            Console.Error.Write(Properties.Resources.Usage);
        }

        private void Error(string message)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(1);
        }

        private void Log(LogStatus status, LogCategory category, string filename, string message)
        {
            TextWriter writer = Console.Out;
            switch (status)
            {
                case LogStatus.Information:
                    break;
                case LogStatus.Warning:
                    writer = Console.Error;
                    break;
                case LogStatus.Error:
                    writer = Console.Error;
                    break;
                case LogStatus.Progress:
                    writer = null;
                    break;
                default:
                    break;
            }
            string msg = LogResource.GetMessage(category, filename, message);
            if (writer != null)
            {
                writer.WriteLine(msg);
            }
            writer.Flush();
        }
        
        private void Checksum_Log(object sender, LogEventArgs e)
        {
            Log(e.Status, e.Category, e.FileName, e.Message);
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

        public MainModule(string[] args)
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
        public void Run()
        {
            HDistCore.FileList list = HDistCore.FileList.LoadChecksum(SourceDir);
            list.WaitUnlocked(DestinationDir, WaitFile);
            list.CompressedDirectory = CompressDir;
            list.Log += Checksum_Log;
            list.UpdateFiles(DestinationDir);
            RunProgram();
        }
    }
}
