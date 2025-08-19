using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace HDist.Core
{
	partial class FileList
	{
		private const int HResult_SHARING_VIOLATION = unchecked((int)0x80070020);
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

			public string? GetLocalChecksum(string directory)
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

			public bool IsModified(string directory)
			{
				return (Size != -1 && Size != GetSize(directory, FileName)) || Checksum != GetLocalChecksum(directory);
			}

			private const string CompressExtenstion = ".bz2";
			private async Task<bool> ExtractFileAsync(FileStream destStream)
			{
				if (!_owner.IsFileUri || string.IsNullOrEmpty(_owner.CompressedDirectory))
				{
					return false;
				}
				string dir = Path.Combine(_owner.BaseUri, _owner.CompressedDirectory);
				string src = Path.Combine(dir, FileName) + CompressExtenstion;
				if (!File.Exists(src))
				{
					return false;
				}
				OnLog(LogStatus.Information, LogCategory.CopyCompressed, null);
				await Task.Run(() =>
				{
					using (FileStream srcStream = new(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						string dest = Path.Combine(_owner.DestinationDirectory, FileName);
						//using FileStream destStream = new(dest, FileMode.Create, FileAccess.Write, FileShare.None);
						BZip2.Decompress(srcStream, destStream, true);
					}
				});
				bool mod = IsModified(_owner.DestinationDirectory);
				if (mod)
				{
					OnLog(LogStatus.Warning, LogCategory.FailToExtract, null);
				}
				return !mod;
			}

			private async Task CopyFileAsync(string src, string dest, FileStream destStream)
			{
				if (_owner.IsFileUri)
				{
					using (FileStream inStream = new(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						await inStream.CopyToAsync(destStream);
						//await Task.Run(() => File.Copy(src, dest, true));
						File.SetLastWriteTimeUtc(dest, File.GetLastWriteTime(src));
						//File.SetAttributes(dest, File.GetAttributes(src));
					}
					return;
				}
				HttpClient client = _owner.RequireClient();
				HttpResponseMessage response = await client.GetAsync(src);
				HttpContent content = response.Content;
				if (!response.IsSuccessStatusCode)
				{
					OnLog(LogStatus.Error, LogCategory.Copy, await content.ReadAsStringAsync());
					return;
				}
				using (Stream inStream = await content.ReadAsStreamAsync())
				{
					try
					{
						inStream.CopyTo(destStream);
					}
					catch (Exception t)
					{
						OnLog(LogStatus.Error, LogCategory.Exception, t.Message);
						return;
					}
					DateTimeOffset? lastModified = content.Headers.LastModified;
					if (lastModified.HasValue)
					{
						File.SetLastWriteTimeUtc(dest, lastModified.Value.UtcDateTime);
					}
				}
			}

			private static void CreateDirectoryForFile(string path)
			{
				if (string.IsNullOrEmpty(path))
				{
					return;
				}
				string? dir = Path.GetDirectoryName(path);
				if (string.IsNullOrEmpty(dir))
				{
					return;
				}
				if (Directory.Exists(dir))
				{
					return;
				}
				Directory.CreateDirectory(dir);
			}

			private async Task CopyFileAsync(string destDir)
			{
				string? src = _owner.GetFullUri(FileNameWeb);
				string? dest = Path.Combine(destDir, FileName);
				CreateDirectoryForFile(dest);
				using (FileStream destStream = new(dest, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					bool flag = false;
					try
					{
						flag = await ExtractFileAsync(destStream);
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
							await CopyFileAsync(src, dest, destStream);
						}
						catch (Exception t)
						{
							OnLog(LogStatus.Error, LogCategory.Exception, t.Message);
						}
					}
				}
			}

			public async Task UpdateFileAsync()
			{
				OnLog(LogStatus.Progress, LogCategory.NoMessage, null);
				if (!IsModified(_owner.DestinationDirectory))
				{
					return;
				}
				try
				{
					try
					{
						await CopyFileAsync(_owner.DestinationDirectory);
					}
					catch (IOException e) when (e.HResult == HResult_SHARING_VIOLATION)
					{
						await CopyFileAsync(_owner.TemporaryDirectory);
					}
				}
				catch (Exception e)
				{
					OnLog(LogStatus.Error, LogCategory.Exception, e.Message);
				}
				return;
			}

			private void OnLog(LogStatus status, LogCategory category, string? message)
			{
				_owner.OnLog(new LogEventArgs(status, category, FileName, message));
			}

			private static string NormalizedFileName(string baseDirectory, string filename)
			{
				string path = Path.DirectorySeparatorChar != '/' ? filename.Replace('/', Path.DirectorySeparatorChar) : filename;
				return Path.IsPathRooted(path) ? Path.GetRelativePath(baseDirectory, path) : path;
			}

			public FileEntry(FileList owner, string filename, string checksum, long size)
			{
				_owner = owner;
				FileName = NormalizedFileName(_owner.BaseUri, filename);
				Checksum = checksum;
				Size = size;
			}

			public int CompareTo(object? obj)
			{
				if (!(obj is FileEntry entry))
				{
					return -1;
				}
				return string.Compare(FileName, entry.FileName, FileNameComparison);
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