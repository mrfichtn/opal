using Opal;
using System;
using System.IO;
using System.Text;

namespace OpalTests
{
    public class FileBuffer : IBuffer, IDisposable
    {
        private readonly StreamReader _reader;
        private StringBuilder _builder;
        private int _filePos;
        private int _remaining;
        private readonly long _fileLength;

        public FileBuffer(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        public FileBuffer(Stream stream)
        {
            _fileLength = stream.Length;
            _reader = new StreamReader(stream);
            _builder = new StringBuilder();
        }

        public int Position
        {
            get { return _filePos - _remaining; }
            set
            {
                _remaining = _filePos - value;
            }

        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public string GetString(int beg, int end)
        {
            var length = end - beg;
            var pos = _filePos - _remaining;
            var start = pos - beg;
            var shift = _builder.Length - start;
            var result = _builder.ToString(shift, length);
            _builder.Remove(shift, length);
            return result;
        }

        public int Peek()
        {
            int result;
            if (_remaining > 0)
            {
                result = _builder[_builder.Length - _remaining];
            }
            else if (_filePos < _fileLength)
            {
                result = _reader.Read();
                _builder.Append(result);
                _filePos++;
                _remaining++;
            }
            else
            {
                result = -1;
            }
            return result;
        }

        public int Read()
        {
            int result;
            if (_remaining > 0)
            {
                result = _builder[_builder.Length - _remaining--];
            }
            else if (_filePos < _fileLength)
            {
                result = _reader.Read();
                _builder.Append((char)result);
                _filePos++;
            }
            else
            {
                result = -1;
            }
            return result;
        }
    }
}
