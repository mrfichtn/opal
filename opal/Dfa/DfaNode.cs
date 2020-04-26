using System.Collections.Generic;
using System.Linq;
using System.Text;
using Generators;

namespace Opal.Dfa
{
    public class DfaNode
    {
        private int[] _next;
        
		public DfaNode(int acceptingState, int index, int[] next)
		{
			AcceptingState = acceptingState;
			Index = index;
			_next = next;
		}

        #region Properties
        public int Index { get; private set; }
        public int AcceptingState { get; }
        public bool IsAccepting => (AcceptingState != 0);
		public bool NonAccepting => (AcceptingState == 0);
        public int Count => _next.Length;
        public IEnumerable<int> Next => _next;

        #region Indexer
        public int this[int index]
		{
			get { return _next[index]; }
			internal set { _next[index] = value; }
		}
        #endregion

        #endregion

        public void Write(IGenerator language)
        {
            language.Write("State({0}", AcceptingState);

            foreach (var next in _next)
                language.Write(", {0}", next);
            
            language.WriteLine("),");
        }

		public void WriteAsArray(IGenerator language)
		{
			language.Write(AcceptingState.ToString());
			foreach (var item in _next)
				language.Write(", {0}", item);
		}

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0,5}", Index);
            builder.AppendFormat(" [{0}]  ", IsAccepting ? AcceptingState.ToString("000") : "   ");
            for (int i = 1; i < _next.Length; i++)
                builder.AppendFormat("{0,10}", _next[i]);
            return builder.ToString();
        }

        public bool IsEqual(DfaNode node)
        {
            if (AcceptingState != node.AcceptingState)
                return false;

            var next = node._next;
            for (int i = 0; i < _next.Length; i++)
            {
                if (_next[i] != next[i])
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Replaces old with next and reindexes states that
        /// shift 1 down
        /// </summary>
        public void RemoveState(int oldNext, int newNext)
        {
            for (int i = 0; i < _next.Length; i++)
            {
                if (_next[i] > oldNext)
                    _next[i]--;
                else if (_next[i] == oldNext)
                    _next[i] = newNext;
            }
            if (Index > oldNext)
                Index--;
        }

        /// <summary>
        /// Removes state
        /// </summary>
        public void RemoveState(int oldNext)
        {
            for (int i = 0; i < _next.Length; i++)
            {
                if (_next[i] > oldNext)
                    _next[i]--;
                else if (_next[i] == oldNext)
                    _next[i] = 0;
            }
            if (Index > oldNext)
                Index--;
        }

        public void CopyNextStatesTo(ICollection<int> set)
        {
            set.Clear();
            for (var i = 1; i < _next.Length; i++)
                set.Add(_next[i]);
        }
    }
}
