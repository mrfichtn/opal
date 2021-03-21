using System.Collections.Generic;

namespace Opal.Nfa
{
    public static class RemainingMatches
    {
        /// <summary>
        /// Returns a match containing characters not found in matches
        /// </summary>
        public static IMatch Remaining(this IEnumerable<IMatch> matches)
        {
            var data = new CharClass();
            foreach (var match in matches)
                data.AddTo(match);
            return data.Invert()
                .Reduce();
        }
    }
}
