using System;
using System.IO;

namespace Generators
{
    public class Generator: GeneratorBase, IGenerator
    {
        public Generator(TextWriter stream, bool ownsStream = true)
            : base(stream, ownsStream)
        {
        }

        public Generator()
        {
        }

        public Generator(string path)
            : base(path)
        {
        }

        public Generator(Generator generator)
			: base(generator)
        {
        }

        public IGenerator StartBlock()
        {
            WriteLine("{");
            Indent();
			return this;
        }

        public IGenerator EndBlock()
        {
            UnIndent();
            WriteLine("}");
			return this;
        }

        public IGenerator EndBlock(string extraText)
        {
            UnIndent();
            WriteLine("}}{0}", extraText);
			return this;
        }

        public IGenerator Indent(int indent = 1)
        {
            _indent += indent;
			return this;
        }

        public Generator UnIndent(int indent = 1)
        {
            _indent -= indent;
            return this;
        }

        IGenerator IGenerator.UnIndent(int indent) => UnIndent(indent);

        public IGenerator Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WriteIndent();
                _stream.Write(value);
            }
            return this;
        }

        public Generator Write(char value)
        {
            if ((value == '\r') || (value == '\n'))
            {
                _indented = false;
            }
            else
            {
                WriteIndent();
            }
            _stream.Write(value);
            return this;
        }

        IGenerator IGenerator.Write(char value) => Write(value);

        public IGenerator Write(string format, params object[] args)
        {
            WriteIndent();
            _stream.Write(format, args);
            return this;
        }
        
        public Generator WriteLine(string value)
        {
            WriteIndent();
            _stream.WriteLine(value);
            Line++;
            _indented = false;
			return this;
        }
        
        public Generator WriteLine(string format, params object[] args)
        {
            WriteIndent();
            _stream.WriteLine(format, args);
            _indented = false;
			return this;
        }

        IGenerator IGenerator.WriteLine(string value) => WriteLine(value);

        public IGenerator WriteLine()
        {
            _stream.WriteLine();
            _indented = false;
            Line++;
            return this;
        }

        public IGenerator WriteBlock(string block)
        {
            if (!string.IsNullOrEmpty(block))
            {
                for (int i = 0; i < block.Length; i++)
                {
                    var ch = block[i];
                    if (ch == '\n')
                        _indented = false;
                    else
                        WriteIndent();
                    _stream.Write(ch);
                }
            }
            return this;
        }
        public Generator WriteEmptyBlock()
        {
            WriteLine("{");
            WriteLine("}");
			return this;
        }

		public Generator Write(IGeneratable generatable)
		{
			if (generatable != null)
                generatable.Write(this);
			return this;
		}
        IGenerator IGenerator.Write(IGeneratable generatable) => Write(generatable);
    }
}
