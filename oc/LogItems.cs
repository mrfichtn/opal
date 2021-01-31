using Opal.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal
{
    public static class LogItems
    {
        public static void Write(this ConsoleLog logger, IEnumerable<LogItem> logItems)
        {
            foreach (var item in logItems)
            {
                switch (item.Level)
                {
                    case LogLevel.Error:
                        logger.WriteError(item);
                        break;
                    case LogLevel.Warning:
                        logger.WriteWarning(item);
                        break;
                    case LogLevel.Info:
                        logger.InfoLine(item.Message);
                        break;
                }
            }
        }

        public static void WriteError(this ConsoleLog log, LogItem item)
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
                .ErrorLine(item.Message)
                .NewLine()
                .Info(string.Format("{0,4}| ", item.Start.Ln))
                .InfoLine(item.Line)
                .ErrorLine(builder.ToString())
                .NewLine();

            if (item.Suggestions != null)
                log.NormalLine(item.Suggestions)
                    .NewLine();
        }

        private static void WriteWarning(this ConsoleLog log, LogItem item)
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
                .WarningLine(item.Message)
                .NewLine()
                .Info(string.Format("{0,4}| ", item.Start.Ln))
                .InfoLine(item.Line)
                .WarningLine(builder.ToString())
                .NewLine();

            if (item.Suggestions != null)
                log.NormalLine(item.Suggestions)
                    .NewLine();
        }
    }
}
