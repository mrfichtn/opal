namespace Opal.Nfa
{
    /// <summary>
    /// Describes the transitions leaving an NfaNode
    /// Epsilon represents the empty-set transition
    /// </summary>
    public enum EdgeState
    {
        /// <summary>
        /// No transitions
        /// </summary>
        None = 0,
        /// <summary>
        /// Node containing a single epsilon transition, stored in right
        /// </summary>
        OneEpsilon = 1,
        /// <summary>
        /// A single transition taken if _match, stored in left
        /// </summary>
        OneTransition = 2,
        /// <summary>
        /// Left transition if _match, right transition for epsilon
        /// </summary>
        Both = 3,
        /// <summary>
        /// Two epsilon transitions
        /// </summary>
        TwoEpsilon = 5
    }
}
