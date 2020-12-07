using Opal.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.LR1
{
    public class State : HashSet<LR1Item>
    {
        public State(int index, Symbol symbol)
        {
            Index = index;
            Symbol = symbol;
        }

        public State(int index)
        {
            Index = index;
        }

        public Symbol? Symbol { get; set; }
        public int Index { get; }

        public LR1Item Add(Rule production, uint position, Symbol lookahead)
        {
            var newItem = new LR1Item(production, position, lookahead);
            Add(newItem);
            return newItem;
        }

        public State Closure(Grammar grammar)
        {
            var queue = new Queue<LR1Item>(this);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var currentRule = item.Production;
                var pos = item.Position;
                if (pos >= currentRule.Right.Length)
                    continue;

                var B = currentRule.Right[pos];
                if (B.IsTerminal)
                    continue;
                //else if (grammar.TerminalSets.HasEmpty(B.Id))
                //{
                //    var newItem = new LR1Item(item.Production, pos + 1, item.Lookahead);
                //    if (Add(newItem))
                //        queue.Enqueue(newItem);
                //}

                var firstSet = currentRule.FindFirst(pos + 1, item.Lookahead);

                foreach (var production in grammar.Where(x=>x.Left == B))
                {
                    foreach (var term in firstSet)
                    {
                        var newItem = new LR1Item(production, 0, grammar.Symbols[(int)term]);
                        if (Add(newItem))
                            queue.Enqueue(newItem);
                    }
                }
            }
            return this;
        }

        public bool TryFindNextSymbol(Symbol symbol, out LR1Item item)
        {
            return this
                .Where(x => x.HasNextSymbol(symbol))
                .TryFirst(out item);
        }

        public List<Symbol> NextSymbols()
        {
            var seq = new HashSet<uint>();
            var symbols = new List<Symbol>();
            foreach (var item in this)
            {
                if (item.NextSymbol(out Symbol? symbol) && seq.Add(symbol!.Id))
                    symbols.Add(symbol);
            }
            return symbols;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            AppendTo(builder);
            return builder.ToString();
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.Append('S')
                .Append(Index);
            if (Symbol != null)
                builder.Append(": (Transition = ")
                    .Append(Symbol)
                    .Append(')');
            builder.AppendLine();

            foreach (var item in this)
            {
                builder.Append("    ");
                item.AppendTo(builder);
                builder.AppendLine();
            }
        }
    }

    public static class StateExt
    {
        public static StringBuilder Append(this StringBuilder builder, 
            State state, 
            bool showTransition)
        {
            builder.Append('S')
                .Append(state.Index);
            if (showTransition && (state.Symbol != null))
            {
                builder.Append(": (Transition = ")
                    .Append(state.Symbol)
                    .Append(')');
            }
            builder.AppendLine();

            foreach (var item in state.OrderBy(x=>x))
            {
                builder.Append("    ")
                    .Append(item, showTransition)
                    .AppendLine();
            }
            return builder;
        }
    }

}
