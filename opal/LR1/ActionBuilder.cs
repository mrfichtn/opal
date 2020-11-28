using Opal.Containers;
using System.Collections.Generic;
using System.Linq;

namespace Opal.LR1
{
    public class ActionBuilder
	{
		private readonly Dictionary<int, IDictionary<uint, IList<int>>> actions;
        private readonly Grammar grammar;

		public ActionBuilder(Grammar grammar)
		{
            this.grammar = grammar;
            actions = new Dictionary<int, IDictionary<uint, IList<int>>>();
		}

		public Actions Build(ILogger logger, 
			States states, 
			Symbols symbols,
			ConflictResolvers resolvers)
		{
			var data = new int[states.Count, symbols.Count];

			for (var j = 0; j < data.GetLength(0); j++)
			{
				for (var i = 0; i < data.GetLength(1); i++)
					data[j, i] = -1;
			}

			foreach (var stateAction in actions)
			{
				foreach (var q in stateAction.Value)
				{
                    var items = q.Value;
					if (items.Count == 0)
						continue;

					if (items.Count == 1)
					{
						data[stateAction.Key, q.Key] = items[0];
					}
					else if (resolvers.TryFind(
							state: stateAction.Key,
							symbol: q.Key,
							action: out var resolveAction) &&
							items.Contains(resolveAction))
					{
						data[stateAction.Key, q.Key] = resolveAction;
					}
					else
                    {
						logger.LogWarning(resolvers.ToString());
						data[stateAction.Key, q.Key] = q.Value.First(items[0], x => x >= 0);
						var symbol = symbols[(int)q.Key];
						var state = states[stateAction.Key];

						logger.LogWarning("Conflicted state S{0}, Lookahead = {1}", stateAction.Key, symbol);
						if (state.Symbol != null)
							logger.LogWarning($"  (transition: {state.Symbol.Name})");

						foreach (var action in q.Value)
						{
							if (action < 0)
							{
								var rule = -2 - action;
								logger.LogWarning("  Reduce rule {0}: {1}", rule, grammar[(uint)rule]);
							}
							else
							{
								logger.LogWarning("  Shift S{0}", action);
							}
						}
                    }
				}
			}

			return new Actions(data, symbols);
		}

		public void Add(int state, uint lookahead, int action)
		{
			if (!actions.TryGetValue(state, out var map))
			{
                var list = new List<int> { action };
                map = new Dictionary<uint, IList<int>> { { lookahead, list } };
                actions.Add(state, map);
			}
			else if (!map.TryGetValue(lookahead, out var list))
			{
                list = new List<int> { action };
                map.Add(lookahead, list);
			}
			else if (!list.Contains(action))
			{
				list.Add(action);
			}
		}

		public void AddGoto(int state, uint lookahead, int nextState)
		{
			Add(state, lookahead, nextState);
		}

        public void AddReduce(int state, uint lookahead, uint ruleId)
        {
            var action = -(int)(2 + ruleId);
            Add(state, lookahead, action);
        }
	}
}
