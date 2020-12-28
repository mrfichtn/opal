using Generators;
using Opal.Containers;
using Opal.Dfa;
using Opal.ParseTree;
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
        private Parser? parser;
        private Dfa.Dfa? dfa;
        private LR1.LR1Parser? lr1Parser;
        private readonly Options options;
        private IGeneratable? scannerWriter;

        private Productions.Grammar grammar;
        
        private bool emitStateScanner;
        private bool compressScanner;

        public Compiler(ILogger logger, string inPath)
        {
            options = new Options();
            this.logger = logger;
            this.inPath = inPath;
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

        public IEnumerable<LogItem> Log => 
            (parser != null) ? parser.Logger : Enumerable.Empty<LogItem>();

        public bool Compile()
        {
            Console.OutputEncoding = Encoding.Unicode;
            var fileName = Path.GetFileName(inPath);
            var fileDir = Path.GetDirectoryName(fileName) ?? ".";
            parser = new Parser(inPath);
            logger.LogMessage(Importance.Normal, "Parsing {0}", fileName);
            var isOk = parser.Parse();
            if (!isOk)
                return isOk;

            var lang = parser.Language;
            if (lang == null)
            {
                logger.LogError("Failed to return language element from root");
                return false;
            }

            lang.MergeOptions(options);
            var nfa = lang.BuildGraph(parser.Logger, options, inPath);
            grammar = lang.BuildGrammar(parser.Logger, nfa);

            //if (!prods.SetStates(logger, parser.Graph.Machine.AcceptingStates))
            //    return false;

            if (options.TryGet("nfa", out var nfaPath))
            {
                if (string.IsNullOrEmpty(nfaPath))
                    nfaPath = Path.ChangeExtension(inPath, ".nfa.txt");
                File.WriteAllText(nfaPath, nfa.ToString());
            }

            logger.LogMessage(Importance.Normal, "Building dfa");
            dfa = nfa.ToDfa();

            emitStateScanner = (!options.TryGet("scanner", out var scannerValue) ||
                    scannerValue!.Equals("state", StringComparison.InvariantCultureIgnoreCase));

            compressScanner = options.HasOption("scanner.compress") ?? true;
            var syntaxErrorTokens = options.HasOption("syntax.error.tokens") ?? true;

            scannerWriter = emitStateScanner ?
                    new DfaStateWriter(dfa, compressScanner, syntaxErrorTokens) as IGeneratable :
                    new DfaSwitchWriter(dfa, syntaxErrorTokens);


            logger.LogMessage(Importance.Normal, "Building LR1");
            
            
            var lr1 = new LR1.Grammar(grammar);
            var grammarPath = Path.ChangeExtension(inPath, ".grammar.txt");
            var grammarText = lr1.ToString();
            File.WriteAllText(grammarPath, grammarText);

            lr1Parser = new LR1.LR1Parser(logger, lr1, lang.Conflicts);

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
                    scannerWriter,
                    dfa);
                TemplateProcessor2.FromAssembly(csharp, templContext, frameFile);
            }

            return isOk;
        }
    }
}
