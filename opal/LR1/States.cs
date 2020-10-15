using System.Collections.Generic;
using System.Text;

namespace Opal.LR1
{
    public class States: List<State>
	{
		public new State Add(State state)
		{
			base.Add(state);
			return state;
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

        public override string ToString() => ToString(true);

        public string ToString(bool showTransition)
        {
            return new StringBuilder()
                .Append(this, showTransition)
                .ToString();
        }
    }

    public static class StatesExt
    {
        public static StringBuilder Append(this StringBuilder builder, 
            States states, 
            bool showTransitions = false)
        {
            foreach (var state in states)
            {
                builder.Append(state, showTransitions)
                    .AppendLine();
            }
            if (states.Count > 0)
                builder.Length-=2;
            return builder;
        }
    }
}
