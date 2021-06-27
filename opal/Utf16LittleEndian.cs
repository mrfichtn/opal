using System.IO;

namespace Opal
{
    public class Utf16LittleEndian : ReaderBase
    {
        public Utf16LittleEndian(Stream stream)
            : base(stream)
        { }

        public override int Read()
        {
            var b0 = ReadByte();
            if (b0 == Eof)
                return Eof;
            var b1 = ReadByte();
            if (b1 == Eof)
                return Eof;

            return (b0 << 8) | b1;
        }
    }
}
