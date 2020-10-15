using Generators;
using System;
using System.IO;
using System.Text;

namespace Opal.Containers
{
	public class TemplateProcessor
	{
		private readonly string _templ;
		private int _pos;
		private int _last;
		private char _ch;
		private const char Eof = char.MaxValue;

		public enum TokenType
		{
			End,
			Text,
			Var,
			SynError
		}

		protected TemplateProcessor(string templ)
        {
			_templ = templ;
        }

		#region Properties

		#region Line Property
		public int Line
		{
			get { return _line; }
		}
		private int _line;
		#endregion

		#region Column Property

		public int Column
		{
			get { return _column; }
		}
		private int _column;

        #endregion

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
			_last = _templ.Length - 1;
			_pos = -1;
			_ch = NextChar();
			_line = 1;
			_column = 0;

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
						if (!provider.AddVarValue(generator, varName))
							generator.Write("(. {0} .)", varName);
						break;
					case TokenType.End:
						return;

					case TokenType.SynError:
						var msg = string.Format("({0},{1}): syntax error", _line, _column);
						throw new Exception(msg);
				}
			}
		}

		public static string GetTextFromAssembly(string name)
		{
			var assm = typeof(TemplateProcessor).Assembly;
            using (var stream = assm.GetManifestResourceStream(name))
            {
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
		}

		private TokenType GetToken(StringBuilder builder)
		{
			builder.Length = 0;
            if (_ch == Eof)
                return TokenType.End;
            else if (_ch == '(')
                goto Var1;

			TextToken:
				builder.Append(_ch);
				_ch = NextChar();
                if (_ch == Eof || _ch == '(')
                    return TokenType.Text;
                goto TextToken;

			Var1: // (
				_ch = NextChar();
				switch (_ch)
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
				_ch = NextChar();
				switch (_ch)
				{
				case ' ':
				case '\t':
					goto Var2;
				case '.':
					return TokenType.SynError;
				case ')':
					builder.Append("(.");
					_ch = NextChar();
					goto TextToken;
				default:
					goto Var3;
				}

			Var3: // (. id
				builder.Append(_ch);
				_ch = NextChar();
				switch (_ch)
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
				_ch = NextChar();
				switch (_ch)
				{
					case ' ':
					case '\t':
						goto Var6;
					case '.': goto Var5;
					default:
						return TokenType.SynError;
				}

			Var5: // (. id .
				_ch = NextChar();
				switch (_ch)
				{
				case ' ':
				case '\t':
					return TokenType.SynError;
				case ')':
					_ch = NextChar();
					return TokenType.Var;
				default:
                    builder.Append('.');
					goto Var3;
				}

			Var6: // (. id .
				_ch = NextChar();
				switch (_ch)
				{
					case ')':
						_ch = NextChar();
						return TokenType.Var;
				}
			return TokenType.SynError;
		}

		private char NextChar()
		{
			char ch;
			if (_pos < _last)
			{
				_pos++;
				ch = _templ[_pos];
				if (ch == '\n')
				{
					_column = 1;
					_line++;
				}
				else
				{
					_column++;
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
