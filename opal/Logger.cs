using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal
{
    public class Logger
    {
        public Logger Error(string msg) => Log(ConsoleColor.Red, msg);
        public Logger ErrorLine(string msg) => LogLine(ConsoleColor.Red, msg);
        public Logger ErrorLine(string fmt, params object[] args) =>
            ErrorLine(string.Format(fmt, args));

        public Logger Info(string msg) => Log(ConsoleColor.Gray, msg);
        public Logger InfoLine(string msg) => LogLine(ConsoleColor.Gray, msg);

        public Logger NewLine()
        {
            Console.WriteLine();
            return this;
        }

        public Logger Log(LogLevel level, string msg)
        {
            var color = level switch
            {
                LogLevel.Error => ConsoleColor.DarkRed,
                LogLevel.Info => ConsoleColor.Gray,
                LogLevel.Warning => ConsoleColor.DarkYellow,
                _ => ConsoleColor.DarkGray
            };
            return Log(color, msg);
        }

        public Logger Log(ConsoleColor color, string msg)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ForegroundColor = old;
            return this;
        }

        public Logger LogLine(ConsoleColor color, string msg)
        {
            Log(color, msg);
            Console.WriteLine();
            return this;
        }

    }
}
