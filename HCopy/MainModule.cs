using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HDist.Core;

namespace HCopy
{
    public class MainModule
    {
        private readonly string? WaitFile;
        private readonly string? RunFile;
        private readonly string? RunParam;
        private readonly string DestinationDir;
        private readonly string SourceUri;
        private readonly string? CompressDir;
        
        private static void ShowUsage(int exitCode)
        {
            Console.Error.Write(Properties.Resources.Usage);
            Environment.Exit(exitCode);
        }

        private static void Error(string message)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(1);
        }

        private static void Log(LogStatus status, LogCategory category, string? filename, string? message)
        {
            TextWriter? writer = Console.Out;
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
        
        private void Checksum_Log(object? sender, LogEventArgs e)
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
                Process.Start(RunFile, RunParam ?? string.Empty);
            }
            catch (Exception t)
            {
                Log(LogStatus.Error, LogCategory.Exception, RunFile, t.Message);
                Environment.Exit(1);
            }
        }

        public MainModule(string[] args)
        {
            SourceUri = string.Empty;
            DestinationDir = string.Empty;
            List<string> paths = new();
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
                            ShowUsage(0);   // ShowUsage中でプログラムが終了するので以降の処理はない
                            return;
                        default:
                            if (a.StartsWith('-'))
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
                    SourceUri = paths[0];
                    DestinationDir = Directory.GetCurrentDirectory();
                    break;
                case 2:
                    SourceUri = paths[0];
                    DestinationDir = paths[1];
                    break;
            }
            if (string.IsNullOrEmpty(SourceUri) || string.IsNullOrEmpty(DestinationDir))
            {
                ShowUsage(1);   // ShowUsage中でプログラムが終了するので以降の処理はない
            }
        }
        public async Task RunAsync()
        {
            try
            {
                if (FileList.IsDisabled(DestinationDir))
                {
                    Log(LogStatus.Information, LogCategory.SuppressUpdating, null, null);
                    return;
                }
                FileList list = FileList.LoadChecksum(SourceUri, CompressDir, DestinationDir);
                list.Log += Checksum_Log;
                list.WaitUnlocked(WaitFile);
                await list.UpdateFilesAsync();
            }
            finally
            {
                RunProgram();
            }
        }
    }
}
