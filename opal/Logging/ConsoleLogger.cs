using System;
using System.Text;

namespace Opal.Logging
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

        public void LogCommandLine(string commandLine)
        {
            LogCommandLine(Importance.Low, commandLine);
        }

        public void LogCommandLine(Importance importance, string commandLine)
        {
            Log(importance, commandLine);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            Log(ConsoleColor.Red, message, messageArgs);
        }

        public void LogError(string subcategory, string errorCode, string helpKeyword, int lineNumber, 
            int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            Log(ConsoleColor.Red, subcategory, errorCode, helpKeyword, lineNumber, columnNumber, 
                endLineNumber, endColumnNumber, message, messageArgs);
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            LogMessage(Importance.Low, message, messageArgs);
        }

        public void LogMessage(Importance importance, string message, params object[] messageArgs)
        {
            if ((messageArgs == null) || (messageArgs.Length == 0))
                Log(importance, message);
            else
                Log(importance, string.Format(message, messageArgs));
        }

        public void LogWarning(string subcategory, string warningCode, string helpKeyword, int lineNumber, 
            int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            Log(ConsoleColor.Yellow, subcategory, warningCode, helpKeyword, lineNumber, columnNumber,
                endLineNumber, endColumnNumber, message, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            Log(ConsoleColor.Yellow, message, messageArgs);
        }

        private void Log(ConsoleColor color, string subcategory, string code, string helpKeyword, int lineNumber, int columnNumber, 
            int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(_file))
                builder.Append(_file);
            if (lineNumber > 0)
            {
                builder.Append('(').Append(lineNumber);
                if (columnNumber > 0)
                {
                    builder.Append(',').Append(columnNumber);
                    if (endLineNumber > 0)
                    {
                        builder.Append(',').Append(endLineNumber);
                        if (endColumnNumber > 0)
                            builder.Append(',').Append(endColumnNumber);
                    }
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

        private void Log(Importance importance, string message)
        {
            ConsoleColor color;
            switch (importance)
            {
                case Importance.High:
                    color = ConsoleColor.White;
                    break;
                case Importance.Normal:
                    color = ConsoleColor.Gray;
                    break;
                case Importance.Low:
                default:
                    color = ConsoleColor.DarkGray;
                    break;
            }
            Log(color, message);
        }

        private void Log(ConsoleColor color, string message, params object[] messageArgs)
        {
            if (messageArgs == null || messageArgs.Length == 0)
                Log(color, message);
            else
                Log(color, string.Format(message, messageArgs));
        }

        private void Log(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = _oldColor;
        }
    }
}
