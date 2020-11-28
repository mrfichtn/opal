using System.Collections.Generic;
using System.Linq;
using Generators;
using Opal.Logging;
using Opal.ParseTree;

namespace Opal.LR1
{
    public class LR1Parser
	{
		private readonly Grammar _grammar;

        public LR1Parser(ILogger logger, 
            Grammar grammar, 
            ConflictList conflicts)
		{
			_grammar = grammar;
			var endSymbol = grammar.Symbols[0];

            //Honalee Algorithm
            //Create kernel item from augmented gramar production
            var kernel = new LR1Item(grammar[0], 0, endSymbol);
            var startState = new State(0) { kernel };
            States = new States { startState };
            startState.Closure(_grammar);

            var queue = new Queue<State>();
            queue.Enqueue(startState);
            var actionBuilder = new ActionBuilder(_grammar);

            //Method2
            while (queue.Count > 0)
            {
                startState = queue.Dequeue();
                Goto(startState, actionBuilder, queue);
            }

            var resolver = new ConflictResolvers(conflicts,
                grammar.Symbols,
                logger);

            Actions = actionBuilder.Build(logger, 
                States, 
                _grammar.Symbols,
                resolver);
        }

        #region Properties
        public Actions Actions { get; }
        public States States { get; }
        #endregion

        /// <summary>
        /// Honalee algorithm
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionBuilder"></param>
        /// <param name="queue"></param>
        private void Goto(State state, ActionBuilder actionBuilder, Queue<State> queue)
		{
			foreach (var symbol in state.NextSymbols())
			{
				var newState = new State(States.Count, symbol);

                foreach (var item in state.Where(x=>x.IsSymbol(symbol)))
                    newState.Add(item.Production, item.Position + 1, item.Lookahead);

                if (newState.Count > 0)
                {
                    if (!States.TryGetId(newState, out var stateId))
                    {
                        States.Add(newState);
                        newState.Closure(_grammar);
                        queue.Enqueue(newState);
                        stateId = newState.Index;
                    }

                    actionBuilder.AddGoto(state.Index, symbol.Id, stateId);
                    //For debug purposes, we'll record the transition it the item
                    foreach (var item in state.Where(x => x.IsSymbol(symbol)))
                        item.SetGoto(stateId);
                }
            }

            foreach (var item in state.Where(x => x.AtEnd))
                actionBuilder.AddReduce(state.Index, item.Lookahead.Id, item.Production.Id);
		}

        public void WriteSymbols(IGenerator generator)
        {
            var symbols = _grammar.Symbols;

            var maxTerminal = symbols.Where(x => x.IsTerminal).Max(x => x.Id);
            generator.Indent(1)
                .WriteLine("#region Symbols")
                .WriteLine($"protected const int _maxTerminal = {maxTerminal};")
                .WriteLine("protected readonly string[] _symbols =")
                .StartBlock();

            generator.Write("\"𝜖\"");
            for (var i = 1; i < symbols.Count; i++)
            {
                generator.WriteLine(",");
                var symbol = symbols[i];
                generator.Write("\"");

                generator.Write(symbol.ParseSymbol);

                //if (symbol.Value.StartsWith("@"))
                //    generator.Write(symbol.Value.Substring(1));
                //else
                //    generator.Write(symbol.Value);

                generator.Write("\"");
            }
            generator
                .WriteLine()
                .EndBlock(";")
                .WriteLine("#endregion")
                .UnIndent(1);
        }
    }
}
