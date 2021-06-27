using System.IO;

namespace Opal
{
    public class Utf8Reader : ReaderBase
    {
        public Utf8Reader(Stream stream)
            : base(stream)
        {
        }

        public override int Read()
        { 
            var ch = ReadByte();
            if (ch == Eof)
                return Eof;
            if (ch < 0b1000_0000)
                return ch;
            if (ch < 0b1110_0000)
                return Read12Bit(ch);

            if (ch < 0b1111_0000)
                return Read16Bit(ch);

            if (ch < 0b1111_1000)
                return Read21Bits(ch);

            return ReplacementCharacter;
        }

        private int Read12Bit(int ch)
        {
            var ch2 = ReadByte();
            if (ch2 == Eof)
                return Eof;
            if ((ch2 >> 6) != 0b10)
                return ReplacementCharacter;
            return (ch2 & 0x3F) + ((ch & 0x1F) << 6);
        }

        private int Read16Bit(int ch)
        {
            var ch2 = ReadByte();
            if (ch2 == Eof)
                return Eof;
            if ((ch2 >> 6) != 0b10)
                return ReplacementCharacter;

            var ch3 = ReadByte();
            if (ch3 == Eof)
                return Eof;
            if ((ch3 >> 6) != 0b10)
                return ReplacementCharacter;
            return (ch3 & 0x3F) + ((ch2 & 0x3F) << 6) + ((ch & 0xF) << 12);
        }

        private int Read21Bits(int ch)
        {
            var ch2 = ReadByte();
            if (ch2 == Eof)
                return Eof;
            if ((ch2 >> 6) != 0b10)
                return ReplacementCharacter;

            var ch3 = ReadByte();
            if (ch3 == Eof)
                return Eof;
            if ((ch3 >> 6) != 0b10)
                return ReplacementCharacter;

            var ch4 = ReadByte();
            if (ch4 == Eof)
                return Eof;
            if ((ch4 >> 6) != 0b10)
                return ReplacementCharacter;

            return (ch4 & 0x3F) + ((ch3 & 0x3F) << 6) + ((ch2 & 0x3F) << 12) +
                ((ch & 0x7) << 18);
        }
    }
}
