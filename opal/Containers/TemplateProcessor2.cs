using Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Opal.Containers
{
	public class TemplateProcessor2
	{
		private string templ;

		public TemplateProcessor2(string templ) =>
			this.templ = templ;

        public static void FromFile(Generator generator, 
			ITemplateContext context, 
			string filePath)
        {
            var templ = File.ReadAllText(filePath);
			var processor = new TemplateProcessor2(templ);
			processor.Format(generator, context);
        }

        public static void FromAssembly(Generator generator, 
			ITemplateContext context, 
			string name)
		{
			var templ = Resources.LoadText(name);
			var processor = new TemplateProcessor2(templ);
			processor.Format(generator, context);
		}

		public void Format(Generator generator, 
			ITemplateContext context)
		{
			var scanner = new TemplScanner(templ);
			var stack = new Stack<bool>();
			var write = true;

			var formatContext = new FormatContext(generator, context);

			while (true)
			{
				var token = scanner.Token();
				if (token.IsEof)
					break;
				token.Write(formatContext);
			}
		}

		public bool ParseMacro(Generator generator,
			string macro, 
			Stack<bool> stack, 
			bool write,
			ITemplateContext context)
        {
			var symbols = new List<string>();
			var scanner = new MacroScanner(macro);
			MacroScanner.IMacroToken token;
			do
			{
				token = scanner.Token();
			} while (token.AddTo(symbols));

			if (symbols.Count == 0)
				return write;

			switch (symbols[0])
            {
				case "if":
					if (symbols.Count == 2)
                    {
						stack.Push(write);
						write = write && context.Condition(symbols[1]);
					}
					else if (symbols.Count > 2 && write)
                    {
						if (symbols[2] == "include")
                        {
							for (var i = 3; i < symbols.Count; i++)
								Include(generator, context, symbols[i]);
						}
						else
                        {
							for (var i = 2; i < symbols.Count; i++)
								context.WriteVariable(generator, symbols[i]);
						}
					}
					break;
				case "include":
					if (write)
                    {
						for (var i = 1; i < symbols.Count; i++)
							Include(generator, context, symbols[i]);
                    }
					break;
				case "endif":
					if (stack.Count > 0)
						write = stack.Pop();
					break;
				default:
					if (write)
					{
						foreach (var symbol in symbols)
							context.WriteVariable(generator, symbol);
					}
					break;
            }
			return write;
        }

		private void Include(Generator generator, 
			ITemplateContext context,
			string name)
        {
			var text = context.Include(name);
			if (string.IsNullOrEmpty(text))
			{
				FromAssembly(
					generator,
					context,
					name);
			}
			else
			{
				var processor = new TemplateProcessor2(text);
				processor.Format(generator, context);
			}
		}

		private enum TokenType
		{
			End,
			Text,
			Macro
		}

		class FormatContext
        {
			private readonly Stack<bool> writeStack;

			public FormatContext(Generator generator,
				ITemplateContext templateContext)
            {
				Generator = generator;
				TemplateContext = templateContext;
				writeStack = new Stack<bool>();
				Write = true;
            }

			public Generator Generator { get; }
			public ITemplateContext TemplateContext { get; }

			public bool Write { get; private set; }

			public void WriteVar(string name) => 
				TemplateContext.WriteVariable(Generator, name);

			public void Include(string name)
			{
				var text = TemplateContext.Include(name);
				if (string.IsNullOrEmpty(text))
					text = Resources.LoadText(name);
				
				var processor = new TemplateProcessor2(text);
				processor.Format(Generator, TemplateContext);
			}

			public void Push(bool value)
            {
				writeStack.Push(Write);
				Write = value;
            }
			public void Pop()
            {
				if (writeStack.Count > 0)
					Write = writeStack.Pop();
            }

			public void WriteBlock(string text)
            {
				if (Write)
					Generator.WriteBlock(text);
			}
		}

		public class ScannerBase
        {
			private readonly SimpleStringBuffer buffer;
			protected readonly StringBuilder builder;
			protected char ch;

			protected ScannerBase(string text)
            {
				buffer = new SimpleStringBuffer(text);
				builder = new StringBuilder();
				ch = buffer.NextChar();
			}

			protected char NextChar()
			{
				ch = buffer.NextChar();
				return ch;
			}
			
			protected char PushChar()
            {
				builder.Append(ch);
				ch = buffer.NextChar();
				return ch;
            }
		}

		class TemplScanner: ScannerBase
		{
			public TemplScanner(string text)
				: base(text)
			{}

			public IToken Token()
			{
				builder.Length = 0;
				if (ch == SimpleStringBuffer.Eof) return new EofToken();
				if (ch == '$') goto Macro1;

				TextToken:
				PushChar();
				if (ch == SimpleStringBuffer.Eof || ch == '$')
					return new TextToken(builder.ToString());
				goto TextToken;

			Macro1: // $
				switch (NextChar())
				{
					case SimpleStringBuffer.Eof:
						builder.Append('(');
						return new TextToken(builder.ToString());
					case '(':
						goto Macro2;
					default:
						builder.Append('(');
						goto TextToken;
				}

			Macro2: // $(
				switch (NextChar())
				{
					case SimpleStringBuffer.Eof:
						builder.Append("$(");
						goto TextToken;
					case ')':
						builder.Append("$(");
						goto TextToken;
				}

			Macro3: // $(...
				switch (PushChar())
				{
					case ')': goto MacroEnd;
					case SimpleStringBuffer.Eof:
						builder.Insert(0, "$(");
						goto TextToken;
					default:
						goto Macro3;
				}

			MacroEnd: // $(...)
				NextChar();
				return new MacroToken(builder.ToString());
			}

			public interface IToken
            {
				void Write(FormatContext context);
				bool IsEof { get; }
            }

			public class TextToken : IToken
			{
				private readonly string text;

				public TextToken(string text) =>
					this.text = text;

				public bool IsEof => false;

				public void Write(FormatContext context) =>
					context.WriteBlock(text);
			}

			public class MacroToken: IToken
            {
				private readonly List<string> symbols;
				public MacroToken(string text)
				{
					symbols = new List<string>();
					var scanner = new MacroScanner(text);
					MacroScanner.IMacroToken token;
					do
					{
						token = scanner.Token();
					} while (token.AddTo(symbols));
				}

				public bool IsEof => false;

				public void Write(FormatContext context)
                {
					if (symbols.Count == 0)
						return;

					switch (symbols[0])
					{
						case "if":
							if (symbols.Count == 2)
							{
								var write = context.Write && 
									context.TemplateContext.Condition(symbols[1]);
								context.Push(write);
							}
							else if (symbols.Count > 2 && context.Write)
							{
								if (symbols[2] == "include")
								{
									for (var i = 3; i < symbols.Count; i++)
										context.Include(symbols[i]);
								}
								else
								{
									for (var i = 2; i < symbols.Count; i++)
										context.WriteVar(symbols[i]);
								}
							}
							break;
						case "include":
							if (context.Write)
							{
								for (var i = 1; i < symbols.Count; i++)
									context.Include(symbols[i]);
							}
							break;
						case "endif":
							context.Pop();
							break;
						default:
							if (context.Write)
							{
								foreach (var symbol in symbols)
									context.WriteVar(symbol);
							}
							break;
					}
				}
			}

			public class EofToken: IToken
            {
				public bool IsEof => true;

				public void Write(FormatContext context)
				{ }
            }

		}

		public class MacroScanner: ScannerBase
        {
			public MacroScanner(string text)
				: base(text)
			{
			}

			public IMacroToken Token()
			{
				builder.Length = 0;
			Start:
				if (ch == SimpleStringBuffer.Eof)
					return new EofToken();
				if (ch == ',') goto Comma;
				if (char.IsWhiteSpace(ch))
				{
					NextChar();
					goto Start;
				}
			Symbol:
				PushChar();
				if ((ch == SimpleStringBuffer.Eof) || char.IsWhiteSpace(ch))
					return new SymbolToken(builder.ToString());
				if (ch == ',')
					goto Comma;
				goto Symbol;

			Comma:
				NextChar();
				return new SymbolToken(builder.ToString());
			}

			public interface IMacroToken
			{
				bool AddTo(IList<string> symbols);
			}

			class SymbolToken : IMacroToken
			{
				private readonly string name;

				public SymbolToken(string name) => this.name = name;

				public bool AddTo(IList<string> symbols)
				{
					symbols.Add(name);
					return true;
				}
			}

			class EofToken : IMacroToken
			{
				public bool AddTo(IList<string> symbols) => false;
			}
		}
	}
}
