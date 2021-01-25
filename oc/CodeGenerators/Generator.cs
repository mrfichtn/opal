using System.IO;

namespace Generators
{
    public class Generator<T>: GeneratorBase
        where T : Generator<T>
    {
        protected readonly T self;

        public Generator(TextWriter stream, bool ownsStream = true)
            : base(stream, ownsStream)
        {
            self = (this as T)!;
        }

        public Generator(string path)
            : this(new StreamWriter(path))
        {
        }

        public Generator()
            : this(new StringWriter())
        { }

        public Generator(GeneratorBase generator)
            : base(generator)
        {
            self = (this as T)!;
        }


        public T StartBlock()
        {
            WriteLine("{");
            Indent();
            return self;
        }

        public T EndBlock()
        {
            UnIndent();
            WriteLine("}");
            return self;
        }

        public T EndBlock(string extraText)
        {
            UnIndent();
            WriteLine("}}{0}", extraText);
            return self;
        }

        public T Indent(int indent = 1)
        {
            base.indent += indent;
            return self;
        }

        public T UnIndent(int indent = 1)
        {
            base.indent -= indent;
            return self;
        }

        public T Write(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WriteIndent();
                WriteText(value);
            }
            return self;
        }

        public T Write(char value)
        {
            if ((value == '\r') || (value == '\n'))
                indented = false;
            else
                WriteIndent();
            WriteChar(value);
            return self;
        }

        public T Write(string format, params object[] args)
        {
            WriteIndent();
            WriteText(string.Format(format, args));
            return self;
        }

        public T WriteLine(string value)
        {
            WriteIndent();
            WriteText(value);
            NewLine();
            return self;
        }

        public T WriteLine(string format, params object[] args)
        {
            WriteIndent();
            WriteText(string.Format(format, args));
            NewLine();
            return self;
        }

        public T WriteLine()
        {
            NewLine();
            return self;
        }

        public T WriteBlock(string block)
        {
            if (!string.IsNullOrEmpty(block))
            {
                for (int i = 0; i < block.Length; i++)
                {
                    var ch = block[i];
                    if (ch == '\n')
                        indented = false;
                    else
                        WriteIndent();
                    WriteChar(ch);
                }
            }
            return self;
        }
        public T WriteEmptyBlock()
        {
            WriteLine("{");
            WriteLine("}");
            return self;
        }
    }


    public class Generator: Generator<Generator>
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

        public Generator(GeneratorBase generator)
			: base(generator)
        {
        }
    }
}
