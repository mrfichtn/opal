using Generators;
using System.Collections.Generic;

namespace Opal.Dfa
{
    public interface IScannerWriterFactory
    {
        IGeneratable Create(Dfa dfa, 
            ITableWriterFactory tableWriterFactory, 
            ISyntaxErrorHandler syntaxErrorHandler);

        TokenStateWriterBase CreateStateWriter(bool writeStates);
    }

    public class StateScannerWriterFactory: IScannerWriterFactory
    {
        public IGeneratable Create(Dfa dfa,
            ITableWriterFactory tableWriterFactory,
            ISyntaxErrorHandler syntaxErrorHandler) =>
            new DfaTableWriter(dfa, tableWriterFactory, syntaxErrorHandler);

        public TokenStateWriterBase CreateStateWriter(bool writeStates) =>
            writeStates ? 
            new AllTokenStatesWriter() :
            new MinimumStatesWriter();
    }

    public class SwitchScannerWriterFactory: IScannerWriterFactory
    {
        public IGeneratable Create(Dfa dfa, 
            ITableWriterFactory tableWriterFactory,
            ISyntaxErrorHandler syntaxErrorHandler) =>
        new DfaSwitchWriter(dfa, syntaxErrorHandler);

        public TokenStateWriterBase CreateStateWriter(bool writeStates) =>
            new AllTokenStatesWriter();
    }
}
