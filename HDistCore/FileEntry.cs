using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace HDistCore
{
    partial class FileList
    {
        public class FileEntry : IComparable
        {
            private FileList _owner;
            public string FileName { get; set; }
            public string Checksum { get; set; }
            public long Size { get; set; }

            public string GetChecksum(string directory)
            {
                string path = Path.Combine(directory, FileName);
                if (!File.Exists(path))
                {
                    return null;
                }
                SHA1 sha = SHA1.Create();
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    sha.ComputeHash(stream);
                }
                return Convert.ToBase64String(sha.Hash);
            }

            public static long GetSize(string path)
            {
                if (!File.Exists(path))
                {
                    return -1;
                }
                return new FileInfo(path).Length;
            }

            public static long GetSize(string directory, string filename)
            {
                string path = Path.Combine(directory, filename);
                if (!File.Exists(path))
                {
                    return -1;
                }
                return new FileInfo(path).Length;
            }

            public bool IsModified(string directory)
            {
                return (Size != -1 && Size != GetSize(directory, FileName)) || Checksum != GetChecksum(directory);
            }

            public void Write(TextWriter writer)
            {
                writer.WriteLine(string.Format("{0}\t{1}\t{2}", GetChecksum(_owner.BaseDirectory), GetSize(_owner.BaseDirectory, FileName), FileName));
            }

            private const string CompressExtenstion = ".bz2";
            private bool ExtractFile()
            {
                if (string.IsNullOrEmpty(_owner.CompressedDirectory))
                {
                    return false;
                }
                string dir = Path.Combine(_owner.BaseDirectory, _owner.CompressedDirectory);
                string src = Path.Combine(dir, FileName) + CompressExtenstion;
                if (!File.Exists(src))
                {
                    return false;
                }
                OnLog(LogStatus.Information, LogCategory.CopyCompressed, null);
                using (FileStream srcStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string dest = Path.Combine(_owner.DestinationDirectory, FileName);
                    using (FileStream destStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BZip2.Decompress(srcStream, destStream, true);
                    }
                }
                bool mod = IsModified(_owner.DestinationDirectory);
                if (mod)
                {
                    OnLog(LogStatus.Warning, LogCategory.FailToExtract, null);
                }
                return !mod;
            }

            public void CompressFile()
            {
                if (string.IsNullOrEmpty(_owner.CompressedDirectory))
                {
                    return;
                }
                string src = Path.Combine(_owner.BaseDirectory, FileName);
                string dest = Path.Combine(_owner.CompressedDirectory, FileName) + CompressExtenstion;
                string destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                OnLog(LogStatus.Information, LogCategory.Compressing, null);
                using (FileStream srcStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (FileStream destStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BZip2.Compress(srcStream, destStream, true, 9);
                    }
                }
            }

            private void CopyFile()
            {
                string src = Path.Combine(_owner.BaseDirectory, FileName);
                string dest = Path.Combine(_owner.DestinationDirectory, FileName);
                string destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                bool flag = false;
                try
                {
                    flag = ExtractFile();
                }
                catch (Exception t)
                {
                    OnLog(LogStatus.Error, LogCategory.Exception, t.Message);
                }
                if (!flag)
                {
                    OnLog(LogStatus.Information, LogCategory.Copy, null);
                    try
                    {
                        File.Copy(src, dest, true);
                    }
                    catch (Exception t)
                    {
                        OnLog(LogStatus.Error, LogCategory.Exception, t.Message);
                    }
                }
            }

            public void UpdateFile()
            {
                OnLog(LogStatus.Progress, LogCategory.NoMessage, null);
                if (!IsModified(_owner.DestinationDirectory))
                {
                    return;
                }
                CopyFile();
                return;
            }

            private void OnLog(LogStatus status, LogCategory category, string message)
            {
                _owner.OnLog(new LogEventArgs(status, category, FileName, message));
            }

            public FileEntry(FileList owner, string filename, string checksum, long size)
            {
                _owner = owner;
                FileName = Path.IsPathRooted(filename) ? Path.GetRelativePath(_owner.BaseDirectory, filename) : filename;
                Checksum = checksum;
                Size = size;
            }

            public FileEntry(FileList owner, string filename)
            {
                _owner = owner;
                FileName = Path.IsPathRooted(filename) ? Path.GetRelativePath(_owner.BaseDirectory, filename) : filename;
                Checksum = GetChecksum(_owner.BaseDirectory);
                Size = GetSize(_owner.BaseDirectory, FileName);
            }

            public int CompareTo(object obj)
            {
                if (!(obj is FileEntry))
                {
                    return -1;
                }
                return string.Compare(FileName, ((FileEntry)obj).FileName, FileNameComparison);
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", Checksum, FileName);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FileEntry))
                {
                    return false;
                }
                return string.Compare(FileName, ((FileEntry)obj).FileName, FileNameComparison) == 0;
            }

            public override int GetHashCode()
            {
                return Treat(FileName).GetHashCode();
            }
        }
    }
}
