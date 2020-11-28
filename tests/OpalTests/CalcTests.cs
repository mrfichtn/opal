using CalcTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal;

namespace OpalTests
{
    [TestClass]
    public class CalcTests
    {
        [TestMethod]
        public void TestCalcParse()
        {
            var text = "4+4*4";
            var parser = Parser.FromString(text);
            var isOk = parser.Parse();
            Assert.IsTrue(isOk);
            var root = parser.Root as Expr;
            Assert.IsNotNull(root);
            var result = root.Calc();
            Assert.AreEqual(20, result);
        }
    }
}
