using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BuildSum
{
    public partial class FileList: IEnumerable<FileList.FileEntry>
    {
        private const string INDEX_FILE = "_index.sha";
        private static bool IsCaseInsensitiveOS()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
        private static StringComparison GetFileNameComparison(bool ignoreCase)
        {
            return ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }
        private static StringComparison FileNameComparison = GetFileNameComparison(IsCaseInsensitiveOS());
        private static bool _ignoreCase = IsCaseInsensitiveOS();
        //public static bool IgnoreCase
        //{
        //    get
        //    {
        //        return _ignoreCase;
        //    }
        //    set
        //    {
        //        _ignoreCase = value;
        //        FileNameComparison = GetFileNameComparison(_ignoreCase);
        //    }
        //}

        private static string Treat(string value)
        {
            return (_ignoreCase && !string.IsNullOrEmpty(value)) ? value.ToLower() : value;
        }

        public Encoding FileEncoding { get; set; } = Encoding.UTF8;
        public string BaseDirectory { get; set; }
        public string? CompressedDirectory { get; set; }

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

        public void SaveIndex()
        {
            string path = Path.Combine(BaseDirectory, INDEX_FILE);
            SaveIndexToFile(path);
        }

        public void SaveIndexToFile(string filename)
        {
            string path = Path.GetTempFileName();
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                SaveIndexToStream(stream);
            }
            File.Delete(filename);
            File.Move(path, filename);
            FileAttributes attr = File.GetAttributes(filename);
            File.SetAttributes(filename, attr | FileAttributes.Hidden);
        }
        public void SaveIndexToStream(Stream stream)
        {
            OnLog(new LogEventArgs(LogStatus.Information, LogCategory.UpdatingChecksum, null, null));
            using StreamWriter writer = new(stream, FileEncoding);
            foreach (FileEntry entry in _list)
            {
                OnLog(new LogEventArgs(LogStatus.Information, LogCategory.NoMessage, entry.FileName, null));
                entry.Write(writer);
            }
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

        /// <summary>
        /// ワイルドカード表現を正規表現に変換
        /// </summary>
        /// <param name="wildcard"></param>
        /// <returns></returns>
        private static Regex ToRegex(string wildcard)
        {
            StringBuilder buf = new();
            buf.Append("(^|[/\\]");
            foreach (char c in wildcard)
            {
                switch (c)
                {
                    case '*':
                        buf.Append("[^/\\]*");
                        break;
                    case '?':
                        buf.Append("[^/\\]");
                        break;
                    case '$':
                    case '(':
                    case ')':
                    case '+':
                    case '.':
                    case '=':
                    case '[':
                    case '\\':
                    case '^':
                    case '{':
                    case '|':
                        buf.Append('\\');
                        buf.Append(c);
                        break;
                    default:
                        buf.Append(c);
                        break;
                }
            }
            buf.Append("($|[^/\\])");
            return new Regex(buf.ToString());
        }

        internal FileList(string baseDir, IList<string> ignoreNames, bool ignoreHidden, string? compressDir)
        {
            BaseDirectory = baseDir;
            List<Regex> ignores = new(ignoreNames.Count);
            foreach (string s in ignoreNames)
            {
                ignores.Add(ToRegex(s));
            }
            ignores.Add(ToRegex(INDEX_FILE));
            EnumFiles(baseDir, ignores, ignoreHidden);
            _list.Sort();
            CompressedDirectory = compressDir;
        }

        public static FileList CreateByDirectory(string directory, IList<string> ignoreNames, bool ignodeHidden, string? compressDirectory)
        {
            return new FileList(directory, ignoreNames, ignodeHidden, compressDirectory);
        }

        public void CompressFiles()
        {
            if (string.IsNullOrEmpty(CompressedDirectory))
            {
                return;
            }
            OnLog(new LogEventArgs(LogStatus.Information, LogCategory.Compressing, null, null));
            if (!Directory.Exists(CompressedDirectory))
            {
                Directory.CreateDirectory(CompressedDirectory);
            }
            foreach (FileEntry entry in _list)
            {
                OnLog(new LogEventArgs(LogStatus.Information, LogCategory.NoMessage, entry.FileName, null));
                entry.CompressFile();
            }
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
