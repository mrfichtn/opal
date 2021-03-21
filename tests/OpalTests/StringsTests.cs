using Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpalTests
{
    [TestClass]
    public class StringsTests
    {
        [TestMethod]
        public void FromHexDigitTest()
        {
            Assert.IsTrue(Strings.FromHexDigit('0', out var value));
            Assert.AreEqual(0, value);

            Assert.IsTrue(Strings.FromHexDigit('9', out value));
            Assert.AreEqual(9, value);

            Assert.IsTrue(Strings.FromHexDigit('a', out value));
            Assert.AreEqual(0xa, value);

            Assert.IsTrue(Strings.FromHexDigit('A', out value));
            Assert.AreEqual(0xa, value);

            Assert.IsTrue(Strings.FromHexDigit('f', out value));
            Assert.AreEqual(0xf, value);

            Assert.IsTrue(Strings.FromHexDigit('F', out value));
            Assert.AreEqual(0xf, value);
        }

        [TestMethod]
        public void FromEscCharStringTests()
        {
            char ch;
            Assert.IsTrue(Strings.FromEscCharString(@"'\x123'", out ch));
            Assert.AreEqual('\x123', ch);

            Assert.IsTrue(Strings.FromEscCharString(@"'\x1234'", out ch));
            Assert.AreEqual('\x1234', ch);
        }
    }
}
