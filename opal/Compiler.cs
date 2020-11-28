using Generators;
using Opal.Containers;
using Opal.Dfa;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Opal
{
    public sealed class Compiler : ITemplateContext
    {
        private readonly ILogger logger;
        private readonly string inPath;
        private Parser? parser;
        private Dfa.Dfa? dfa;
        private ProductionList? prods;
        private LR1.LR1Parser? lr1Parser;
        private readonly Dictionary<string, object> options;
        private IGeneratable? scannerWriter;
        
        private bool emitTokenStates;
        private bool emitStateScanner;

        public Compiler(ILogger logger, string inPath)
        {
            options = new Dictionary<string, object>();
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
                options.TryGetOption("frame", out var value);
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
            (parser != null) ? parser.Log : Enumerable.Empty<LogItem>();

        public bool Compile()
        {
            Console.OutputEncoding = Encoding.Unicode;
            var fileName = Path.GetFileName(inPath);
            var fileDir = Path.GetDirectoryName(fileName);
            parser = new Parser(logger, inPath);
            parser.SetOptions(options);
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
            prods = lang.Productions;

            var nfa = parser.Graph;
            if (!prods.SetStates(logger, parser.Graph.Machine.AcceptingStates))
                return false;

            if (options.TryGetOption("nfa", out var nfaPath))
            {
                if (string.IsNullOrEmpty(nfaPath))
                    nfaPath = Path.ChangeExtension(inPath, ".nfa.txt");
                File.WriteAllText(nfaPath, nfa.ToString());
            }

            logger.LogMessage(Importance.Normal, "Building dfa");
            dfa = nfa.ToDfa();

            emitStateScanner = (!options.TryGetOption("scanner", out var scannerValue) ||
                    scannerValue!.Equals("state", StringComparison.InvariantCultureIgnoreCase));

            scannerWriter = emitStateScanner ?
                    new DfaStateWriter(dfa) as IGeneratable :
                    new DfaSwitchWriter(dfa);


            logger.LogMessage(Importance.Normal, "Building LR1");
            var grammar = new LR1.Grammar(lang.Productions);
            var grammarPath = Path.ChangeExtension(inPath, ".grammar.txt");
            var grammarText = grammar.ToString();
            File.WriteAllText(grammarPath, grammarText);

            lr1Parser = new LR1.LR1Parser(logger, grammar, lang.Conflicts);

            var statesPath = Path.ChangeExtension(inPath, ".states.txt");
            File.WriteAllText(statesPath, lr1Parser.States.ToString());

            emitTokenStates = !emitStateScanner || options.HasOption("emit_token_states");

            using (var csharp = new Generator(OutPath))
            {
                if (!string.IsNullOrEmpty(Namespace))
                    lang.Namespace = new ParseTree.Identifier(Namespace!);

                var frameSpecified = false;
                if (options.TryGetOption("frame", out var frameFile) && 
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
                    logger.LogMessage(Importance.Normal, "Writing output {0}", OutPath);
                    TemplateProcessor.FromFile(csharp, this, frameFile!);
                }
                else
                {
                    logger.LogMessage(Importance.Normal, "Using internal frame file");
                    logger.LogMessage(Importance.Normal, "Writing output {0}", OutPath);
                    TemplateProcessor.FromAssembly(csharp, this, "Opal.FrameFiles.Parser.txt");
                }
            }

            return isOk;
        }

        bool ITemplateContext.Condition(string varName) => options.HasOption(varName);

        string? ITemplateContext.Include(string name)
        {
            return string.Empty;
        }

        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            bool result = true;
            switch (varName)
            {
                case "usings":              generator.Write(parser!.Usings);    break;
                case "namespace.start":
                    if (parser!.Language!.Namespace != null)
                    {
                        generator.WriteLine("namespace {0}", parser.Language.Namespace)
                            .Write("{")
                            .Indent();
                    }
                    break;
                case "productions":
                    generator.Indent(2);
                    options.TryGetOption("no_action", out var noAction);
                    if (prods != null)
                        prods.Write(generator, noAction!);
                    generator.UnIndent(2);
                    break;
                case "actions":                 lr1Parser!.Actions.Write(generator); break;
                case "parser.symbols":          lr1Parser!.WriteSymbols(generator); break;
                case "scanner":                 scannerWriter!.Write(generator); break;
                case "scanner.states":          dfa!.WriteTokenEnum(generator, emitTokenStates); break;
                case "namespace.end":
                    if (parser!.Language!.Namespace != null)
                        generator.EndBlock();
                    break;
                case "namespace":               generator.Write(parser!.Language!.Namespace); break;
                default:
                    if (options.TryGetOption(varName, out var value))
                        generator.Write(value!);
                    else
                        result = false;
                    break;
            }
            return result;
        }
    }
}
