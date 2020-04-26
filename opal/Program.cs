using Opal.Logging;

namespace Opal
{
    class Program
    {
        static int Main(string[] args)
        {
            var file = args[0];
            var logger = new ConsoleLogger(file);
            var parserGen = new Compiler(logger, args[0]);
            if (args.Length > 1)
                parserGen.OutPath = args[1];

            var isOk = parserGen.Compile();
            return isOk ? 0 : -1;
        }
    }
}
