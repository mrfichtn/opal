using System.Collections.Generic;
using System.Linq;
using System.Text;
using Generators;

namespace Opal.Dfa
{
    public class DfaNode
    {
        private readonly int[] next;
        
		public DfaNode(int acceptingState, int index, int[] next)
		{
			AcceptingState = acceptingState;
			Index = index;
			this.next = next;
		}

        #region Properties
        public int Index { get; private set; }
        public int AcceptingState { get; }
        public bool IsAccepting => (AcceptingState != 0);
		public bool NonAccepting => (AcceptingState == 0);
        public int Count => next.Length;
        public IEnumerable<int> Next => next;

        #region Indexer
        public int this[int index]
		{
			get { return next[index]; }
			internal set { next[index] = value; }
		}
        #endregion

        #endregion

        public void Write(IGenerator language)
        {
            language.Write("State({0}", AcceptingState);

            foreach (var next in next)
                language.Write(", {0}", next);
            
            language.WriteLine("),");
        }

		public void WriteAsArray(IGenerator language)
		{
			language.Write(AcceptingState.ToString());
			foreach (var item in next)
				language.Write(", {0}", item);
		}

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0,5}", Index);
            builder.AppendFormat(" [{0}]  ", IsAccepting ? AcceptingState.ToString("000") : "   ");
            for (int i = 1; i < next.Length; i++)
                builder.AppendFormat("{0,10}", next[i]);
            return builder.ToString();
        }

        public bool IsEqual(DfaNode node)
        {
            if (AcceptingState != node.AcceptingState)
                return false;

            var next = node.next;
            for (int i = 0; i < this.next.Length; i++)
            {
                if (this.next[i] != next[i])
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
            for (int i = 0; i < next.Length; i++)
            {
                if (next[i] > oldNext)
                    next[i]--;
                else if (next[i] == oldNext)
                    next[i] = newNext;
            }
            if (Index > oldNext)
                Index--;
        }

        /// <summary>
        /// Removes state
        /// </summary>
        public void RemoveState(int oldNext)
        {
            for (int i = 0; i < next.Length; i++)
            {
                if (next[i] > oldNext)
                    next[i]--;
                else if (next[i] == oldNext)
                    next[i] = 0;
            }
            if (Index > oldNext)
                Index--;
        }

        public void CopyNextStatesTo(ICollection<int> set)
        {
            set.Clear();
            for (var i = 1; i < next.Length; i++)
                set.Add(next[i]);
        }
    }
}
