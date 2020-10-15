using System;
using System.Text;

namespace Opal
{
    public class ConsoleLogger : ILogger
	{
		private readonly string _file;
		private readonly ConsoleColor _oldColor;

		public ConsoleLogger(string file)
		{
			_file = file;
			_oldColor = Console.ForegroundColor;
		}

		public void LogError(string message, params object[] messageArgs) =>
			Log(ConsoleColor.Red, message, messageArgs);

		public void LogError(Segment segment, string message, params object[] messageArgs) =>
			Log(ConsoleColor.Red, segment, message, messageArgs);

		public void LogMessage(string message, params object[] messageArgs) =>
			LogMessage(Importance.Low, message, messageArgs);

		public void LogMessage(Importance importance, 
			string message, 
			params object[] messageArgs) =>
			Log(ToColor(importance), message, messageArgs);

		public void LogMessage(Importance importance, 
			Segment segment, 
			string message, 
			params object[] messageArgs) =>
			Log(ToColor(importance), segment, message, messageArgs);

		public void LogWarning(Segment segment, 
			string message, 
			params object[] messageArgs) =>
			Log(ConsoleColor.Yellow, segment, message, messageArgs);

		public void LogWarning(string message, params object[] messageArgs) =>
			Log(ConsoleColor.Yellow, message, messageArgs);

		public void Log(ConsoleColor color, 
			Segment segment, 
			string message, 
			params object[] messageArgs)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrEmpty(_file))
				builder.Append(_file);
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
			Log(color, builder.ToString());
		}

		public void Log(ConsoleColor color, 
			string message, 
			params object[] messageArgs)
		{
			if (messageArgs == null || messageArgs.Length == 0)
				Log(color, message);
			else
				Log(color, string.Format(message, messageArgs));
		}

		public void Log(ConsoleColor color, string message)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = _oldColor;
		}

		private static ConsoleColor ToColor(Importance importance)
		{
			ConsoleColor color;
			switch (importance)
			{
				case Importance.High: color = ConsoleColor.White; break;
				case Importance.Normal: color = ConsoleColor.Gray; break;
				default: color = ConsoleColor.DarkGray; break;
			}
			return color;
		}
	}

}
