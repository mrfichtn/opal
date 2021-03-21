using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public static class AddSyntaxErrorExt
    {
        public static IEnumerable<DfaNode> AddSyntaxError(this DfaNode[] nodes)
        {
            if (nodes.Length == 0)
                yield break;

            var first = nodes[0];

            if (nodes.Length == 1)
            {
                yield return first;
                yield break;
            }

            var next = first.Next.ToArray();
            next[0] = nodes.Length;
            for (var i = 1; i < next.Length; i++)
            {
                if (next[i] == 0)
                    next[i] = nodes.Length;
            }
            yield return new DfaNode(first.AcceptingState,
                first.Index, next);
            for (var i = 1; i < nodes.Length; i++)
                yield return nodes[i];

            next = first.Next
                .Select(x => x == 0 ? nodes.Length : 0)
                .ToArray();
            yield return new DfaNode(-1,
                nodes.Length,
                next);
        }
    }
}
