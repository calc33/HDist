using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HDistCore
{
    public partial class FileList: IEnumerable<FileList.FileEntry>
    {
        private const string CHECKSUM_FILE = "_checksum.sha";
        private const string UPDATED_FILE = "_updated";
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
        public string DestinationDirectory { get; set; }
        /// <summary>
        /// _checksum.sha のSHA1チェックサム値
        /// </summary>
        public byte[] Checksum { get; }
        private List<FileEntry> _list = new List<FileEntry>();
        //private Dictionary<string, FileEntry> _nameToEntry = new Dictionary<string, FileEntry>();
        private Dictionary<string, FileEntry> _nameToEntry;
        private object _nameToEntryLock = new object();

        private void UpdateNameToEntry()
        {
            if (_nameToEntry != null)
            {
                return;
            }
            lock (_nameToEntryLock)
            {
                if (_nameToEntry != null)
                {
                    return;
                }
                _nameToEntry = new Dictionary<string, FileEntry>();
                foreach (FileEntry entry in _list)
                {
                    _nameToEntry[Treat(entry.FileName)] = entry;
                }
            }
        }

        private void InvalidateNameToEntry()
        {
            lock (_nameToEntryLock)
            {
                _nameToEntry = null;
            }
        }

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

        public FileEntry FindEntry(string filename)
        {
            UpdateNameToEntry();
            FileEntry entry;
            if (_nameToEntry.TryGetValue(Treat(filename), out entry))
            {
                return entry;
            }
            return null;
        }

        public FileEntry FindEntry(string targetDirectory, string filename)
        {
            UpdateNameToEntry();
            string path = Path.GetRelativePath(targetDirectory, filename);
            FileEntry entry;
            if (_nameToEntry.TryGetValue(Treat(path), out entry))
            {
                return entry;
            }
            return null;
        }

        public void SaveChecksum()
        {
            string path = Path.Combine(BaseDirectory, CHECKSUM_FILE);
            SaveToFile(path);
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
            FileAttributes attr = File.GetAttributes(filename);
            File.SetAttributes(filename, attr | FileAttributes.Hidden);
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

        public static bool IsDisabled(string destinationDirectory)
        {
            string path = Path.Combine(destinationDirectory, UPDATED_FILE);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                string s = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(s))
                {
                    s = s.TrimEnd();
                }
                return (s == "-" || s == "noupdate");
            }
        }

        public void UpdateFiles()
        {
            bool success = false;
            _aborting = false;
            _pausing = false;
            foreach (FileEntry entry in _list)
            {
                try
                {
                    entry.UpdateFile();
                }
                catch (Exception t)
                {
                    OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, entry.FileName, t.Message));
                    success = false;
                }
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
                    success = false;
                    OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Aborted, null, null));
                    break;
                }
            }
            try
            {
                string path = Path.Combine(DestinationDirectory, UPDATED_FILE);
                if (success)
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        writer.Write(Convert.ToBase64String(Checksum));
                    }
                }
                else
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
            catch (Exception t)
            {
                OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, UPDATED_FILE, t.Message));
            }
        }

        public void UpdateFiles(string[] files)
        {
            _aborting = false;
            _pausing = false;
            foreach (string s in files)
            {
                FileEntry entry = FindEntry(s);
                if (entry == null)
                {
                    continue;
                }
                try
                {
                    entry.UpdateFile();
                }
                catch (Exception t)
                {
                    OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, entry.FileName, t.Message));
                }
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

        public void WaitUnlocked(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            string path = Path.Combine(DestinationDirectory, filename);
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
            int line = 1;
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
                    long len = -1;
                    switch (strs.Length)
                    {
                        case 2:
                            _list.Add(new FileEntry(this, strs[1], strs[0], len));
                            break;
                        case 3:
                            if (!long.TryParse(strs[1].Trim(), out len))
                            {
                                len = -1;
                            }
                            _list.Add(new FileEntry(this, strs[2], strs[0], len));
                            break;
                        default:
                            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.InvalidChecksumEntry, filename, line.ToString()));
                            break;
                    }
                }
            }
            InvalidateNameToEntry();
            SHA1 sha = SHA1.Create();
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Checksum = sha.ComputeHash(stream);
            }
        }
        
        private bool IsIgnored(string fullPath, string relPath, List<Regex> ignoreNames, bool ignoreHidden)
        {
            if (ignoreHidden && (File.GetAttributes(fullPath) & FileAttributes.Hidden) != 0)
            {
                return true;
            }
            string path1 = Treat(relPath);
            string path2 = Path.GetFileName(path1);
            foreach (Regex regex in ignoreNames)
            {
                if (regex.Match(path1).Success || regex.Match(path2).Success)
                {
                    return true;
                }
            }
            return false;
        }

        private void EnumFiles(string directory, List<Regex> ignoreNames, bool ignoreHidden)
        {
            foreach (string s in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                string relPath = Path.GetRelativePath(BaseDirectory, s);
                if (IsIgnored(s, relPath, ignoreNames, ignoreHidden))
                {
                    continue;
                }
                _list.Add(new FileEntry(this, relPath));
            }
            foreach (string s in Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                string relPath = Path.GetRelativePath(BaseDirectory, s);
                if (IsIgnored(s, relPath, ignoreNames, ignoreHidden))
                {
                    continue;
                }
                EnumFiles(s, ignoreNames, ignoreHidden);
            }
            InvalidateNameToEntry();
        }

        private static Regex ToRegex(string wildcard)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append('^');
            foreach (char c in wildcard)
            {
                switch (c)
                {
                    case '*':
                        buf.Append(".*");
                        break;
                    case '?':
                        buf.Append(".");
                        break;
                    case '$':
                    case '(':
                    case '+':
                    case '.':
                    case '[':
                    case '^':
                    case '{':
                        buf.Append('\\');
                        buf.Append(c);
                        break;
                    default:
                        buf.Append(c);
                        break;
                }
            }
            buf.Append('$');
            return new Regex(buf.ToString());
        }

        internal FileList(string baseDir, IList<string> ignoreNames, bool ignoreHidden)
        {
            FileName = null;
            BaseDirectory = baseDir;
            List<Regex> ignores = new List<Regex>(ignoreNames.Count);
            foreach (string s in ignoreNames)
            {
                ignores.Add(ToRegex(s));
            }
            ignores.Add(ToRegex(CHECKSUM_FILE));
            EnumFiles(baseDir, ignores, ignoreHidden);
            _list.Sort();
            Checksum = new byte[0];
        }

        public static FileList CreateByDirectory(string directory, IList<string> ignoreNames, bool ignodeHidden, string compressDirectory)
        {
            return new FileList(directory, ignoreNames, ignodeHidden)
            {
                CompressedDirectory = compressDirectory,
                DestinationDirectory = directory,
            };
        }

        public static FileList LoadChecksum(string sourceDirectory, string compressDirectory, string destinationDirectory)
        {
            return LoadFromFile(sourceDirectory, Path.Combine(sourceDirectory, CHECKSUM_FILE), compressDirectory, destinationDirectory);
        }
        public static FileList LoadFromFile(string directory, string filename, string compressDirectory, string destinationDirectory)
        {
            return new FileList(directory, filename)
            {
                CompressedDirectory = compressDirectory,
                DestinationDirectory = destinationDirectory,
            };
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
