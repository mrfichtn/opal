using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal;
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
    public class BufferPerfTests
    {
        [TestMethod]
        public void Test()
        {
            var inFilePath = @"d:\src\opal\tests\opal.txt";

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                var text = File.ReadAllText(inFilePath);
                var buffer = new StringBuffer(text);
                var scanner = new Scanner(buffer);
                while (scanner.NextToken().State != TokenStates.Empty)
                {
                }
            }
            //var elapsed = sw.Elapsed;

            sw = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                using var buffer = new FileBuffer(inFilePath);
                var scanner = new Scanner(buffer);
                while (scanner.NextToken().State != TokenStates.Empty)
                {
                }
            }

            //var elapsed2 = sw.Elapsed;
        }

        [TestMethod]
        public void TestBuffer()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("44+4+4");
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new FileBuffer(stream);
            var scanner = new Scanner(buffer);
            var token = scanner.NextToken();
            Assert.AreEqual("44", token.Value);
            token = scanner.NextToken();
            Assert.AreEqual("+", token.Value);
            token = scanner.NextToken();
            Assert.AreEqual("4", token.Value);
            token = scanner.NextToken();
            Assert.AreEqual("+", token.Value);
            token = scanner.NextToken();
            Assert.AreEqual("4", token.Value);
            token = scanner.NextToken();
            Assert.AreEqual(string.Empty, token.Value);
        }
    }
}
