using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.Dfa;
using Opal.Nfa;

namespace OpalTests
{
    [TestClass]
    public class NfaTests
    {
        [TestMethod]
        public void NfaTest()
        {
            var graph = new Graph("hello");
            graph.MarkEnd("kw_hello");

            var g2 = graph.Create("world");
            g2.MarkEnd("kw_world");
            graph = graph.Union(g2);

            var dfa = graph.ToDfa();
            var actual = new ScannerStateTable(dfa.States).Create();

            var expected = new[]
            { 
            // ▽  ⌀  o  l  e  h  d  r  w
                0, 0, 0, 0, 0, 1, 0, 0, 2, 0,
                0, 0, 0, 0, 7, 0, 0, 0, 0, 0,
                0, 0, 3, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 4, 0, 0,
                0, 0, 0, 5, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 6, 0, 0, 0,
                2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 8, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 9, 0, 0, 0, 0, 0, 0,
                0, 0,10, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            Verify(expected, actual);

            actual = new ScannerStateTableWithSyntaxErrors(dfa.States).Create();
            //var expected2 = string.Join(",", map.OfType<int>().ToArray());
            expected = new[]
            {
            //  ▽ ⌀  o  l  e  h  d  r  w
                0,11,11,11,11, 1,11,11, 2,11,
                0, 0, 0, 0, 7, 0, 0, 0, 0, 0,
                0, 0, 3, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 4, 0, 0,
                0, 0, 0, 5, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 6, 0, 0, 0,
                2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 8, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 9, 0, 0, 0, 0, 0, 0,
                0, 0,10, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
               -1,11,11,11,11, 0,11,11, 0,11
            };
            Verify(expected, actual);
        }

        void Verify(int[] expected, int[,] actual)
        {
            var index = 0;
            foreach (var item in actual)
            {
                if (index >= expected.Length)
                    Assert.Fail("Actual length is greator than expected");
                Assert.AreEqual(expected[index++], item);
            }
            if (index < expected.Length)
                Assert.Fail("Actual length is less than expected");
        }


        [TestMethod]
        public void NfaReduceTest()
        {
            var graph = new Graph("h");
            graph.MarkEnd("kw_h");

            var g2 = graph.Create("y");
            var g3 = graph.Create("m");
            g2.Union(g3);
            g2.MarkEnd("kw_you");

            graph = graph.Union(g2);

            graph.Reduce();

            var expected = NfaArray.Create()
                .Epsilon2(index: 8, left: 6, right: 1)
                .Match(index: 1, left: 0, match: 1)
                .Epsilon2(index: 6, left: 5, right: 3)
                .Epsilon1(index: 0, state: 1, right: 9)
                .Match(index: 3, left: 7, match: 2)
                .Match(index: 5, left: 7, match: 3)
                .None(index: 9)
                .Epsilon1(index: 7, state: 2, right: 9)
                .ToArray();

            var actual = graph.ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NfaRemoveSingleEpsilonTest()
        {
            var graph = new Graph("h");
            graph.MarkEnd("kw_h");

            var g2 = graph.Create("y");
            var g3 = graph.Create("m");
            g2.Union(g3);
            g2.MarkEnd("kw_you");

            graph = graph.Union(g2);

            graph.RemoveSingleEpsilons();

            var expected = NfaArray.Create()
                .Epsilon2(index: 8, left: 6, right: 1)
                .Match   (index: 1, left: 0, match: 1)
                .Epsilon2(index: 6, left: 5, right: 3)
                .Epsilon1(index: 0, state: 1, right: 9)
                .Match   (index: 3, left: 7, match: 2)
                .Match   (index: 5, left: 7, match: 3)
                .None    (index: 9)
                .Epsilon1(index: 7, state:2, right: 9)
                .ToArray();

            var actual = graph.ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConcatTest()
        {
            var graph = new Graph("c");
            var h = graph.Create("h");
            graph.Concatenate(h);

            var actual = graph.ToArray();


            var expected = NfaArray.Create()
                .Match(index: 1, left: 0, match: 1)
                .Epsilon1(index: 0, right: 3)
                .Match(index: 3, left: 2, match: 2)
                .None(index: 2)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
