using Opal.Nfa;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Dfa
{
    public class DfaState
    {
        private readonly int _acceptingState;
        private int _index;
        private int[] _next;

        public DfaState(AcceptingStates acceptingStates, IEnumerable<int> nodes, int index, int edges)
        {
            NfaStates = nodes.ToArray();
            _index = index;
            _next = new int[edges];

            foreach (var node in nodes)
            {
                if (acceptingStates.TryFind(node, out var acceptingState))
                {
                    if((_acceptingState == 0) || (_acceptingState < acceptingState))
                        _acceptingState = acceptingState;
                }
            }
        }

        #region Properties
        public int[] NfaStates { get; }
        public int this[int i] => _next[i];
        #endregion


        public void AddTransition(int index, DfaState nextState)
        {
            _next[index] = nextState._index;
        }

        public DfaNode ToNode()
        {
            return new DfaNode(_acceptingState, _index, _next);
        }

        public void ReduceEdge(IList<int> toRemove)
        {
            var endIndex = toRemove.Count;

            var next = new int[_next.Length - endIndex];
            var destIndex = 0;
            var rmIndex = 1;

            int nextToRemove = toRemove[0];
            int i;
            for (i = 0; i < _next.Length; i++)
            {
                while (i < nextToRemove)
                    next[destIndex++] = _next[i++];

                if (rmIndex < endIndex)
                    nextToRemove = toRemove[rmIndex++];
                else
                    nextToRemove = _next.Length;
            }
            _next = next;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Index: ").Append(_index).AppendLine()
                .Append("Accept: ").Append(_acceptingState).AppendLine()
                .Append("States: ").AppendLine();
            var index = 0;

            foreach (var next in NfaStates)
            {
                builder.Append(next.ToString("D3")).Append(" ");
                if (index == 19)
                {
                    index = 0;
                    builder.Length--;
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
