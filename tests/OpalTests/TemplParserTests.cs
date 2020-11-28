using Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.Containers;
using OpalTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpalTests
{
    [TestClass]
    public class TemplParserTests
    {
        [TestMethod]
        public void TemplParserTest()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");
            provider.Variables.Add("templ", "$(b)");

            var template = "a $(b) $(include templ) $(if false)drop$(endif)include";
            
            var templ = new TemplateProcessor2(template);
            var generator = new Generator();
            templ.Format(generator, provider);
            var text = generator.ToString();

            Assert.AreEqual(
                "a b-value b-value include",
                text);
        }

        [TestMethod]
        public void TemplParserIf()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");
            provider.Variables.Add("templ", "$(b)");

            var template = "$(if true)true$(endif)$(if false)$(true b)$(endif)";

            var templ = new TemplateProcessor2(template);
            var generator = new Generator();
            templ.Format(generator, provider);
            var text = generator.ToString();

            Assert.AreEqual(
                "true",
                text);
        }

        [TestMethod]
        public void MacroScannerTest()
        {
            var scanner = new TemplateProcessor2.MacroScanner("hi 1 2,,");
            var list = new List<string>();
            while (true)
            {
                var t = scanner.Token();
                if (!t.AddTo(list))
                    break;
            }
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("hi", list[0]);
            Assert.AreEqual("1", list[1]);
            Assert.AreEqual("2", list[2]);
            Assert.AreEqual(string.Empty, list[3]);
        }

        [TestMethod]
        public void SimpleBufferNextChar()
        {
            var buffer = new SimpleStringBuffer("hi");
            var ch = buffer.NextChar();
            Assert.AreEqual('h', ch);

            ch = buffer.NextChar();
            Assert.AreEqual('i', ch);

            ch = buffer.NextChar();
            Assert.AreEqual(ch, SimpleStringBuffer.Eof);
        }
    }
}
