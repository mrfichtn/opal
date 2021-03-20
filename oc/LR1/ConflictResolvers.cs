using Opal.Logging;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Opal.LR1
{
    public class ConflictResolvers
    {
        private readonly Dictionary<Key, int> data;
        
        public ConflictResolvers(ConflictList conflicts, 
            Symbols symbols,
            ILogger logger)
        {
            data = new Dictionary<Key, int>();
            foreach (var conflict in conflicts)
            {
                if (symbols.TryFind(conflict.Symbol, out var symbol))
                {
                    var key = new Key(conflict.State, symbol!.Id);
                    var action = conflict.Shift ?
                        conflict.NextState :
                        -(int)(2 + conflict.NextState);
                    data[key] = action;
                }
                else
                {
                    logger.LogWarning(conflict, "Missing symbol {0}", conflict.Symbol);
                }
            }
        }


        public int Count => data.Count;

        public bool TryFind(int state, uint symbol, out int action)
        {
            var key = new Key(state, symbol);
            return data.TryGetValue(key, out action);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var pair in data)
            {
                builder.AppendLine($"{pair.Key.State} {pair.Key.Symbol} = {pair.Value}");
            }
            return builder.ToString();
        }


        struct Key: IEquatable<Key>
        {
            public Key(int state, uint symbol)
            {
                State = state;
                Symbol = symbol;
            }
            
            public int State { get; }
            public uint Symbol { get; }

            public bool Equals(Key other)
            {
                return State == other.State &&
                    Symbol == other.Symbol;
            }

            public override bool Equals(object? obj) =>
                (obj is Key key) && Equals(key);

            public override int GetHashCode()
            {
                return State ^ Symbol.GetHashCode();
            }
        }
    }
}
