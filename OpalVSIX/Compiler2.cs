using Generators;
using Opal.Containers;
using Opal.Dfa;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Opal
{
    public sealed class Compiler2 : IVarProvider
    {
        private readonly CompileArgs args;
        private readonly string inPath;
        private Parser parser;
        private Dfa.Dfa dfa;
        private ProductionList prods;
        private LR1.LR1Parser lr1Parser;
        private readonly Dictionary<string, string> options;

        private IGeneratable scannerWriter;

        public Compiler2(CompileArgs args)
        {
            this.args = args;
            inPath = args.InputContents;
            options = new Dictionary<string, string>();
        }

        public string ParserFrame
        {
            get
            {
                options.TryGetValue("frame", out var value);
                return value;
            }
            set { options["frame"] = value; }
        }

        public bool Compile()
        {
            var fileName = Path.GetFileName(inPath);
            var fileDir = Path.GetDirectoryName(fileName);
            parser = Parser.FromString(args.InputContents);
            parser.SetOptions(options);
            args.LogMessage(Importance.Normal, "Parsing {0}", fileName);
            var isOk = parser.Parse();
            if (!isOk)
                return isOk;

            var lang = parser.Language;
            if (lang == null)
            {
                return false;
            }
            prods = lang.Productions;

            var nfa = parser.Graph;
            if (!prods.SetStates(args, parser.Graph.Machine.AcceptingStates))
                return false;

            if (options.TryGetValue("nfa", out var nfaPath))
            {
                if (string.IsNullOrEmpty(nfaPath))
                    nfaPath = Path.ChangeExtension(inPath, ".nfa.txt");
                File.WriteAllText(nfaPath, nfa.ToString());
            }

            args.LogMessage(Importance.Normal, "Building dfa");
            dfa = nfa.ToDfa();

            scannerWriter = (!options.TryGetValue("scanner", out var scannerValue) ||
                    scannerValue.Equals("state", StringComparison.InvariantCultureIgnoreCase)) ?
                    new DfaStateWriter(dfa) as IGeneratable :
                    new DfaSwitchWriter(dfa);


            args.LogMessage(Importance.Normal, "Building LR1");
            var grammar = new LR1.Grammar(lang.Productions);
            var grammarPath = Path.ChangeExtension(inPath, ".grammar.txt");
            var grammarText = grammar.ToString();
            File.WriteAllText(grammarPath, grammarText);

            lr1Parser = new LR1.LR1Parser(args, grammar);

            var statesPath = Path.ChangeExtension(inPath, ".states.txt");
            File.WriteAllText(statesPath, lr1Parser.States.ToString());


            using (var csharp = new Generator(args.Output))
            {
                if (!string.IsNullOrEmpty(args.DefaultNamespace))
                    lang.Namespace = new ParseTree.Identifier(args.DefaultNamespace);

                var frameSpecified = false;
                if (options.TryGetValue("frame", out var frameFile) && !string.IsNullOrEmpty(frameFile))
                {
                    frameSpecified = File.Exists(frameFile);
                    if (!frameSpecified)
                    {
                        frameFile = Path.Combine(fileDir, frameFile);
                        frameSpecified = File.Exists(frameFile);
                        if (!frameSpecified)
                            args.LogWarning("Unable to find frame file {0}", frameFile);
                    }
                }
                if (frameSpecified)
                {
                    args.LogMessage(Importance.Normal, "Using frame file {0}", frameFile);
                    //args.LogMessage(Importance.Normal, "Writing output {0}", OutPath);
                    TemplateProcessor.FromFile(csharp, this, frameFile);
                }
                else
                {
                    args.LogMessage(Importance.Normal, "Using internal frame file");
                    //args.LogMessage(Importance.Normal, "Writing output {0}", OutPath);
                    TemplateProcessor.FromAssembly(csharp, this, "Opal.FrameFiles.Parser.txt");
                }
            }

            return isOk;
        }

        bool IVarProvider.AddVarValue(Generator generator, string varName)
        {
            bool result = true;
            switch (varName)
            {
                case "usings":              generator.Write(parser.Usings);    break;
                case "namespace.start":
                    if (parser.Language.Namespace != null)
                    {
                        generator.WriteLine("namespace {0}", parser.Language.Namespace)
                            .Write("{")
                            .Indent();
                    }
                    break;
                case "productions":
                    generator.Indent(2);
                    options.TryGetValue("no_action", out var noAction);
                    prods.Write(generator, noAction);
                    generator.UnIndent(2);
                    break;
                case "actions":                 lr1Parser.Actions.Write(generator); break;
                case "parser.symbols":          lr1Parser.WriteSymbols(generator); break;
                case "scanner":                 scannerWriter.Write(generator); break;
                case "scanner.states":          dfa.WriteTokenEnum(generator); break;
                case "namespace.end":
                    if (parser.Language.Namespace != null)
                        generator.EndBlock();
                    break;
                case "namespace":               generator.Write(parser.Language.Namespace); break;
                default:
                    if (options.TryGetValue(varName, out var value))
                        generator.Write(value);
                    else
                        result = false;
                    break;
            }
            return result;
        }
    }
}
