using System.IO;

namespace Opal
{
    public class AnsiReader : ReaderBase
    {
        public AnsiReader(Stream stream)
            : base(stream)
        {
        }

        public override int Read() => ReadByte();

        public override void SkipBOM()
        {
        }
    }

}
