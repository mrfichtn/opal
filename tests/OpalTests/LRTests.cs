using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.LR1;
using Opal.ParseTree;

namespace OpalTests
{
    [TestClass]
    public class LRTests
    {
        [TestMethod]
        public void BookTest()
        {
            var grammar = new Grammar(
                "S", "C", "C", null,
                "C", "c", "C", null,
                "C", "d", null);

            var text = grammar.ToString(false);
            var expected =
@"R0: S' -> S
R1: S -> C C
R2: C -> c C
R3: C -> d
";
            Assert.AreEqual(expected, text);

            var test = "test.log";
            var logger = new Opal.ConsoleLogger(test);
            var conflicts = new ConflictList();
            var builder = new LR1Parser(logger, grammar, conflicts);
            var states = builder.States.ToString(false);

            expected =
@"S0
    S': · S, ＄
    S: · C C, ＄
    C: · c C, c
    C: · c C, d
    C: · d, c
    C: · d, d

S1
    S': S ·, ＄

S2
    S: C · C, ＄
    C: · c C, ＄
    C: · d, ＄

S3
    C: · c C, c
    C: · c C, d
    C: c · C, c
    C: c · C, d
    C: · d, c
    C: · d, d

S4
    C: d ·, c
    C: d ·, d

S5
    S: C C ·, ＄

S6
    C: · c C, ＄
    C: c · C, ＄
    C: · d, ＄

S7
    C: d ·, ＄

S8
    C: c C ·, c
    C: c C ·, d

S9
    C: c C ·, ＄
";
            Assert.AreEqual(expected, states);
        }
    }
}
