using System;
using System.IO;

namespace Generators
{
    public class GeneratorBase: IDisposable
    {
        private bool _isDisposed;
        private readonly bool _ownsStream;
        protected readonly TextWriter _stream;
        protected int _indent;
        protected bool _indented;

        public GeneratorBase(TextWriter stream, bool ownsStream = true)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _ownsStream = ownsStream;
            Line = 1;
        }

        public GeneratorBase()
            : this(new StringWriter(), true)
        {
        }

        public GeneratorBase(string path)
            : this(new StreamWriter(path))
        {
        }

        public GeneratorBase(GeneratorBase generator)
            : this(generator?._stream, false)
        {
            _indent = generator._indent;
            _indented = generator._indented;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing && _ownsStream)
                    _stream.Dispose();
                _isDisposed = true;
            }
        }

        #region Properties
        public int Line { get; set; }
        #endregion

        public void Close()
        {
            _stream.Close();
        }

        public override string ToString()
        {
            return _stream.ToString();
        }

        protected void WriteIndent()
        {
            if (_indented == false)
            {
                for (int i = 0; i < _indent; i++)
                    _stream.Write('\t');
                _indented = true;
            }
        }
    }
}
