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

        #region OutPath Property
        public string OutPath
        {
            get => outPath;
            set => outPath = value ?? throw new ArgumentNullException(nameof(OutPath));
        }
        private string outPath;
        #endregion

        public string? Namespace { get; set; }

        #region ParserFrame Property
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
        #endregion

        #region Log Property
        public IEnumerable<LogItem> Log => parserLogger ?? Enumerable.Empty<LogItem>();
        #endregion

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
                logger, parser.Logger, lang, out var scannerWriter))
                return false;

            var grammar = lang.BuildGrammar(parser.Logger, 
                scannerWriter!.Symbols,
                options);

            if (grammar == null)
                return false;

            logger.LogMessage(Importance.Normal, "Building LR1");
            var lr1 = new LR1.Grammar(grammar);
            WriteGrammar(lr1, options);

            var lr1Parser = new LR1.LR1Parser(logger, lr1, lang.Conflicts);
            WriteLr1States(lr1Parser, options);

            WriteParser(lang,
                grammar,
                lr1Parser,
                scannerWriter,
                fileDir);

            return isOk;
        }

        /// <summary>
        /// If lr1.grammar is set to a file path, this method will write out 
        /// a clean copy of the grammar (i.e without attributes / actions)
        /// </summary>
        private static void WriteGrammar(LR1.Grammar lr1,
            Options options)
        {
            if (options.TryGet("lr1.grammar", out var filePath))
            {
                var grammarText = lr1.ToString();
                File.WriteAllText(filePath!, grammarText);
            }
        }

        /// <summary>
        /// If lr1.states is set to a file path, this method will write out 
        /// the lr(1) states prior to generating the action table
        /// </summary>
        private static void WriteLr1States(LR1.LR1Parser lr1Parser,
            Options options)
        {
            if (options.TryGet("lr1.states", out var filePath))
                File.WriteAllText(filePath!, lr1Parser.States.ToString());
        }

        private void WriteParser(ParseTree.Language lang, 
            Productions.Grammar grammar,
            LR1.LR1Parser lr1Parser,
            ScannerBuilder.ScannerWriter scanner,
            string fileDir)
        {
            using var csharp = new Generator(OutPath);
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

            logger.LogMessage(Importance.Normal, "Generating parser file {0}", OutPath);
            var templContext = new ParserTemplateContext(options,
                Namespace,
                lang,
                grammar,
                lr1Parser,
                scanner);

            if (frameSpecified)
            {
                logger.LogMessage(Importance.Normal, "Using frame file {0}", frameFile!);
                TemplateProcessor2.FromFile(csharp, templContext, frameFile);
            }
            else
            {
                var internalFrameFile = options.HasOption("no.lib", false) ?
                    "Opal.FrameFiles.Parser.txt" :
                    "Opal.FrameFiles.LibParser.txt";
                
                logger.LogMessage(Importance.Normal, "Using internal frame file");
                TemplateProcessor2.FromAssembly(csharp, 
                    templContext,
                    internalFrameFile);
            }
        }
    }
}
