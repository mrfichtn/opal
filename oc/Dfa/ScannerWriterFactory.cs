namespace Opal.Dfa
{
    public interface IScannerWriterFactory
    {
        IDfaWriter Create(Dfa dfa, 
            ITableWriterFactory tableWriterFactory, 
            ISyntaxErrorHandler syntaxErrorHandler);

        TokenStateWriterBase CreateStateWriter(bool writeStates);
    }

    public class StateScannerWriterFactory: IScannerWriterFactory
    {
        public IDfaWriter Create(Dfa dfa,
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
        public IDfaWriter Create(Dfa dfa, 
            ITableWriterFactory tableWriterFactory,
            ISyntaxErrorHandler syntaxErrorHandler) =>
        new DfaSwitchWriter(dfa, syntaxErrorHandler);

        public TokenStateWriterBase CreateStateWriter(bool writeStates) =>
            new AllTokenStatesWriter();
    }
}
