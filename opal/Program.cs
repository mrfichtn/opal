using System;

namespace Opal
{
    class Program
    {
        static int Main(string[] args)
        {
            bool isOk;
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Missing file");
                    return -1;
                }
                var file = args[0];
                var logger = new ConsoleLogger(file);
                var parserGen = new Compiler(logger, args[0]);
                if (args.Length > 1)
                    parserGen.OutPath = args[1];

                isOk = parserGen.Compile();

                var log = new Logger();
                foreach (var item in parserGen.Log)
                {
                    switch (item.Level)
                    {
                        case LogLevel.Error:
                            log .NewLine()
                                .InfoLine(item.Message)
                                .NewLine()
                                .Info(string.Format("{0,4}| ", item.Token.Start.Ln))
                                //.Info(item.Line.Substring(0,
                                //item.Token.Start.Col - 1))
                                //.Error(item.Token.Value)
                                //.InfoLine(item.Line.Substring(item.Token.End.Col))
                                .InfoLine(item.Line)
                                .ErrorLine("     {0}^",
                                    new string(' ', item.Token.Start.Col)
                                    //new string('^', item.Token.Length)
                                    )
                                .NewLine();

                            if (item.Suggestions != null)
                                log.InfoLine(item.Suggestions)
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
