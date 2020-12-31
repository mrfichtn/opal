using System.Collections.Generic;

namespace Opal.Dfa
{
    public interface ISyntaxErrorHandler
    {
        ScannerStateTable CreateStateTable(Dfa dfa);
        IEnumerable<DfaNode> GetStates(Dfa dfa);

    }


    public class GreedySyntaxErrorHandler: ISyntaxErrorHandler
    {
        public ScannerStateTable CreateStateTable(Dfa dfa) =>
            new ScannerStateTableWithSyntaxErrors(dfa.States);
        public IEnumerable<DfaNode> GetStates(Dfa dfa) =>
            dfa.States.AddSyntaxError();
    }

    public class SingleCharSyntaxErrorHandler: ISyntaxErrorHandler
    {
        public ScannerStateTable CreateStateTable(Dfa dfa) =>
            new ScannerStateTable(dfa.States);
        public IEnumerable<DfaNode> GetStates(Dfa dfa) =>
            dfa.States;
    }
}
