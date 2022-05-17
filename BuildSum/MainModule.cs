﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HashCore;

namespace BuildHash
{
    public class MainModule
    {
        public string TargetDir;
        public string CompressDir;
        public List<string> IgnoreFiles = new List<string>();

        private void Log(LogStatus status, LogCategory category, string filename, string message)
        {
            TextWriter writer = Console.Out;
            bool newline = true;
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
                    newline = false;
                    break;
                default:
                    break;
            }
            string msg = LogResource.GetMessage(category, filename, message);
            if (newline)
            {
                writer.WriteLine(msg);
            }
            else
            {
                writer.Write(msg);
            }
        }
        private void FileList_Log(object sender, LogEventArgs e)
        {
            Log(e.Status, e.Category, e.FileName, e.Message);
        }
        public void Error(string message)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(1);
        }
        private void ShowUsage()
        {
            Console.Error.Write(Properties.Resources.Usage);
            Environment.Exit(0);
        }
        public MainModule(string[] args)
        {
            string ignorePath = null;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                try
                {
                    switch (a)
                    {
                        case "--compress":
                        case "-c":
                            i++;
                            CompressDir = args[i];
                            break;
                        case "--ignore":
                        case "-i":
                            if (ignorePath != null)
                            {
                                Error("--ignoreと--ignore-listを同時に指定できません");
                            }
                            i++;
                            IgnoreFiles.Add(args[i]);
                            break;
                        case "--ignore-list":
                        case "-I":
                            if (ignorePath != null)
                            {
                                Error("--ignore-listを複数指定できません");
                            }
                            if (IgnoreFiles.Count != 0)
                            {
                                Error("--ignoreと--ignore-listを同時に指定できません");
                            }
                            i++;
                            ignorePath = args[i];
                            break;
                        case "--help":
                            ShowUsage();
                            break;
                        default:
                            if (TargetDir != null)
                            {
                                ShowUsage();
                            }
                            TargetDir = a;
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Error(string.Format("{0}に続くパラメータがありません", a));
                }
            }
            string dir = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(TargetDir))
            {
                TargetDir = dir;
            }
            else
            {
                TargetDir = Path.Combine(dir, TargetDir);
            }
            if (ignorePath != null)
            {
                using (StreamReader reader = new StreamReader(ignorePath, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        string s = reader.ReadLine();
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        IgnoreFiles.Add(s);
                    }
                }
            }
        }

        private void CompressFiles(FileList list)
        {
            if (string.IsNullOrEmpty(CompressDir))
            {
                return;
            }
            foreach (FileList.FileEntry entry in list)
            {
                entry.CompressFile();
            }
        }

        public void Run()
        {
            if (!Directory.Exists(TargetDir))
            {
                Error(string.Format("{0}: ディレクトリがありません", TargetDir));
            }
            FileList list = FileList.CreateByDirectory(TargetDir, IgnoreFiles);
            list.CompressedDirectory = CompressDir;
            list.Log += FileList_Log;
            list.SaveChecksum();
            CompressFiles(list);
        }

    }
}