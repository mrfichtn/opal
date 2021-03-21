using Generators;
using System;
using System.IO;
using System.Text;

namespace Opal.Containers
{
	public class TemplateProcessor
	{
		private const char Eof = char.MaxValue;

		private readonly string templ;
		private int pos;
		private int last;
		private char ch;

		public enum TokenType
		{
			End,
			Text,
			Var,
			SynError
		}

		protected TemplateProcessor(string templ) =>
			this.templ = templ;

		#region Properties

		public int Line { get; private set; }

		public int Column { get; private set; }

        #endregion

        public static void FromFile(Generator generator, IVarProvider provider, string filePath)
        {
            var templ = File.ReadAllText(filePath);
			var processor = new TemplateProcessor(templ);
			processor.Format(generator, provider);
        }

        public static void FromAssembly(Generator generator, IVarProvider provider, string name)
		{
			var templ = GetTextFromAssembly(name);
			var processor = new TemplateProcessor(templ);
			processor.Format(generator, provider);
		}

		public void Format(Generator generator, IVarProvider provider)
		{
			last = templ.Length - 1;
			pos = -1;
			ch = NextChar();
			Line = 1;
			Column = 0;

			var builder = new StringBuilder();
			while (true)
			{
				switch (GetToken(builder))
				{
					case TokenType.Text:
						generator.WriteBlock(builder.ToString());
						break;
					case TokenType.Var:
						var varName = builder.ToString();
						if (!provider.WriteVariable(generator, varName))
							generator.Write("(. {0} .)", varName);
						break;
					case TokenType.End:
						return;

					case TokenType.SynError:
						throw new Exception($"({Line},{Column}): syntax error");
				}
			}
		}

		public static string GetTextFromAssembly(string name)
		{
			var assm = typeof(TemplateProcessor).Assembly;
            using var stream = assm.GetManifestResourceStream(name);
            if (stream == null)
                return string.Empty;

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

		private TokenType GetToken(StringBuilder builder)
		{
			builder.Length = 0;
            if (ch == Eof)
                return TokenType.End;
            else if (ch == '(')
                goto Var1;

			TextToken:
				builder.Append(ch);
				ch = NextChar();
                if (ch == Eof || ch == '(')
                    return TokenType.Text;
                goto TextToken;

			Var1: // (
				ch = NextChar();
				switch (ch)
				{
				case Eof:
					builder.Append('(');
					return TokenType.Text;
				case '.':
					goto Var2;
				default:
					builder.Append('(');
					goto TextToken;
				}

			Var2: // (.
				ch = NextChar();
				switch (ch)
				{
				case ' ':
				case '\t':
					goto Var2;
				case '.':
					return TokenType.SynError;
				case ')':
					builder.Append("(.");
					ch = NextChar();
					goto TextToken;
				default:
					goto Var3;
				}

			Var3: // (. id
				builder.Append(ch);
				ch = NextChar();
				switch (ch)
				{
				case ' ':
				case '\t':
					goto Var4;
				case '.':
					goto Var5;
				default:
					goto Var3;
				}

			Var4: // (. id space
				ch = NextChar();
				switch (ch)
				{
					case ' ':
					case '\t':
						goto Var6;
					case '.': goto Var5;
					default:
						return TokenType.SynError;
				}

			Var5: // (. id .
				ch = NextChar();
				switch (ch)
				{
				case ' ':
				case '\t':
					return TokenType.SynError;
				case ')':
					ch = NextChar();
					return TokenType.Var;
				default:
                    builder.Append('.');
					goto Var3;
				}

			Var6: // (. id .
				ch = NextChar();
				switch (ch)
				{
					case ')':
						ch = NextChar();
						return TokenType.Var;
				}
			return TokenType.SynError;
		}

		private char NextChar()
		{
			char ch;
			if (pos < last)
			{
				pos++;
				ch = templ[pos];
				if (ch == '\n')
				{
					Column = 1;
					Line++;
				}
				else
				{
					Column++;
				}
			}
			else
			{
				ch = Eof;
			}
			return ch;
		}
	}
}
