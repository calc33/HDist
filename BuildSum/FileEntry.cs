using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace BuildSum
{
    partial class FileList
    {
        public class FileEntry : IComparable
        {
            private readonly FileList _owner;
            public string FileName { get; set; }
            public string FileNameWeb
            {
                get
                {
                    return (Path.DirectorySeparatorChar != '/') ? FileName.Replace(Path.DirectorySeparatorChar, '/') : FileName;
                }
            }
            public string? Checksum { get; set; }
            public long Size { get; set; }

            public string? GetChecksum(string directory)
            {
                string path = Path.Combine(directory, FileName);
                if (!File.Exists(path))
                {
                    return null;
                }
                SHA1 sha = SHA1.Create();
                using (FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    sha.ComputeHash(stream);
                }
                if (sha.Hash == null || sha.Hash.Length == 0)
                {
                    return null;
                }
                return Convert.ToBase64String(sha.Hash);
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

            private static string SizeStr(long size)
            {
                string s = size.ToString();
                return s.PadLeft(9);
            }

            public void Write(TextWriter writer)
            {
                string s = SizeStr(GetSize(_owner.BaseDirectory, FileName));
                writer.WriteLine(string.Format("{0}\t{1}\t{2}", GetChecksum(_owner.BaseDirectory), s, FileNameWeb));
            }

            private const string CompressExtenstion = ".bz2";

            public void CompressFile()
            {
                if (string.IsNullOrEmpty(_owner.CompressedDirectory))
                {
                    return;
                }
                string? src = Path.Combine(_owner.BaseDirectory, FileName);
                string? dest = Path.Combine(_owner.CompressedDirectory, FileName) + CompressExtenstion;
                string? destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                OnLog(LogStatus.Information, LogCategory.Compressing, null);
                using FileStream srcStream = new(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using FileStream destStream = new(dest, FileMode.Create, FileAccess.Write, FileShare.None);
                BZip2.Compress(srcStream, destStream, true, 9);
            }

            private void OnLog(LogStatus status, LogCategory category, string? message)
            {
                _owner.OnLog(new LogEventArgs(status, category, FileName, message));
            }

            public FileEntry(FileList owner, string filename)
            {
                _owner = owner;
                FileName = Path.IsPathRooted(filename) ? Path.GetRelativePath(_owner.BaseDirectory, filename) : filename;
                Checksum = GetChecksum(_owner.BaseDirectory);
                Size = GetSize(_owner.BaseDirectory, FileName);
            }

            public int CompareTo(object? obj)
            {
                if (obj is FileEntry entry)
                {
                    return string.Compare(FileName, entry.FileName, FileNameComparison);
                }
                return -1;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", Checksum, FileName);
            }

            public override bool Equals(object? obj)
            {
                if (!(obj is FileEntry entry))
                {
                    return false;
                }
                return string.Compare(FileName, entry.FileName, FileNameComparison) == 0;
            }

            public override int GetHashCode()
            {
                return Treat(FileName).GetHashCode();
            }
        }
    }
}
