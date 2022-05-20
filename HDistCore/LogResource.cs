using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDistCore
{
    public enum LogStatus
    {
        Information,
        Warning,
        Error,
        Progress
    }
    public enum LogCategory
    {
        Exception,
        Copy,
        CopyCompressed,
        FailToExtract,
        Compressing,
        NoMessage,
        WaitLocked,
        Aborted,
        Paused,
        Resumed,
        Finished,
        SuppressUpdating,
    }
    public class LogEventArgs : EventArgs
    {
        public LogStatus Status { get; }
        public LogCategory Category { get; }
        public string FileName { get; }
        public string Message { get; }
        public LogEventArgs(LogStatus status, LogCategory category, string filename, string message)
        {
            Status = status;
            Category = category;
            FileName = filename;
            Message = message;
        }
    }

    public static class LogResource
    {
        private static Dictionary<LogCategory, string> CategoryToMessage = new Dictionary<LogCategory, string>()
        {
            { LogCategory.Exception, Properties.Resources.LogFormatException },
            { LogCategory.Copy, Properties.Resources.LogFormatCopy },
            { LogCategory.CopyCompressed, Properties.Resources.LogFormatCopyCompressed },
            { LogCategory.FailToExtract, Properties.Resources.LogFormatFailToExtract },
            { LogCategory.Compressing, Properties.Resources.LogFormatCopyCompressed },
            { LogCategory.NoMessage, Properties.Resources.LogFormatNoMessage },
            { LogCategory.WaitLocked, Properties.Resources.LogFormatWaitLocked },
            { LogCategory.Aborted, Properties.Resources.LogFormatAborted },
            { LogCategory.Paused, Properties.Resources.LogFormatPaused },
            { LogCategory.Resumed, Properties.Resources.LogFormatResumed },
            { LogCategory.Finished, Properties.Resources.LogFormatFinished },
            { LogCategory.SuppressUpdating, Properties.Resources.LogFormatSuppressUpdating },
        };

        public static string GetMessageFormat(LogCategory category)
        {
            string fmt;
            if (!CategoryToMessage.TryGetValue(category, out fmt))
            {
                return "{0}: {1}";
            }
            return fmt;
        }
        public static string GetMessage(LogCategory category, string filename, string message)
        {
            string fmt = GetMessageFormat(category);
            return string.Format(fmt, filename, message);
        }
    }
}
