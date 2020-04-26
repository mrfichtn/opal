using System.Collections.Generic;
using System.Text;

namespace Opal.LR1
{
    internal class States: List<State>
	{
		public new State Add(State state)
		{
			base.Add(state);
			return state;
		}

		public int FindState(LR1Item item)
		{
			foreach (var state in this)
			{
				foreach (var i in state)
				{
					if (item.Equals(i))
						return state.Index;
				}
			}
			return -1;
		}

		public bool Contains(LR1Item item)
		{
			foreach (var state in this)
			{
                if (state.Contains(item))
                    return true;
			}
			return false;
		}

        public bool TryGetId(State newState, out int id)
        {
            var result = false;
            id = -1;
            foreach (var state in this)
            {
                if (newState.IsSubsetOf(state))
                {
                    result = true;
                    id = state.Index;
                    break;
                }
            }
            return result;
        }

        public new bool Contains(State newState)
		{
			var result = false;
			foreach (var state in this)
			{
                if (newState.SetEquals(state))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (var state in this)
			{
                state.AppendTo(builder);
				builder.AppendLine();
			}
			return builder.ToString();
		}
	}
}
