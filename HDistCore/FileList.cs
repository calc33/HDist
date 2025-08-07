using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HDist.Core
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

        public string ChecksumFileName { get; set; }

        private Uri _baseUri;
        public bool IsFileUri { get => _baseUri.IsFile; }
        public string BaseUri
        {
            get { return _baseUri.AbsoluteUri; }
            set { _baseUri = new Uri(value, UriKind.Absolute); }
        }
        public string? CompressedDirectory { get; set; }
        public string DestinationDirectory { get; set; }

        internal List<string> _requestHeaders = new();

        public void AddRequestHeader(string header)
        {
            _requestHeaders.Add(header);
        }

        public void AddRequestHeaders(IEnumerable<string> headers)
        {
            _requestHeaders.AddRange(headers);
        }

        private byte[]? _checksum;
        /// <summary>
        /// _checksum.sha のSHA1チェックサム値
        /// </summary>
        public byte[] Checksum { get { return _checksum ?? throw new ApplicationException("Checksum is not initialized."); } }
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

        public static bool IsDisabled(string destinationDirectory)
        {
            string path = Path.Combine(destinationDirectory, UPDATED_FILE);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }
            using StreamReader reader = new(path, Encoding.UTF8);
            string s = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(s))
            {
                s = s.TrimEnd();
            }
            return (s == "-" || s == "noupdate");
        }

        public async Task UpdateFilesAsync()
        {
            bool success = false;
            _aborting = false;
            _pausing = false;
            foreach (FileEntry entry in _list)
            {
                try
                {
                    await entry.UpdateFileAsync();
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
                    using StreamWriter writer = new(path, false, Encoding.UTF8);
                    writer.Write(Convert.ToBase64String(Checksum));
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

        public async Task UpdateFilesAsync(string[] files)
        {
            _aborting = false;
            _pausing = false;
            foreach (string s in files)
            {
                FileEntry? entry = FindEntry(s);
                if (entry == null)
                {
                    continue;
                }
                try
                {
                    await entry.UpdateFileAsync();
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

        private static bool IsWritable(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            try
            {
                using (FileStream stream = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void WaitUnlocked(string? filename)
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

        private static bool TrySplitHeader(string header, out string name, out string value)
        {
            int pos = header.IndexOf(':');
            if (pos < 0)
            {
                name = header;
                value = string.Empty;
                return false;
            }
            name = header.Substring(0, pos).Trim();
            value = header.Substring(pos + 1).Trim();
            return true;
        }

        private HttpClient? _httpClient;
        internal HttpClient RequireClient()
        {
            if (_httpClient == null)
            {
                HttpClientHandler handler = new()
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                _httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
                _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                foreach (string s in _requestHeaders)
                {
                    if (TrySplitHeader(s, out string name, out string value))
                    {
                        _httpClient.DefaultRequestHeaders.Add(name, value);
                    }
                }
            }
            return _httpClient;
        }

        private async Task<Stream> OpenChecksumStreamAsync()
        {
            Uri uri = new(_baseUri, ChecksumFileName);
            if (uri.IsFile)
            {
                return new FileStream(uri.AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            HttpClient client = RequireClient();
            HttpResponseMessage response = await client.GetAsync(uri);
            MemoryStream stream = new();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        private void LoadEntries(Stream checksumStream)
        {
            int line = 1;
            using (StreamReader reader = new(checksumStream, FileEncoding, leaveOpen: true))
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
                            OnLog(new LogEventArgs(LogStatus.Error, LogCategory.InvalidChecksumEntry, CHECKSUM_FILE, line.ToString()));
                            break;
                    }
                }
            }
            InvalidateNameToEntry();
            SHA1 sha = SHA1.Create();
            checksumStream.Position = 0;
            //using FileStream stream = new(ChecksumFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _checksum = sha.ComputeHash(checksumStream);
        }

        internal FileList(string baseUri, string destinationDir, string? compressDir, IEnumerable<string> requestHeaders)
        {
            _baseUri = new Uri(baseUri, UriKind.Absolute);
            ChecksumFileName = CHECKSUM_FILE;
            DestinationDirectory = destinationDir;
            CompressedDirectory = compressDir;
            AddRequestHeaders(requestHeaders);
        }

        public string GetFullUri(string filename)
        {
            return CombineUri(BaseUri, filename);
        }

        private static string CombineUri(string sourceUri, string filename)
        {
            if (string.IsNullOrEmpty(sourceUri))
            {
                return filename;
            }
            if (sourceUri.EndsWith('/'))
            {
                return sourceUri + filename;
            }
            return sourceUri + "/" + filename;
        }

        public async Task LoadEntriesAsync()
        {
            using (Stream stream = await OpenChecksumStreamAsync())
            {
                LoadEntries(stream);
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
