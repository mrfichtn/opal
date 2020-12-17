using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.Containers;
using Opal.Nfa;

namespace OpalTests
{
    [TestClass]
    public class MatchTests
    {
        [TestMethod]
        public void TestRemaining()
        {
            var match = new SingleChar('a');
            var remaining = match
                .Yield()
                .Remaining();
            Assert.AreEqual(char.MaxValue, remaining.Count);
        }

        [TestMethod]
        public void TestRemaining2()
        {
            var match = new CharClass("[ab]");
            var remaining = match
                .Yield()
                .Remaining();
            Assert.AreEqual(char.MaxValue-1, remaining.Count);
        }

    }
}
