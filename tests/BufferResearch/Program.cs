using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal;
using System;
using System.IO;
using System.Text;

namespace BufferResearch
{
    class Program
    {
        private static readonly string fileName = "ansi.txt";
        
        static void Main(string[] args)
        {
            using (var buffer = new FileBuffer(fileName))
            {
                var pos = buffer.Position;
                var c1 = buffer.Read();
                Assert.AreEqual('t', c1);
                var c2 = buffer.Read();
                Assert.AreEqual('e', c2);

                var end = buffer.Position;
                var s1 = buffer.GetToken(end);
                Assert.AreEqual("te", s1);

                var s = buffer.PeekLine();
                Assert.AreEqual("st file", s);

                while (true)
                {
                    var ch = buffer.Read();
                    if (ch == -1)
                        break;
                }

                var p = new Position(2, 1, 100);
                s = buffer.Line(p);
                Assert.AreEqual("line 2", s);
            }

            
            var memBuffer = new MemoryStream(new byte[] { 0xEF, 0xBB, 0xBF });
            using (var buffer = new FileBuffer(memBuffer, EncodingType.Utf8))
            {
                var ch = buffer.Read();
            }

            memBuffer = new MemoryStream();
            var writer = new StreamWriter(memBuffer, Encoding.UTF8);
            writer.WriteLine("h");
            writer.Flush();

            var arr = memBuffer.ToArray();

        }

        static void CharTests()
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                //UTF8                  EF BB BF
                //UTF-16 big-endian     FE FF
                //UTF-16 little-endian  FF FE
                //UTF-32 big-endian     00 00 FE FF
                //UTF-32 little-endian  FF FE 00 00


                var b = file.ReadByte();
                var b2 = file.ReadByte();
                var b3 = file.ReadByte();

                var data = new byte[] { 0xC0, 0x0 };
                var s = Encoding.UTF8.GetString(data);
                var ch = s[0];

                data = new byte[] { 0xC0, 0x80 };
                s = Encoding.UTF8.GetString(data);
                ch = s[0];


                for (var i = 0; i < 0xD7FF; i++)
                {
                    var expectedArray = new char[1];
                    var chExpected = expectedArray[0] = (char)i;  //'°';
                    data = Encoding.UTF8.GetBytes(expectedArray);

                    var chActual = (char)GetFromUtf8(data);
                    Assert.AreEqual(chExpected, chActual);
                }
            }
        }

        static int GetFromUtf8(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var reader = new Utf8Reader(stream);
            var ch = reader.Read();
            return ch;
        }

        static void TestBuffer()
        {
            var buffer = new FileBuffer("ansi.txt");
            var ch = buffer.Read();
            Assert.AreEqual('t', ch);

            var pos = buffer.Position;

            ch = buffer.Read();
            Assert.AreEqual('e', ch);

            var pos2 = buffer.Position;
            var text = buffer.GetToken(2);
            Assert.AreEqual(text, "te");

        }
    }
}
