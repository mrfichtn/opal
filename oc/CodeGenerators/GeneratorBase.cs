using System;
using System.IO;

namespace Generators
{
    public class GeneratorBase: IDisposable
    {
        private bool isDisposed;
        private readonly bool ownsStream;
        private readonly TextWriter stream;
        protected int indent;
        protected bool indented;

        public GeneratorBase(TextWriter stream, bool ownsStream = true)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.ownsStream = ownsStream;
            Line = 1;
        }

        public GeneratorBase(GeneratorBase generator)
            : this(generator.stream, false)
        {
            indent = generator.indent;
            indented = generator.indented;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing && ownsStream)
                    stream.Dispose();
                isDisposed = true;
            }
        }

        public int Line { get; set; }

        public void Close() => stream.Close();

        public override string? ToString() => stream.ToString();

        public void WriteIndent()
        {
            if (indented == false)
            {
                for (int i = 0; i < indent; i++)
                    stream.Write('\t');
                indented = true;
            }
        }

        public void WriteText(string text) =>
            stream.Write(text);

        public void WriteChar(char ch) =>
            stream.Write(ch);

        public void NewLine()
        {
            stream.WriteLine();
            Line++;
            indented = false;
        }
    }
}
