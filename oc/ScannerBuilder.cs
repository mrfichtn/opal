using Generators;
using Opal.Dfa;
using System.Collections.Generic;
using System.Linq;

namespace Opal
{
    public class ScannerBuilder
    {
        private readonly IScannerWriterFactory scannerWriterFactory;
        private readonly ITableWriterFactory tableWriterFactory;
        private readonly ISyntaxErrorHandler syntaxErrorHandler;
        private readonly TokenStateWriterBase tokenStateWriter;
        private readonly Nfa.INfaWriter nfaWriter;

        public ScannerBuilder(Options options)
        {
            nfaWriter = options.TryGet("nfa", out var nfaPath) ?
                new Nfa.NfaWriter(nfaPath) :
                new Nfa.NullNfaWriter();

            scannerWriterFactory = !options.Equals("scanner", "switch") ?
                new StateScannerWriterFactory() :
                new SwitchScannerWriterFactory();

            tableWriterFactory = options.HasOption("scanner.compress", true) ?
                new CompressedTableWriterFactory() :
                new UncompressedTableWriterFactory();

            syntaxErrorHandler = options.HasOption("syntax.error.tokens", true) ?
                new GreedySyntaxErrorHandler() :
                new SingleCharSyntaxErrorHandler();

            tokenStateWriter = scannerWriterFactory.CreateStateWriter(
                options.HasOption("scanner.write.states", true));

        }

        public bool TryBuild(ILogger logger,
            Logger parserLogger, 
            ParseTree.Language language,
            out ScannerWriter? scanner)
        {
            if (!language.BuildNfa(parserLogger, nfaWriter, out var nfa))
            {
                scanner = null;
                return false;
            }

            logger.LogMessage(Importance.Normal, "Building dfa");
            var dfa = nfa!.ToDfa(logger);

            var scannerStatesWriter = scannerWriterFactory.Create(dfa,
                tableWriterFactory,
                syntaxErrorHandler);

            scanner = new ScannerWriter(nfa!, 
                dfa, 
                scannerStatesWriter,
                tokenStateWriter);
            
            return true;
        }

        public class ScannerWriter
        {
            private readonly Dfa.Dfa dfa;
            private readonly IGeneratable scannerStates;
            private readonly Nfa.Symbol[] symbols;
            private readonly TokenStateWriterBase tokenStates;

            public ScannerWriter(Nfa.Graph graph,
                Dfa.Dfa dfa,
                IGeneratable scannerStatesWriter,
                TokenStateWriterBase tokenStatesWriter)
            {
                symbols = graph.Machine.AcceptingStates.Symbols.ToArray();
                this.dfa = dfa;
                this.scannerStates = scannerStatesWriter;
                this.tokenStates = tokenStatesWriter;
            }

            public IEnumerable<Nfa.Symbol> Symbols => symbols;

            public void Write(Generator generator) =>
                scannerStates.Write(generator);

            public void WriteTokenEnum(Generator generator) =>
                tokenStates.Write(generator, dfa);
        }
    }
}
