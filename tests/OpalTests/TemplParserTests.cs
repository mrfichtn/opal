using Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.Containers;
using Opal.Templates;
using OpalTests.Mocks;
using System.Collections.Generic;

namespace OpalTests
{
    [TestClass]
    public class TemplParserTests
    {
        [TestMethod]
        public void TemplSymbolTest()
        {
            var provider = new VarProviderMock();
            provider.Variables.Add("b", "b-value");

            var template = "a $(b)";

            var templ = new TemplateProcessor2(template);
            var generator = new Generator();
            templ.Format(generator, provider);
            var text = generator.ToString();

            Assert.AreEqual("a b-value", text);
        }
        
        [TestMethod]
        public void NotConditionTest()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");

            var template = "$(if !true)bad$(endif)$(if !false)good$(endif)";
            var text = Generate(template, provider);
            Assert.AreEqual("good", text);
        }

        [TestMethod]
        public void PartialMacroTest()
        {
            var provider = new VarProviderMock();

            var template = "$\"Hello world\"";
            var text = Generate(template, provider);
            Assert.AreEqual("$\"Hello world\"", text);
        }

        [TestMethod]
        public void MacroEscTest()
        {
            var provider = new VarProviderMock();

            var template = "$()";
            var text = Generate(template, provider);
            Assert.AreEqual("$(", text);
        }


        [TestMethod]
        public void NotConditionExprTrueTest()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");

            var template = "$(if !false)$(b)$(endif)";
            var text = Generate(template, provider);
            Assert.AreEqual("b-value", text);
        }

        [TestMethod]
        public void NotConditionExprFalseTest()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Variables.Add("b", "b-value");

            var template = "$(if !true)$(b)$(endif)";
            var text = Generate(template, provider);
            Assert.AreEqual("", text);
        }


        [TestMethod]
        public void ElseTest()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");

            var template = "$(if false)bad$(else)good$(endif)";
            var text = Generate(template, provider);
            Assert.AreEqual("good", text);
        }


        private static string Generate(string template, ITemplateContext context)
        {
            var templ = new TemplateProcessor2(template);
            var generator = new Generator();
            templ.Format(generator, context);
            return generator.ToString();
        }

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
        public void TemplParserStandaloneIf()
        {
            var provider = new VarProviderMock();
            provider.Conditions.Add("true", true);
            provider.Conditions.Add("false", false);
            provider.Variables.Add("b", "b-value");
            provider.Variables.Add("templ", "$(b)");

            var template = "$(if true)true$(endif)$(if false)false$(endif)";

            var templ = new TemplateProcessor2(template);
            var generator = new Generator();
            templ.Format(generator, provider);
            var text = generator.ToString();

            Assert.AreEqual("true", text);
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
            var scanner = new MacroScanner("hi 1 2,,");
            var list = new List<string>();
            while (scanner.NextToken(out var t))
                list.Add(t);
            
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("hi", list[0]);
            Assert.AreEqual("1", list[1]);
            Assert.AreEqual("2", list[2]);
            Assert.AreEqual(string.Empty, list[3]);
        }

        [TestMethod]
        public void SimpleBufferNextChar()
        {
            var buffer = new StringBuffer("hi");
            var ch = buffer.NextChar();
            Assert.AreEqual('h', ch);

            ch = buffer.NextChar();
            Assert.AreEqual('i', ch);

            ch = buffer.NextChar();
            Assert.AreEqual(ch, StringBuffer.Eof);
        }
    }
}
