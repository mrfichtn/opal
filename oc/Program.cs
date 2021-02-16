using Opal.Exceptions;
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
            var log = new ConsoleLog();
            log.NewLine();
            log.NormalLine("opal");
            log.NormalLine("--------------------------------------");

            try
            {
                if (args.Length == 0)
                    throw new ErrorException("Nothing to compile: please specify a file");
                
                var file = args[0];
                var logger = new ConsoleLogger(log, file);

                if (!File.Exists(file))
                    throw new ErrorException($"Cannot find file '{file}'", -2);

                var parserGen = new Compiler(logger, args[0]);
                if (args.Length > 1)
                    parserGen.OutPath = args[1];

                isOk = parserGen.Compile();
                log.Write(parserGen.Log);
            }
            catch (ErrorException ex)
            {
                log.ErrorLine(ex.Message);
                return ex.ExitCode;
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
