using System.Collections.Generic;
using System.Linq;
using Generators;
using System.Text;
using Opal.Nfa;

namespace Opal.Dfa
{
    public class Dfa
    {
        public Dfa(Matches matches, 
            AcceptingStates acceptingStates, 
            IEnumerable<DfaNode> states)
        {
            Matches = matches;
            AcceptingStates = acceptingStates;
            MatchToClass = matches.ToArray();
            States = states.ToArray();
        }

        #region Properties
        public AcceptingStates AcceptingStates { get; }
        public DfaNode[] States { get; }
        public int MaxClass => Matches.NextId;
        public int[] MatchToClass { get; }
        public Matches Matches { get; }
        #endregion

        public string GetStatesDecompressMethod() => GetMethod("Decompress", States.Length);

        public string GetMethod(string method, int max)
        {
            string result;
            if (max <= byte.MaxValue)
                result = $"{method}8";
            else if (max <= ushort.MaxValue)
                result = $"{method}16";
            else if (max < (1 << 24))
                result = $"{method}24";
            else
                result = $"{method}32";
            return result;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("     Accept  ");
            Matches.AppendTo(builder).AppendLine();
            foreach (var state in States)
                builder.AppendLine(state.ToString());
            return builder.ToString();
        }

        /// <summary>
        /// Returns safe name for accepting state
        /// </summary>
        /// <param name="accepting"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TryGetAccepting(int accepting, out string name)
        {
            var result = AcceptingStates.TryGetName(accepting, out name);
            if (result)
                name = Identifier.SafeName(name);
            return result;
        }
    }
}
