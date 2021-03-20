using System.Collections.Generic;
using System.Text;

namespace Opal.Logging
{
    /// <summary>
    /// Writes log items to console
    /// </summary>
    public static class LogItems
    {
        /// <summary>
        /// Writes collection of log items to console
        /// </summary>
        /// <param name="log">Console</param>
        /// <param name="logItems">Log item collection</param>
        public static void Write(this ConsoleLog log, 
            IEnumerable<LogItem> logItems)
        {
            foreach (var item in logItems)
            {
                switch (item.Level)
                {
                    case LogLevel.Error:
                    case LogLevel.Warning:
                        log.WriteError(item);
                        break;
                    case LogLevel.Info:
                        log.InfoLine(item.Message);
                        break;
                }
            }
        }

        /// <summary>
        /// Writes error or warning to console, with corresponding line and 
        /// suggestions if available
        /// </summary>
        /// <param name="log">Console</param>
        /// <param name="item">Error log item</param>
        public static void WriteError(this ConsoleLog log, 
            LogItem item)
        {
            var builder = new StringBuilder("     ");
            for (var i = 0; i < item.Start.Col - 1; i++)
            {
                if (item.Line[i] == '\t')
                    builder.Append('\t');
                else
                    builder.Append(' ');
            }
            builder.Append('^');

            log.NewLine()
                .LogLine(item.Level, item.Message)
                .NewLine()
                .Info(string.Format("{0,4}| ", item.Start.Ln))
                .InfoLine(item.Line)
                .LogLine(item.Level, builder.ToString())
                .NewLine();

            if (item.Suggestions != null)
                log.NormalLine(item.Suggestions)
                    .NewLine();
        }

        private static ConsoleLog LogLine(this ConsoleLog log, 
            LogLevel level, 
            string msg)
        {
            switch (level)
            {
                case LogLevel.Error: log.ErrorLine(msg); break;
                case LogLevel.Warning: log.WarnLine(msg); break;
                case LogLevel.Info: log.InfoLine(msg); break;
            }
            return log;
        }
    }
}
