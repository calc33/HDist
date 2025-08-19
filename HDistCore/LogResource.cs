using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDist.Core
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
		InvalidChecksumEntry,
	}
	public class LogEventArgs : EventArgs
	{
		public LogStatus Status { get; }
		public LogCategory Category { get; }
		public string? FileName { get; }
		public string? Message { get; }
		public LogEventArgs(LogStatus status, LogCategory category, string? filename, string? message)
		{
			Status = status;
			Category = category;
			FileName = filename;
			Message = message;
		}
	}

	public static class LogResource
	{
		private static readonly Dictionary<LogCategory, string> CategoryToMessage = new()
		{
			{ LogCategory.Exception, Properties.Resources0.LogFormatException },
			{ LogCategory.Copy, Properties.Resources0.LogFormatCopy },
			{ LogCategory.CopyCompressed, Properties.Resources0.LogFormatCopyCompressed },
			{ LogCategory.FailToExtract, Properties.Resources0.LogFormatFailToExtract },
			{ LogCategory.Compressing, Properties.Resources0.LogFormatCopyCompressed },
			{ LogCategory.NoMessage, Properties.Resources0.LogFormatNoMessage },
			{ LogCategory.WaitLocked, Properties.Resources0.LogFormatWaitLocked },
			{ LogCategory.Aborted, Properties.Resources0.LogFormatAborted },
			{ LogCategory.Paused, Properties.Resources0.LogFormatPaused },
			{ LogCategory.Resumed, Properties.Resources0.LogFormatResumed },
			{ LogCategory.Finished, Properties.Resources0.LogFormatFinished },
			{ LogCategory.SuppressUpdating, Properties.Resources0.LogFormatSuppressUpdating },
			{ LogCategory.InvalidChecksumEntry, Properties.Resources0.LogFormatInvalidChecksumEntry },
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
