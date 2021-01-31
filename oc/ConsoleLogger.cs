using Opal.Logging;
using System.Text;

namespace Opal
{
    public class ConsoleLogger : ILogger
	{
		private readonly ConsoleLog log;
		private readonly string file;

		public ConsoleLogger(ConsoleLog log, string file)
        {
			this.log = log;
			this.file = file;
        }
		
		public ConsoleLogger(string file)
			: this(new ConsoleLog(), file)
		{ }

		public void LogError(string message) =>
			log.ErrorLine(message);

		public void LogError(string message, params object[] messageArgs) =>
			LogError(string.Format(message, messageArgs));

		public void LogError(Segment segment, string message, params object[] messageArgs) =>
			LogError(Format(segment, message, messageArgs));

		public void LogMessage(string message, params object[] messageArgs) =>
			LogMessage(Importance.Low, message, messageArgs);

		public void LogMessage(Importance importance, string message)
		{
			switch (importance)
			{
				case Importance.High:
					log.HighLine(message);
					break;
				case Importance.Normal:
					log.NormalLine(message);
					break;
				case Importance.Low:
					log.LowLine(message);
					break;
			}
		}

		public void LogMessage(Importance importance,
			string message,
			params object[] messageArgs) =>
			LogMessage(importance, string.Format(message, messageArgs));

		public void LogMessage(Importance importance, 
			Segment segment, 
			string message, 
			params object[] messageArgs) =>
			LogMessage(importance, Format(segment, message, message));

		public void LogWarning(Segment segment, 
			string message, 
			params object[] messageArgs) =>
			LogWarning(Format(segment, message, messageArgs));

		public void LogWarning(string message) =>
			log.WarningLine(message);

		public void LogWarning(string message, params object[] messageArgs) =>
			log.WarningLine(string.Format(message, messageArgs));

		public string Format(Segment segment, 
			string message, 
			params object[] messageArgs)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrEmpty(file))
				builder.Append(file);
			if (segment.Start.Ln > 0)
			{
				builder.Append('(').Append(segment.Start.Ln);
				if (segment.Start.Col > 0)
				{
					builder.Append(',').Append(segment.Start.Col);
					if (segment.End.Ln > 0)
						builder.Append(',').Append(segment.End.Ln).Append(',').Append(segment.End.Col);
				}
				builder.Append(')');
			}
			if (builder.Length > 0)
				builder.Append(":\t");
			if (messageArgs == null || messageArgs.Length == 0)
				builder.Append(message);
			else
				builder.AppendFormat(message, messageArgs);
			return builder.ToString();
		}
	}
}
