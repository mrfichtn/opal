namespace Opal.Nfa
{
    public static class Match
    {
        public static IMatch Invert(Token t, IMatch match) => match.Invert(t);

        public static IMatch Union(IMatch m1, IMatch m2) => m1.Union(m2);

        public static IMatch Difference(IMatch m1, IMatch m2) => m1.Difference(m2);
    }
}
