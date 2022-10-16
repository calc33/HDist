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
        
        private void ShowUsage(int exitCode)
        {
            Console.Error.Write(Properties.Resources.Usage);
            Environment.Exit(exitCode);
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
                writer.Flush();
            }
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
                        case "/?":
                            ShowUsage(0);
                            return;
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
                    ShowUsage(1);
                    return;
            }
        }
        public void Run()
        {
            try
            {
                if (FileList.IsDisabled(DestinationDir))
                {
                    Log(LogStatus.Information, LogCategory.SuppressUpdating, null, null);
                    return;
                }
                FileList list = FileList.LoadChecksum(SourceDir, CompressDir, DestinationDir);
                list.Log += Checksum_Log;
                list.WaitUnlocked(WaitFile);
                list.UpdateFiles();
            }
            finally
            {
                RunProgram();
            }
        }
    }
}
