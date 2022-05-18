using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace HDistCore
{
    public class FileList: IEnumerable<FileList.FileEntry>
    {
        private const string CHECKSUM = "checksum.md5";
        public class FileEntry : IComparable
        {
            private FileList _owner;
            public string FileName { get; set; }
            public string Checksum { get; set; }

            public string GetChecksum(string directory)
            {
                string path = Path.Combine(directory, FileName);
                if (!File.Exists(path))
                {
                    return null;
                }
                MD5 md5 = MD5.Create();
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    md5.ComputeHash(stream);
                }
                return Convert.ToBase64String(md5.Hash);
            }

            public bool IsModified(string directory)
            {
                return Checksum != GetChecksum(directory);
            }

            public void Write(TextWriter writer)
            {
                writer.WriteLine(string.Format("{0}\t{1}", GetChecksum(_owner.BaseDirectory), FileName));
            }

            private const string CompressExtenstion = ".bz2";
            private bool ExtractFile(string destination)
            {
                if (string.IsNullOrEmpty(_owner.CompressedDirectory))
                {
                    return false;
                }
                string src = Path.Combine(_owner.CompressedDirectory, FileName) + CompressExtenstion;
                if (!File.Exists(src))
                {
                    return false;
                }
                OnLog(LogStatus.Information, LogCategory.CopyCompressed, null);
                using (FileStream srcStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (FileStream destStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BZip2.Decompress(srcStream, destStream, true);
                    }
                }
                bool mod = IsModified(destination);
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

            private void CopyFile(string destinationDirectory)
            {
                string src = Path.Combine(_owner.BaseDirectory, FileName);
                string dest = Path.Combine(destinationDirectory, FileName);
                string destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                bool flag = false;
                try
                {
                    flag = ExtractFile(dest);
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
                    catch(Exception t)
                    {
                        OnLog(LogStatus.Error, LogCategory.Exception, t.Message);
                    }
                }
            }

            public void UpdateFile(string destinationDirectory)
            {
                OnLog(LogStatus.Progress, LogCategory.NoMessage, null);
                if (!IsModified(destinationDirectory))
                {
                    return;
                }
                CopyFile(destinationDirectory);
            }

            private void OnLog(LogStatus status, LogCategory category, string message)
            {
                _owner.OnLog(new LogEventArgs(status, category, FileName, message));
            }

            public FileEntry(FileList owner, string filename, string checksum)
            {
                _owner = owner;
                FileName = Path.IsPathRooted(filename) ? Path.GetRelativePath(_owner.BaseDirectory, filename) : filename;
                Checksum = checksum;
            }

            public FileEntry(FileList owner, string filename)
            {
                _owner = owner;
                FileName = Path.IsPathRooted(filename) ? Path.GetRelativePath(_owner.BaseDirectory, filename) : filename;
                Checksum = GetChecksum(_owner.BaseDirectory);
            }

            public int CompareTo(object obj)
            {
                if (!(obj is FileEntry))
                {
                    return -1;
                }
                return string.Compare(FileName, ((FileEntry)obj).FileName, FileList.FileNameComparison);
            }
        }
        private static StringComparison FileNameComparison = StringComparison.OrdinalIgnoreCase;
        private static bool _ignoreCase = true;
        public static bool IgnoreCase
        {
            get
            {
                return _ignoreCase;
            }
            set
            {
                _ignoreCase = value;
                FileNameComparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            }
        }

        private static string Treat(string value)
        {
            return (_ignoreCase && !string.IsNullOrEmpty(value)) ? value.ToLower() : value;
        }

        public Encoding FileEncoding { get; set; } = Encoding.UTF8;

        public string FileName { get; set; }
        public string BaseDirectory { get; set; }
        public string CompressedDirectory { get; set; }
        private List<FileEntry> _list = new List<FileEntry>();
        private Dictionary<string, FileEntry> _nameToEntry = new Dictionary<string, FileEntry>();

        private bool _aborting = false;
        private bool _pausing = false;
        
        public bool Aborting { get { return _aborting; } }
        
        public bool Pausing { get { return _pausing; } }

        public event EventHandler<LogEventArgs> Log;

        public void OnLog(LogEventArgs e)
        {
            Log?.Invoke(this, e);
        }

        public int Count => _list.Count;

        public FileEntry this[int index]
        {
            get
            {
                return _list[index];
            }
        }

        public void SaveChecksum()
        {
            SaveToFile(Path.Combine(BaseDirectory, CHECKSUM));
        }

        public void SaveToFile(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename, false, FileEncoding))
            {
                foreach (FileEntry entry in _list)
                {
                    writer.WriteLine(entry.GetChecksum(BaseDirectory) + "\t" + entry.FileName);
                }
            }
        }
        public void SaveToStream(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, FileEncoding))
            {
                foreach (FileEntry entry in _list)
                {
                    entry.Write(writer);
                }
            }
        }

        public void UpdateFiles(string destinationDirectory)
        {
            _aborting = false;
            _pausing = false;
            foreach (FileEntry entry in _list)
            {
                entry.UpdateFile(destinationDirectory);
                if (_pausing)
                {
                    OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Paused, null, null));
                    while (_pausing && !_aborting)
                    {
                        Thread.Sleep(100);
                    }
                }
                if (_aborting)
                {
                    OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Aborted, null, null));
                    break;
                }
            }
        }

        public void Abort()
        {
            _aborting = true;
        }

        public void Pause()
        {
            _pausing = true;
        }

        public void Resume()
        {
            _pausing = false;
        }

        private bool IsWritable(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void WaitUnlocked(string directory, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            string path = Path.Combine(directory, filename);
            if (IsWritable(path))
            {
                return;
            }
            OnLog(new LogEventArgs(LogStatus.Information, LogCategory.WaitLocked, filename, null));
            while (!IsWritable(path))
            {
                Thread.Sleep(100);
            }
        }

        internal FileList(string baseDir, string filename)
        {
            FileName = filename;
            BaseDirectory = baseDir;
            using (StreamReader reader = new StreamReader(filename, FileEncoding))
            {
                while (!reader.EndOfStream)
                {
                    string s = reader.ReadLine();
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    string[] strs = s.Split('\t');
                    if (2 <= strs.Length)
                    {
                        _list.Add(new FileEntry(this, strs[1], strs[0]));
                    }
                }
            }
        }
        
        private void EnumFiles(string directory, Dictionary<string, bool> ignoreNames)
        {
            foreach (string s in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                string path1 = Path.GetRelativePath(BaseDirectory, s);
                string path2 = Path.GetFileName(s);
                if (ignoreNames.ContainsKey(Treat(path1)) || ignoreNames.ContainsKey(Treat(path2)))
                {
                    continue;
                }
                _list.Add(new FileEntry(this, path1));
            }
            foreach (string s in Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                string path1 = Path.GetRelativePath(BaseDirectory, s);
                string path2 = Path.GetFileName(s);
                if (ignoreNames.ContainsKey(Treat(path1)) || ignoreNames.ContainsKey(Treat(path2)))
                {
                    continue;
                }
                EnumFiles(s, ignoreNames);
            }

        }

        internal FileList(string baseDir, IList<string> ignoreNames)
        {
            FileName = null;
            BaseDirectory = baseDir;
            Dictionary<string, bool> ignoreDict = new Dictionary<string, bool>(ignoreNames.Count);
            foreach (string s in ignoreNames)
            {
                ignoreDict[Treat(s)] = true;
            }
            ignoreDict[Treat(CHECKSUM)] = true;
            EnumFiles(baseDir, ignoreDict);
            _list.Sort();
        }

        public static FileList CreateByDirectory(string directory, IList<string> ignoreNames)
        {
            return new FileList(directory, ignoreNames);
        }

        public static FileList LoadChecksum(string directory)
        {
            return LoadFromFile(directory, Path.Combine(directory, CHECKSUM));
        }
        public static FileList LoadFromFile(string directory, string filename)
        {
            return new FileList(directory, filename);
        }

        #region IEnumerable<FileEntry>
        public IEnumerator<FileEntry> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        #endregion
    }
}
