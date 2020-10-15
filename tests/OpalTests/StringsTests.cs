using Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
