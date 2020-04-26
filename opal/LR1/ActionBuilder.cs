using Opal.Containers;
using Opal.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Opal.LR1
{
    public class ActionBuilder
	{
		private readonly Dictionary<int, IDictionary<uint, IList<int>>> _actions;
        private readonly Grammar _grammar;

		public ActionBuilder(Grammar grammar)
		{
            _grammar = grammar;
            _actions = new Dictionary<int, IDictionary<uint, IList<int>>>();
		}

		public Actions Build(ILogger logger, int states, IList<Symbol> symbols)
		{
			var data = new int[states, symbols.Count];

			for (var j = 0; j < data.GetLength(0); j++)
			{
				for (var i = 0; i < data.GetLength(1); i++)
					data[j, i] = -1;
			}

			foreach (var p in _actions)
			{
				foreach (var q in p.Value)
				{
                    var items = q.Value;
                    if (items.Count == 1)
                    {
                        data[p.Key, q.Key] = items[0];
                    }
                    else if (items.Count > 1)
                    {
                        data[p.Key, q.Key] = q.Value.First(items[0], x => x >= 0);
                        var symbol = symbols[(int)q.Key];

                        logger.LogWarning("Conflicted state = {0}, Lookahead = {1}", p.Key, symbol);

                        foreach (var action in q.Value)
                        {
                            if (action < 0)
                            {
                                var rule = -2 - action;
                                logger.LogWarning("  Reduce rule {0}: {1}", rule, _grammar[(uint)rule]);
                            }
                            else
                            {
                                logger.LogWarning("  Shift {0}", action);
                            }
                        }
                    }
				}
			}

			return new Actions(data, symbols);
		}

		public void Add(int state, uint lookahead, int action)
		{
			if (!_actions.TryGetValue(state, out var map))
			{
                var list = new List<int> { action };
                map = new Dictionary<uint, IList<int>> { { lookahead, list } };
                _actions.Add(state, map);
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
