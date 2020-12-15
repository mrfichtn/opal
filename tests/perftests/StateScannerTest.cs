using Opal;
using System.Collections.Generic;

namespace perftests
{
    public class StateScannerTest: TestBase
    {
        public StateScannerTest()
            : base("State scanner")
        {
        }
        
        protected override long Test(string source)
        {
            long sum = 0;
            var scanner = new OpalStateScanner(source);
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
            var scanner = new OpalStateScanner(source);
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
