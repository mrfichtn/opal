using Generators;
using System;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public interface IMatch : IEquatable<IMatch>, IEnumerable<char>
    {
        int Count { get; }

        IMatch Invert();
        IMatch Invert(Token t);
        IMatch Reduce();
        bool IsMatch(char ch);
        IMatch? Intersect(IMatch match);
        IMatch Difference(IMatch match);
        IMatch Union(IMatch match);

        string SwitchCondition(string varName);
    }


}
