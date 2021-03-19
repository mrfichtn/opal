using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Opal
{
    public class ScannerBuffer: IDisposable
    {
        private readonly ScannerBase scanner;
        private readonly Logger logger;
        private ImmutableQueue<Token> peekQueue;


        public ScannerBuffer(ScannerBase scanner,
            Logger logger) 
        {
            this.scanner = scanner;
            this.logger = logger;
            peekQueue = ImmutableQueue<Token>.Empty;
        }

        public void Dispose()
        {
            scanner.Dispose();
            GC.SuppressFinalize(this);
        }

        public Token NextToken()
        {
            while (!peekQueue.IsEmpty)
            {
                peekQueue = peekQueue.Dequeue(out var token);
                if (token.State >= 0)
                    return token;
                SyntaxError(token);
            }

            while (true)
            {
                var token = scanner.NextToken();
                if (token.State >= 0)
                    return token;
                SyntaxError(token);
            }
        }


        public Token PeekToken()
        {
            while (true)
            {
                var token = scanner.NextToken();
                peekQueue = peekQueue.Enqueue(token);
                if (token.State >= 0)
                    return token;
            }
        }
        
        private void SyntaxError(Token token) => logger.LogError("Syntax error", token);
    }
}
