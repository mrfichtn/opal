using Opal.Logging;
using System;
using System.IO;
using System.Text;

namespace Opal
{
    class Program
    {
        static int Main(string[] args)
        {
            bool isOk;
            try
            {
                var log = new ConsoleLog();
                log.NewLine();
                log.NormalLine("opal");
                log.NormalLine("--------------------------------------");

                if (args.Length == 0)
                {
                    log.ErrorLine("Nothing to compile: please specify a file");
                    return -1;
                }
                var file = args[0];
                var logger = new ConsoleLogger(log, file);

                if (!File.Exists(file))
                {
                    logger.LogError($"Cannot find file '{file}'");
                    return -2;
                }

                var parserGen = new Compiler(logger, args[0]);
                if (args.Length > 1)
                    parserGen.OutPath = args[1];

                isOk = parserGen.Compile();

                foreach (var item in parserGen.Log)
                {
                    switch (item.Level)
                    {
                        case LogLevel.Error:
                            WriteError(log, item);

                            //_logger.LogError(token, "Unexpected token '{0}'", token.Value);
                            //_logger.LogError("[{0,4}]   {1}", token.Start.Ln, _scanner.Line(token.Start.Ln));
                            //_logger.LogError("        {0}{1}", 
                            //	new string(' ', token.Start.Col),
                            //	new string('^', token.Length));
                            break;
                        case LogLevel.Warning:
                            WriteWarning(log, item);
                            break;
                        case LogLevel.Info:
                            logger.LogMessage(Importance.Normal, item.Message);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                isOk = false;
            }
            return isOk ? 0 : -1;
        }

        private static void WriteError(ConsoleLog log, LogItem item)
        {
            var builder = new StringBuilder("     ");
            for (var i = 0; i < item.Start.Col-1; i++)
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

        private static void WriteWarning(ConsoleLog log, LogItem item)
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
