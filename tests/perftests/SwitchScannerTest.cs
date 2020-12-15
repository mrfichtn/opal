using Opal;
using System.Collections.Generic;

namespace perftests
{
    public class SwitchScannerTest: TestBase
    {
        public SwitchScannerTest()
            : base("Switch scanner")
        {
        }
        
        protected override long Test(string source)
        {
            long sum = 0;
            var scanner = new OpalSwitchScanner(source);
            Token token;
            do
            {
                token = scanner.RawNextToken();
                sum += token.State;
            } while (token.State != 0);
            return sum;
        }
        public static Token[] States(string source)
        {
            var scanner = new OpalSwitchScanner(source);
            var states = new List<Token>();
            Token token;
            do
            {
                token = scanner.RawNextToken();
                states.Add(token);
            } while (token.State != 0);
            return states.ToArray();
        }
    }
}
