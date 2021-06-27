using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal;
using System;
using System.IO;
using System.Text;

namespace OpalTests
{
    [TestClass]
    public class ReaderTests
    {
        [TestMethod]
        public void TestUtf8()
        {
            TestReader(Encoding.UTF8, s => new Utf8Reader(s));
        }

        private void TestReader(Encoding encoding,
            Func<Stream, ReaderBase> createReader)
        {
            var buffer = new MemoryStream();
            var writer = new StreamWriter(buffer, encoding);
            writer.Write(TestFile);
            writer.Flush();

            buffer.Position = 0;
            var reader = createReader(buffer);
            reader.SkipBOM();

            int ch = 0;
            for (var i = 0; i < TestFile.Length; i++)
            {
                ch = reader.Read();
                Assert.AreEqual((int)TestFile[i], ch);
            }
            ch = reader.Read();
            Assert.AreEqual(-1, ch);
        }


        private static readonly string TestFile = 
@"Line 1
Line 2
Line 3
";
    }
}
