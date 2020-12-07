using System;
using System.IO;

namespace Opal
{
    public abstract class Reader: IDisposable
    {
        protected MemoryStream stream;

        public Reader(byte[] compressed)
        {
            stream = new MemoryStream();
            stream.DecompressFrom(compressed);
            stream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            stream.Dispose();
            GC.SuppressFinalize(this);
        }

        public abstract int Read();
    }

    public class Reader8: Reader
    {
        public Reader8(byte[] compressed): base(compressed) { }

        public override int Read() => stream.Read8();
    }

    public class Reader16: Reader
    {
        public Reader16(byte[] compressed) : base(compressed) { }

        public override int Read() => stream.Read16();
    }

    public class Reader24: Reader
    {
        public Reader24(byte[] compressed) : base(compressed) { }

        public override int Read() => stream.Read24();
    }

    public class Reader32: Reader
    {
        public Reader32(byte[] compressed) : base(compressed) { }

        public override int Read() => stream.Read32();
    }
}
