using System;
using System.IO;

namespace Opal
{
    public abstract class ReaderBase : IDisposable
    {
        protected readonly Stream stream;
        public const int ReplacementCharacter = 0xFFFD;
        public const int ZeroWidthNoBreakSpace = 0xFEFF;
        public const int Eof = -1;

        protected ReaderBase(Stream stream)
        {
            this.stream = stream;
        }

        public void Dispose()
        {
            stream.Dispose();
            GC.SuppressFinalize(this);
        }

        public long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        protected int ReadByte() => stream.ReadByte();

        public abstract int Read();

        public virtual void SkipBOM()
        {
            var oldPosition = stream.Position;
            if (Read() != ZeroWidthNoBreakSpace)
                stream.Position = oldPosition;
        }
    }
}
