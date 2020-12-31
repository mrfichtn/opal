using System;
using System.Text;

namespace Opal.Logging
{
    public class ConsoleLog
    {
        private readonly ConsoleColor oldColor;

		public ConsoleLog()
		{
			oldColor = Console.ForegroundColor;
			Console.OutputEncoding = Encoding.UTF8;
		}

		public ConsoleLog NewLine()
		{
			Console.WriteLine();
			return this;
		}
		
		public ConsoleLog ErrorLine(string message) =>
			LogLine(ConsoleColor.Red, message);

		public ConsoleLog ErrorLine(string fmt, params object[] args) =>
			ErrorLine(string.Format(fmt, args));

		public ConsoleLog Info(string msg) => Normal(msg);
		public ConsoleLog InfoLine(string msg) => LogLine(ConsoleColor.Gray, msg);

		public ConsoleLog WarningLine(string message) =>
			LogLine(ConsoleColor.Yellow, message);

		public ConsoleLog HighLine(string message) =>
			LogLine(ConsoleColor.White, message);

		public ConsoleLog Normal(string message) =>
			Log(ConsoleColor.Gray, message);

		public ConsoleLog NormalLine(string message) =>
			LogLine(ConsoleColor.Gray, message);

		public ConsoleLog LowLine(string message) =>
			LogLine(ConsoleColor.DarkGray, message);

		public ConsoleLog Log(ConsoleColor color, string msg)
		{
			Console.ForegroundColor = color;
			Console.Write(msg);
			Console.ForegroundColor = oldColor;
			return this;
		}


		public ConsoleLog LogLine(ConsoleColor color, string message)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = oldColor;
			return this;
		}
	}
}
