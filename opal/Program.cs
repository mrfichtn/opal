using Opal.Logging;
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
