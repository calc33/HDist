using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HDist.Core
{
    /// <summary>
    /// FileList.Run は hdist.dll をプログラムに組み込んで自己更新するためのAPI群
    /// </summary>
    partial class FileList
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationDirectory"></param>
        /// <returns></returns>
        //public bool HasModified(string destinationDirectory)
        //{

        //}
        /// <summary>
        /// 実行中のプログラムが参照しているexeやdllのうちどれかが更新されていればtrueを返す
        /// trueを返された場合、ファイルが更新できないので RunHCopy/RunHCopyW を呼び出した後
        /// 自身を終了させる必要がある。
        /// </summary>
        /// <param name="destinationDirectory"></param>
        /// <returns></returns>
        public bool HasModifiedInModule()
        {
            foreach (Module module in Process.GetCurrentProcess().Modules)
            {
                string path = module.Assembly.Location;
                string fname = Path.GetRelativePath(DestinationDirectory, path);
                FileEntry? entry = FindEntry(fname);
                if (entry != null && entry.IsModified(DestinationDirectory))
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly string[] PreCopyFiles = new string[] { "hcopy.exe", "HCopyW.exe", "hcopy.dll", "hcopy.deps.json", "hcopy.runtimeconfig.json", "hdist.dll", "ICSharpCode.SharpZipLib.dll" };
        private const string HCopy_exe = "hcopy.exe";
        private const string HCopyW_exe = "HCopyW.exe";

        private static string? GetExecuatblePath(string destinationDirectory, string filename)
        {
            string path = Path.Combine(destinationDirectory, filename);
            if (File.Exists(path))
            {
                return path;
            }
            string[] paths = Environment.ExpandEnvironmentVariables("%PATH%").Split(';');
            foreach (string dir in paths)
            {
                path = Path.Combine(dir, filename);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        private static string _Q(string value)
        {
            if (value.StartsWith('"'))
            {
                return value;
            }
            StringBuilder buf = new();
            buf.Append('"');
            foreach (char c in value)
            {
                if (c == '"')
                {
                    buf.Append('"');
                }
                buf.Append(c);
            }
            buf.Append('"');
            return buf.ToString();
        }

        private async Task RunProcessAsync(string hcopyExe, bool restart)
        {
            // hcopy.exe/HCopyW.exeと関連ファイルは事前にコピー
            await UpdateFilesAsync(PreCopyFiles);
            string? exe = GetExecuatblePath(DestinationDirectory, hcopyExe);
            if (string.IsNullOrEmpty(exe))
            {
                throw new FileNotFoundException(string.Format("{0}が見つかりません", hcopyExe));
            }
            Process p = Process.GetCurrentProcess();
            ProcessStartInfo info = p.StartInfo;
            if (p.MainModule == null)
            {
                throw new InvalidOperationException("MainModule is null");
            }
            string args = string.Format("{0} {1}", _Q(BaseUri), _Q(DestinationDirectory));
            if (restart)
            {
                args += string.Format(" --wait {0} --run {0} --param {1}", _Q(p.MainModule.FileName), _Q(info.Arguments));
            }
            Process.Start(exe, args);
        }

        /// <summary>
        /// hdist.dllをプログラムに組み込んで自己更新する。
        /// 更新処理に hcopy.exe を使う。
        /// 自己書き換えのために一旦プログラムを終了して再起動したい場合は restart=true に設定
        /// </summary>
        /// <param name="destinationDirectory"></param>
        /// <param name="restart"></param>
        public void RunHCopy(bool restart)
        {
            _ = RunProcessAsync(HCopy_exe, restart);
        }

        /// <summary>
        /// hdist.dllをプログラムに組み込んで自己更新する。
        /// 更新処理に HCopyW.exe を使う。
        /// 自己書き換えのために一旦プログラムを終了して再起動したい場合は restart=true に設定
        /// </summary>
        /// <param name="destinationDirectory"></param>
        /// <param name="restart"></param>
        public void RunHCopyW(bool restart)
        {
            _ = RunProcessAsync(HCopyW_exe, restart);
        }
    }
}
