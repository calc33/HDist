using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildSum
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
        Compressing,
        NoMessage,
        //Aborted,
        //Paused,
        //Resumed,
        //Finished,
        //SuppressUpdating,
        UpdatingChecksum,
    }
    public class LogEventArgs(LogStatus status, LogCategory category, string? filename, string? message) : EventArgs
    {
        public LogStatus Status { get; } = status;
        public LogCategory Category { get; } = category;
        public string? FileName { get; } = filename;
        public string? Message { get; } = message;
    }

    public static class LogResource
    {
        private static readonly Dictionary<LogCategory, string> CategoryToMessage = new()
        {
            { LogCategory.Exception, Properties.Resources.LogFormatException },
            { LogCategory.Compressing, Properties.Resources.LogFormatCompressing },
            { LogCategory.NoMessage, Properties.Resources.LogFormatNoMessage },
            //{ LogCategory.Aborted, Properties.Resources.LogFormatAborted },
            //{ LogCategory.Paused, Properties.Resources.LogFormatPaused },
            //{ LogCategory.Resumed, Properties.Resources.LogFormatResumed },
            //{ LogCategory.Finished, Properties.Resources.LogFormatFinished },
            //{ LogCategory.SuppressUpdating, Properties.Resources.LogFormatSuppressUpdating },
            { LogCategory.UpdatingChecksum, Properties.Resources.LogFormatUpdatingChecksum },
        };

        public static string GetMessageFormat(LogCategory category)
        {
            if (!CategoryToMessage.TryGetValue(category, out string? fmt))
            {
                return "{0}: {1}";
            }
            return fmt;
        }
        public static string GetMessage(LogCategory category, string? filename, string? message)
        {
            string fmt = GetMessageFormat(category);
            return string.Format(fmt, filename, message);
        }
    }
}
