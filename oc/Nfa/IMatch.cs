using Generators;
using System;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public interface IMatch : IEquatable<IMatch>, IEnumerable<char>
    {
        int Count { get; }
        int WriteCount { get; }

        IMatch Invert();
        IMatch Invert(Token t);
        IMatch Reduce();
        bool IsMatch(char ch);
        IMatch? Intersect(IMatch match);
        IMatch Difference(IMatch match);
        IMatch Union(IMatch match);

        string SwitchCondition(string varName);
    }

    public static class Match
    {
        public static IMatch Invert(Token t, IMatch match) => match.Invert(t);

        public static IMatch Union(IMatch m1, IMatch m2) => m1.Union(m2);

        public static IMatch Difference(IMatch m1, IMatch m2) => m1.Difference(m2);
    }
}
