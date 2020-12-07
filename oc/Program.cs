using Opal.Logging;
using System;
using System.IO;

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
                            log.NewLine()
                                .ErrorLine(item.Message)
                                .NewLine()
                                .Info(string.Format("{0,4}| ", item.Token.Start.Ln))
                                .InfoLine(item.Line)
                                .Info(new string(' ', item.Token.Start.Col + 5))
                                .ErrorLine("^")
                                .NewLine();

                            if (item.Suggestions != null)
                                log.NormalLine(item.Suggestions)
                                    .NewLine();

                            //_logger.LogError(token, "Unexpected token '{0}'", token.Value);
                            //_logger.LogError("[{0,4}]   {1}", token.Start.Ln, _scanner.Line(token.Start.Ln));
                            //_logger.LogError("        {0}{1}", 
                            //	new string(' ', token.Start.Col),
                            //	new string('^', token.Length));
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
    }
}
