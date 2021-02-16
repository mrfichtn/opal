using System;

namespace Opal.ParseTree
{
    public class Conflict: Segment
    {
        private readonly int state;
        private readonly int nextState;
        
        public Conflict(Identifier state,
            Identifier symbol,
            Identifier action,
            Identifier nextState)
            : base(state)
        {
            if (state.Value.Length > 0)
                int.TryParse(state.Value.Substring(1), out this.state);

            Symbol = symbol.Value;
            Shift = !string.Equals(action.Value, 
                "reduce", 
                StringComparison.InvariantCultureIgnoreCase);
            if (nextState.Value.Length > 0)
                int.TryParse(nextState.Value.Substring(1), out this.nextState);
        }

        public int State => state;
        public string Symbol { get; }
        public bool Shift { get; }
        public int NextState => nextState;
    }
}
