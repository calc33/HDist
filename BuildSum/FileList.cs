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

namespace BuildSum
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

        public string? ChecksumFileName { get; set; }
        public string BaseDirectory { get; set; }
        public string? CompressedDirectory { get; set; }
        /// <summary>
        /// _checksum.sha のSHA1チェックサム値
        /// </summary>
        public byte[] Checksum { get; }
        private readonly List<FileEntry> _list = new();
        private Dictionary<string, FileEntry>? _nameToEntry = null;
        private readonly Lock _nameToEntryLock = new();

        private Dictionary<string, FileEntry> RequireNameToEntry()
        {
            if (_nameToEntry != null)
            {
                return _nameToEntry;
            }
            lock (_nameToEntryLock)
            {
                if (_nameToEntry != null)
                {
                    return _nameToEntry;
                }
                _nameToEntry = [];
                foreach (FileEntry entry in _list)
                {
                    _nameToEntry[Treat(entry.FileName)] = entry;
                }
                return _nameToEntry;
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

        public event EventHandler<LogEventArgs>? Log;

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

        public FileEntry? FindEntry(string filename)
        {
            if (RequireNameToEntry().TryGetValue(Treat(filename), out FileEntry? entry))
            {
                return entry;
            }
            return null;
        }

        public FileEntry? FindEntry(string targetDirectory, string filename)
        {
            string path = Path.GetRelativePath(targetDirectory, filename);
            if (RequireNameToEntry().TryGetValue(Treat(path), out FileEntry? entry))
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
            string path = Path.GetTempFileName();
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                SaveToStream(stream);
            }
            File.Delete(filename);
            File.Move(path, filename);
            FileAttributes attr = File.GetAttributes(filename);
            File.SetAttributes(filename, attr | FileAttributes.Hidden);
        }
        public void SaveToStream(Stream stream)
        {
            using StreamWriter writer = new(stream, FileEncoding);
            foreach (FileEntry entry in _list)
            {
                entry.Write(writer);
            }
        }

        //public static bool IsDisabled(string destinationDirectory)
        //{
        //    string path = Path.Combine(destinationDirectory, UPDATED_FILE);
        //    if (string.IsNullOrEmpty(path) || !File.Exists(path))
        //    {
        //        return false;
        //    }
        //    using StreamReader reader = new(path, Encoding.UTF8);
        //    string s = reader.ReadToEnd();
        //    if (!string.IsNullOrEmpty(s))
        //    {
        //        s = s.TrimEnd();
        //    }
        //    return (s == "-" || s == "noupdate");
        //}

        //public void UpdateFiles()
        //{
        //    bool success = false;
        //    _aborting = false;
        //    _pausing = false;
        //    foreach (FileEntry entry in _list)
        //    {
        //        try
        //        {
        //            entry.UpdateFile();
        //        }
        //        catch (Exception t)
        //        {
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, entry.FileName, t.Message));
        //            success = false;
        //        }
        //        if (_pausing)
        //        {
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Paused, null, null));
        //            while (_pausing && !_aborting)
        //            {
        //                Thread.Sleep(100);
        //            }
        //        }
        //        if (_aborting)
        //        {
        //            success = false;
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Aborted, null, null));
        //            break;
        //        }
        //    }
        //    try
        //    {
        //        string path = Path.Combine(DestinationDirectory, UPDATED_FILE);
        //        if (success)
        //        {
        //            using StreamWriter writer = new(path, false, Encoding.UTF8);
        //            writer.Write(Convert.ToBase64String(Checksum));
        //        }
        //        else
        //        {
        //            if (File.Exists(path))
        //            {
        //                File.Delete(path);
        //            }
        //        }
        //    }
        //    catch (Exception t)
        //    {
        //        OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, UPDATED_FILE, t.Message));
        //    }
        //}

        //public void UpdateFiles(string[] files)
        //{
        //    _aborting = false;
        //    _pausing = false;
        //    foreach (string s in files)
        //    {
        //        FileEntry? entry = FindEntry(s);
        //        if (entry == null)
        //        {
        //            continue;
        //        }
        //        try
        //        {
        //            entry.UpdateFile();
        //        }
        //        catch (Exception t)
        //        {
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Exception, entry.FileName, t.Message));
        //        }
        //        if (_pausing)
        //        {
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Paused, null, null));
        //            while (_pausing && !_aborting)
        //            {
        //                Thread.Sleep(100);
        //            }
        //        }
        //        if (_aborting)
        //        {
        //            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.Aborted, null, null));
        //            break;
        //        }
        //    }
        //}

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

        //private bool IsWritable(string path)
        //{
        //    if (!File.Exists(path))
        //    {
        //        return true;
        //    }
        //    try
        //    {
        //        using FileStream stream = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); return true;
        //    }
        //    catch (IOException)
        //    {
        //        return false;
        //    }
        //}

        //public void WaitUnlocked(string filename)
        //{
        //    if (string.IsNullOrEmpty(filename))
        //    {
        //        return;
        //    }
        //    string path = Path.Combine(DestinationDirectory, filename);
        //    if (IsWritable(path))
        //    {
        //        return;
        //    }
        //    OnLog(new LogEventArgs(LogStatus.Information, LogCategory.WaitLocked, filename, null));
        //    while (!IsWritable(path))
        //    {
        //        Thread.Sleep(100);
        //    }
        //}

        internal FileList(string baseDir, string checksumPath)
        {
            BaseDirectory = baseDir;
            ChecksumFileName = checksumPath;
            int line = 1;
            using (StreamReader reader = new(checksumPath, FileEncoding))
            {
                while (!reader.EndOfStream)
                {
                    string? s = reader.ReadLine();
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
                            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.InvalidChecksumEntry, checksumPath, line.ToString()));
                            break;
                    }
                }
            }
            InvalidateNameToEntry();
            SHA1 sha = SHA1.Create();
            using FileStream stream = new(checksumPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Checksum = sha.ComputeHash(stream);
        }
        
        private static bool IsIgnored(string fullPath, string relPath, List<Regex> ignoreNames, bool ignoreHidden)
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
            StringBuilder buf = new();
            buf.Append('^');
            foreach (char c in wildcard)
            {
                switch (c)
                {
                    case '*':
                        buf.Append(".*");
                        break;
                    case '?':
                        buf.Append('.');
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
            ChecksumFileName = null;
            BaseDirectory = baseDir;
            List<Regex> ignores = new(ignoreNames.Count);
            foreach (string s in ignoreNames)
            {
                ignores.Add(ToRegex(s));
            }
            ignores.Add(ToRegex(CHECKSUM_FILE));
            EnumFiles(baseDir, ignores, ignoreHidden);
            _list.Sort();
            Checksum = [];
        }

        public static FileList CreateByDirectory(string directory, IList<string> ignoreNames, bool ignodeHidden, string? compressDirectory)
        {
            return new FileList(directory, ignoreNames, ignodeHidden)
            {
                CompressedDirectory = compressDirectory,
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
