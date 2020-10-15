using Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.CodeGenerators;
using System.IO;

namespace OpalTests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void TestEscapes()
        {
            var esc = "Hello\r\nWorld";
            var writer = new StringWriter();
            var gen = new Generator(writer);
            gen.WriteEsc(esc);

            var text = writer.ToString();
            Assert.AreEqual(@"""Hello\r\nWorld""", text);
        }
    }
}
