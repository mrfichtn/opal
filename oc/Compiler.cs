using Generators;
using Opal.Logging;
using Opal.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Opal
{
    public sealed class Compiler
    {
        private readonly ILogger logger;
        private readonly string inPath;
        private readonly Options options;
        private Logger? parserLogger;

        public Compiler(ILogger logger, string inPath)
        {
            this.logger = logger;
            this.inPath = inPath;
            options = new Options();
            outPath = Path.ChangeExtension(this.inPath, ".Parser.cs");
        }

        public string OutPath
        {
            get => outPath;
            set => outPath = value ?? throw new ArgumentNullException(nameof(OutPath));
        }
        private string outPath;

        public string? Namespace { get; set; }
        public string? ParserFrame
        {
            get
            {
                options.TryGet("frame", out var value);
                return value;
            }
            set 
            {
                if (value == null)
                    options.Remove("frame");
                else
                    options["frame"] = value; 
            }
        }

        public IEnumerable<LogItem> Log => parserLogger ?? Enumerable.Empty<LogItem>();

        public bool Compile()
        {
            Console.OutputEncoding = Encoding.Unicode;
            var fileName = Path.GetFileName(inPath);
            var fileDir = Path.GetDirectoryName(fileName) ?? ".";
            var parser = new Parser(inPath);
            parserLogger = parser.Logger;
            logger.LogMessage(Importance.Normal, "Parsing {0}", fileName);
            var isOk = parser.TryParse(out var lang);
            if (!isOk)
                return isOk;
            
            if (lang == null)
            {
                logger.LogError("Failed to return language element from root");
                return false;
            }

            lang.MergeTo(options);

            if (!new ScannerBuilder(options).TryBuild(
                logger, parser.Logger, lang, out var scanner))
                return false;

            var grammar = lang.BuildGrammar(parser.Logger, 
                scanner!.Symbols,
                options);

            if (grammar == null)
                return false;

            logger.LogMessage(Importance.Normal, "Building LR1");
            
            var lr1 = new LR1.Grammar(grammar);
            var grammarPath = Path.ChangeExtension(inPath, ".grammar.txt");
            var grammarText = lr1.ToString();
            File.WriteAllText(grammarPath, grammarText);

            var lr1Parser = new LR1.LR1Parser(logger, lr1, lang.Conflicts);

            var statesPath = Path.ChangeExtension(inPath, ".states.txt");
            File.WriteAllText(statesPath, lr1Parser.States.ToString());

            using (var csharp = new Generator(OutPath))
            {
                if (!string.IsNullOrEmpty(Namespace))
                    lang.Namespace = new ParseTree.Identifier(Namespace!);

                var frameSpecified = false;
                if (options.TryGet("frame", out var frameFile) && 
                    !string.IsNullOrEmpty(frameFile))
                {
                    frameSpecified = File.Exists(frameFile);
                    if (!frameSpecified)
                    {
                        frameFile = Path.Combine(fileDir, frameFile);
                        frameSpecified = File.Exists(frameFile);
                        if (!frameSpecified)
                            logger.LogWarning("Unable to find frame file {0}", frameFile);
                    }
                }
                
                if (frameSpecified)
                {
                    logger.LogMessage(Importance.Normal, "Using frame file {0}", frameFile!);
                }
                else
                {
                    logger.LogMessage(Importance.Normal, "Using internal frame file");
                    frameFile = "Opal.FrameFiles.Parser.txt";
                }
                logger.LogMessage(Importance.Normal, "Writing output {0}", OutPath);

                var templContext = new ParserTemplateContext(options,
                    Namespace,
                    lang,
                    grammar,
                    lr1Parser,
                    scanner);
                TemplateProcessor2.FromAssembly(csharp, templContext, frameFile);
            }

            return isOk;
        }
    }
}
