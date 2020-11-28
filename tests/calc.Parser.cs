using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq.Expressions;


namespace CalcTest
{
	public partial class Parser: IDisposable
	{
		private readonly ILogger _logger;
		private readonly Scanner _scanner;
		private readonly LRStack _stack;
		private bool _hasErrors;
			
		public Parser(ILogger logger, Scanner scanner)
		{
			_logger = logger;
			_scanner = scanner;
			_stack = new LRStack();
			Init();
		}
	
		public Parser(Scanner scanner)
			: this(new ConsoleLogger(scanner.FilePath), scanner) {}
	
		public Parser(ILogger logger, string filePath)
			: this(logger, Scanner.FromFile(filePath)) {}
	
		public Parser(string filePath)
			: this(Scanner.FromFile(filePath))
		{
		}
	
		public static Parser FromString(string text)
		{
			var scanner = new Scanner(text);
			return new Parser(scanner);
		}
	
		public void Dispose()
		{
			_scanner.Dispose();
		}
	
		public object Root { get; private set; }
	
		partial void Init();
	
		public bool Parse()
		{
			var token = NextToken();
			var errorState = 0U;
			var suppressError = 0;
	
			while (true)
			{
	            if (suppressError > 0) --suppressError;
				
				var state = _stack.PeekState();
				var actionType = GetAction(state, (uint) token.State, out var result);
	
				switch (actionType)
				{
					case ActionType.Error:
	                    _hasErrors = true;
	                    if (!TryRecover(ref token, (suppressError > 0)))
	                        return false;
	                    if (token.State == 0)
	                    {
	                        if (errorState == state)
	                            return false;
	                        else
	                            errorState = state;
	                    }
	                    suppressError = 2;
		                break;
	
					case ActionType.Reduce:
						var rule = result;
						var reducedState = Reduce(rule);
						if (rule == 0)
						{
							Root = _stack.PopValue();
							return !_hasErrors;
						}
						if (GetAction(reducedState, _stack.PeekState(), out result) == ActionType.Error)
							goto case ActionType.Error;
						_stack.Replace((uint)result);
						break;
	
					case ActionType.Shift:
						_stack.Shift(result, token);
						token = NextToken();
						break;
				}
			}
		}
	
	    private Token NextToken()
	    {
	        while (true)
	        {
	            var token = _scanner.NextToken();
	            if (token.State >= 0)
	                return token;
	            _logger.LogError(token, "syntax error - {0}", token.Value);
	            _hasErrors = true;
	        }
	    }
	
		private uint Reduce(uint rule)
		{
			uint state = 0;
	
			switch (rule)
			{
				case 1: // expr = term;
					items = 1;
					state = Reduce(8, At(0));
					break;
				case 2: // expr = expr "+" term;
					items = 3;
					state = Reduce(8, new AddExpr(At<Expr>(0),At(2)));
					break;
				case 3: // expr = expr "-" term;
					items = 3;
					state = Reduce(8, new SubExpr(At<Expr>(0),At(2)));
					break;
				case 4: // term = primary;
					items = 1;
					state = Reduce(9, At(0));
					break;
				case 5: // term = term "*" primary;
					items = 3;
					state = Reduce(9, new MultiExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 6: // term = term "/" primary;
					items = 3;
					state = Reduce(9, new DivExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 7: // primary = Int;
					items = 1;
					state = Reduce(10, new Constant(At<Token>(0)));
					break;
	
			}
			return state;
		}
	
		private enum ActionType { Shift, Reduce, Error }
	
		private ActionType GetAction(uint state, uint token, out uint arg)
		{
			var action = _actions[state, token];
			ActionType actionType;
			if (action == -1)
			{
				actionType = ActionType.Error;
			}
			else if (action >= 0)
			{
				actionType = ActionType.Shift;
			}
			else
			{
				actionType = ActionType.Reduce;
				action = -2 - action;
			}
			arg = (uint)action;
			return actionType;
		}
	
		#region Actions Table
		private readonly int[,] _actions = 
		{
			{ -1, 4, -1, -1, -1, -1, -1, -1, 1, 2, 3 },
			{ -2, -1, -1, -1, 5, 6, -1, -1, -1, -1, -1 },
			{ -3, -1, -1, -1, -3, -3, 7, 8, -1, -1, -1 },
			{ -6, -1, -1, -1, -6, -6, -6, -6, -1, -1, -1 },
			{ -9, -1, -1, -1, -9, -9, -9, -9, -1, -1, -1 },
			{ -1, 4, -1, -1, -1, -1, -1, -1, -1, 9, 3 },
			{ -1, 4, -1, -1, -1, -1, -1, -1, -1, 10, 3 },
			{ -1, 4, -1, -1, -1, -1, -1, -1, -1, -1, 11 },
			{ -1, 4, -1, -1, -1, -1, -1, -1, -1, -1, 12 },
			{ -4, -1, -1, -1, -4, -4, 7, 8, -1, -1, -1 },
			{ -5, -1, -1, -1, -5, -5, 7, 8, -1, -1, -1 },
			{ -7, -1, -1, -1, -7, -7, -7, -7, -1, -1, -1 },
			{ -8, -1, -1, -1, -8, -8, -8, -8, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		#region Symbols
		protected const int _maxTerminal = 7;
		protected readonly string[] _symbols =
		{
			"𝜖",
			"Int",
			"identifier",
			"white_space",
			"+",
			"-",
			"*",
			"/",
			"expr",
			"term",
			"primary"
		};
		#endregion
	
		#endregion
		
	
	    private bool TryRecover(ref Token token, bool suppress)
	    {
	        var isOk = false;
	
	        if (token.State != 0)
	            _logger.LogError(token, "unexpected token '{0}'", token.Value);
	        else
	            _logger.LogError(token, "unexpected token [EOF]");
	
	        while (true)
	        {
	            for (var i = 0; _stack.GetState(i, out var state); i++)
	            {
	                if (_actions[state, token.State] != -1)
	                {
	                    _stack.Pop(i);
	                    return true;
	                }
	            }
	            if (token.State == 0)
	                break;
	            token = NextToken();
	        }
	        return isOk;
	    }
	
		#region LRStack
		[DebuggerDisplay("Count = {_count}"), DebuggerTypeProxy(typeof(LRStackProxy))]
		private class LRStack
		{
			private Rec[] _array = new Rec[4];
			private int _items;
			private int _count = 1;
		
			public LRStack SetItems(int value)
			{
				_items = value;
				return this;
			}
		
			public object this[int offset]
			{
				get
				{
					if (_count < offset || offset < 0)
						throw new InvalidOperationException(string.Format("Unable to retrieve {0} items", offset));
					return _array[_count - _items + offset].Value;
				}
			}
		
			public uint Reduce(uint state, object value)
			{
				var oldState = _array[_count - _items - 1].State;
				for (var i = _items - 1; i > 0; i--)
					_array[_count - i].Value = null;
				_array[_count - _items] = new Rec(state, value);
				_count = _count - _items + 1;
				return oldState;
			}
	
	        public uint Push(uint state, object value)
	        {
	            var oldState = _array[_count - 1].State;
	            Shift(state, value);
	            return oldState;
	        }
	
	        public void Replace(uint state)
			{
				_array[_count - 1].State = state;
			}
		
			public void Shift(uint state, object value)
			{
	            if (_count == _array.Length)
	            {
	                var array = new Rec[_array.Length == 0 ? 4 : 2 * _array.Length];
	                Array.Copy(_array, 0, array, 0, _count);
	                _array = array;
	            }
	            _array[_count++] = new Rec(state, value);
			}
		
			public object PopValue()
			{
				return _array[--_count].Value;
			}
	
			public bool GetState(int index, out uint state)
	        {
	            index++;
	            var isOk = _count > index;
	            state = isOk ? _array[_count - index].State : 0;
	            return isOk;
	        }
	
	        public void Pop(int items)
	        {
	            for (var i = items - 1; i > 0; i--)
	                _array[_count - i].Value = null;
	            _count -= items;
	        }
		
			public uint PeekState()
			{
				return _array[_count - 1].State;
			}
		
	        [DebuggerDisplay("{State,2}: {Value}")]
			struct Rec
			{
				public Rec(uint state, object value)
				{
					State = state;
					Value = value;
				}
				public uint State;
				public object Value;
			}
	
	        sealed class LRStackProxy
	        {
	            private readonly LRStack _stack;
	            public LRStackProxy(LRStack stack)
	            {
	                _stack = stack;
	            }
	
	            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	            public Rec[] Items
	            {
	                get
	                {
	                    var result = new Rec[_stack._count];
	                    for (var i = 0; i < _stack._count; i++)
	                        result[i] = _stack._array[_stack._count - i - 1];
	                    return result;
	                }
	            }
	        }
		}
		#endregion
	}
	
	public class Scanner: IDisposable
	{
	    public const int Eof = -1;
	
	    private readonly IBuffer buffer;
	    private int ch;
	    private int line;
		private int column;
	
		private int lastAcceptingState;
	    private int lastAcceptingPosition;
	    private int lastLine;
	    private int lastColumn;
	
	    public Scanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column) 
		{}
	
	    public Scanner(IBuffer buffer, int line = 1, int column = 0)
	    {
	        this.buffer = buffer;
	        this.line = line;
	        this.column = column;
	        NextChar();
	    }
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0)
		{
			var text = File.ReadAllText(filePath);
			return new Scanner(text, line, column);
		}
	
		public void Dispose() => buffer.Dispose();
	
	    public string FilePath { get; private set; }
	
	    /// <summary>Skipping ignore, returns next token</summary>
	    public Token NextToken()
	    {
	        Token token;
	        do { token = RawNextToken(); } 
	        while (token.State < TokenStates.SyntaxError);
	        return token;
	    }
	       
		Token RawNextToken()
		{
	        Token token;
			if (ch == Eof)
			{
	            token = new Token(line, column, buffer.Position);
	            MarkAccepting(TokenStates.Empty);
				goto EndState;
			}
	        token = new Token(line, column, buffer.Position - 1);
			MarkAccepting(TokenStates.SyntaxError);
	
			if (_ch=='0') goto State1;
			if (_ch=='+') goto State5;
			if (_ch=='-') goto State6;
			if (_ch=='*') goto State7;
			if (_ch=='/') goto State8;
			if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State4;
			if ((_ch>='1' && _ch<='9')) goto State2;
			if ((_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State3;
			goto EndState2;
		State1:
			MarkAccepting(TokenStates.Int);
			NextChar();
			goto EndState;
		State2:
			MarkAccepting(TokenStates.Int);
			NextChar();
			if ((_ch>='0' && _ch<='9')) goto State2;
			goto EndState;
		State3:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State3;
			goto EndState;
		State4:
			MarkAccepting(TokenStates.white_space);
			NextChar();
			if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State4;
			goto EndState;
		State5:
			MarkAccepting(TokenStates.Plus);
			NextChar();
			goto EndState;
		State6:
			MarkAccepting(TokenStates.Minus);
			NextChar();
			goto EndState;
		State7:
			MarkAccepting(TokenStates.Asterisk);
			NextChar();
			goto EndState;
		State8:
			MarkAccepting(TokenStates.Slash);
			NextChar();
			goto EndState;
		EndState:
			if (lastAcceptingState == TokenStates.SyntaxError)
			{
				lastAcceptingPosition = buffer.Position - 1;
				lastAcceptingState = -1;
			}
			var value = buffer.GetString(token.Beg, lastAcceptingPosition);
			token.Set(lastAcceptingState, value, lastLine, lastColumn, lastAcceptingPosition - 1);
			if (buffer.Position != lastAcceptingPosition)
			{
				buffer.Position = lastAcceptingPosition;
				line = lastLine;
				column = lastColumn;
				NextChar();
			}
			return token;
	
	    EndState2:
			value = buffer.GetString(token.Beg, lastAcceptingPosition);
			token.Set(lastAcceptingState, value, lastLine, lastColumn, lastAcceptingPosition - 1);
			NextChar();
			return token;
		}
	
	    void MarkAccepting(int type)
	    {
	        lastAcceptingState = type;
	        lastAcceptingPosition = buffer.Position;
	        lastLine = line;
	        lastColumn = column;
	    }
	
	    /// <summary>
	    /// Retrieves the next character, adjusting position information
	    /// </summary>
	    private void NextChar()
	    {
	        ch = buffer.Read();
	        if (ch == '\n')
	        {
	            ++line;
	            column = 0;
	        }
	        //Normalize \r\n -> \n
	        else if (ch == '\r' && buffer.Peek() == '\n')
	        {
	            ch = buffer.Read();
	            ++line;
	            column = 0;
	        }
	        else
	        { 
	            ++column;
	        }
	    }
	}
	
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
		public const int Int = 1;
		public const int identifier = 2;
		public const int white_space = 3;
		public const int Plus = 4;
		public const int Minus = 5;
		public const int Asterisk = 6;
		public const int Slash = 7;
	}
	
	
	public class Token: Segment
	{
		public int State;
		public string Value;
	
		public Token() {}
			
		public Token(int line, int column, int ch)
			: base(new Position(line, column, ch)) {}
	
		public Token Set(int state, string value, int ln, int col, int ch)
		{
			State = state;
			Value = value;
			End = new Position(ln, col, ch);
			return this;
		}
	
	    public static implicit operator string(Token t) =>  t.Value;
	
		public override string ToString() =>
			$"({Start.Ln},{Start.Col}): '{Value}', state = {State}";
	}
	
	///<summary>Location of a character within a file</summary>
	public struct Position: IEquatable<Position>
	{
		public readonly int Ln;
		public readonly int Col;
		public readonly int Ch;
	
		public Position(int ln, int col, int ch)
		{
			Ln = ln;
			Col = col;
			Ch = ch;
		}
	
		public override int GetHashCode() => Ch.GetHashCode();
		public override bool Equals(object obj) => Equals((Position)obj);
		public bool Equals(Position other) => (Ch == other.Ch);
	
		public override string ToString() =>
			string.Format("Ln {0}, Col {1}, Ch {2}", Ln, Col, Ch);
	
		public static bool operator ==(Position left, Position right) =>
			left.Ch == right.Ch;
	
		public static bool operator !=(Position left, Position right) =>
			left.Ch != right.Ch;
	}
	
	///<summary>Segment location in a file</summary>
	public class Segment
	{
		public Position Start;
		public Position End;
		
		public Segment() {}
		
		public Segment(Segment cpy)
		{
			if (cpy != null)
			{
				Start = cpy.Start;
				End = cpy.End;
			}
		}
		
		public Segment(Position start)
		{
			Start = start;
			End = start;
		}
		
		public Segment(Position start, Position end)
		{
			Start = start;
			End = end;
		}
	
		public bool IsEmpty => (End == Start);
		
		public int Beg => Start.Ch;
		
		public int Length => End.Ch - Start.Ch + 1;
				
		public void CopyFrom(Segment segment)
		{
			if (segment != null)
			{
				Start = segment.Start;
				End = segment.End;
			}
		}
	}
	
	
	/// <summary>
	/// Buffer between text/file object and scanner
	/// </summary>
	public interface IBuffer: IDisposable
	{
	    /// <summary>Text/file length</summary>
		long Length { get; }
	
		/// <summary>Returns the index within the buffer</summary>
	    int Position { get; set; }
	
	    /// <summary>Returns the next character, moves the position one forward</summary>
	    int Read();
	
	    /// <summary>Examines the next character in the stream, leaves position at the same place</summary>
	    int Peek();
	
	    /// <summary>
	    /// Returns string from beg to end
	    /// </summary>
	    string GetString(int beg, int end);
		
		string PeekLine();
	}
	
	public class FileBuffer : IBuffer, IDisposable
	{
		private readonly StreamReader reader;
		private readonly StringBuilder builder;
		private int filePos;
		private int remaining;
	
	    public FileBuffer(string filePath)
		    : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
		}
		
		public FileBuffer(Stream stream)
		{
		    Length = stream.Length;
		    reader = new StreamReader(stream);
		    builder = new StringBuilder();
		}
	
		public void Dispose() => reader.Dispose();
		
	    public long Length { get; }
	
	    public int Position
		{
		    get => filePos - remaining;
		    set => remaining = filePos - value;
		}
		
		public string GetString(int beg, int end)
		{
		    var length = end - beg;
			var start = filePos - builder.Length - beg;
		    var result = builder.ToString(start, length);
		    builder.Remove(start, length);
		    return result;
		}
		
		public int Peek()
		{
		    int result;
		    if (remaining > 0)
		    {
		        result = builder[builder.Length - remaining];
		    }
		    else if (filePos < Length)
		    {
		        result = reader.Read();
				filePos++;
				builder.Append((char)result);
		        remaining++;
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
			if (remaining > 0)
			{
				result = builder[builder.Length - remaining--];
			}
			else if (filePos < Length)
			{
				result = reader.Read();
				builder.Append((char)result);
				filePos++;
			}
			else
			{
				result = -1;
			}
			return result;
		}
	
	
		public string PeekLine()
	    {
			var result = new StringBuilder();
			for (var i = builder.Length - remaining; i < builder.Length; i++)
	        {
				var ch = builder[i];
				if (ch == '\n') return result.ToString();
				if (ch != '\r') result.Append(ch);
	        }
	
			while (filePos < Length)
	        {
				var ch = reader.Read();
				filePos++;
				builder.Append((char)ch);
				remaining++;
				if (ch == '\n') return result.ToString();
				if (ch != '\r') result.Append((char)ch);
			}
			return result.ToString();
		}
	}
	
	
	public class StringBuffer: IBuffer
	{
	    private readonly string text;
	
	    public StringBuffer(string text)
	    {
	        this.text = text;
	    }
	
		public void Dispose() {}
	
		public long Length => text.Length;
	
	    public int Position { get; set;}
	
	    public int Read()
	    {
	        return (Position < text.Length) ? text[Position++] : -1;
	    }
	
	    public int Peek()
	    {
	        return (Position < text.Length) ? text[Position] : -1;
	    }
	
	    public string GetString(int start, int end)
	    {
	        return text.Substring(start, end - start);
	    }
	}
	
	#region Logger
	
	public enum Importance
	{
		High = 0,
		Normal = 1,
		Low = 2
	}
	
	public interface ILogger
	{
		void LogError(string message, params object[] messageArgs);
		void LogError(Segment segment, string message, params object[] messageArgs);
		void LogWarning(string message, params object[] messageArgs);
		void LogWarning(Segment segment, string message, params object[] messageArgs);
		void LogMessage(Importance importance, string message, params object[] messageArgs);
		void LogMessage(Importance importance, Segment segment, string message, params object[] messageArgs);
	}
	
	public class ConsoleLogger : ILogger
	{
	    private readonly string _file;
	    private readonly ConsoleColor _oldColor;
	
	    public ConsoleLogger(string file)
	    {
	        _file = file;
	        _oldColor = Console.ForegroundColor;
	    }
	
	    public void LogError(string message, params object[] messageArgs)
	    {	Log(ConsoleColor.Red, message, messageArgs); }
	
	    public void LogError(Segment segment, string message, params object[] messageArgs)
	    {	Log(ConsoleColor.Red, segment, message, messageArgs); }
	
	    public void LogMessage(string message, params object[] messageArgs)
	    {	LogMessage(Importance.Low, message, messageArgs); }
	
	    public void LogMessage(Importance importance, string message, params object[] messageArgs)
	    {	
	        Log(ToColor(importance), message, messageArgs);
	    }
	
		public void LogMessage(Importance importance, Segment segment, string message, params object[] messageArgs)
		{
			Log(ToColor(importance), segment, message, messageArgs);
		}
	
	    public void LogWarning(Segment segment, string message, params object[] messageArgs)
	    {	Log(ConsoleColor.Yellow, segment, message, messageArgs); }
	
	    public void LogWarning(string message, params object[] messageArgs)
	    {	Log(ConsoleColor.Yellow, message, messageArgs); }
	
	    public void Log(ConsoleColor color, Segment segment, string message, params object[] messageArgs)
	    {
	        var builder = new StringBuilder();
	        if (!string.IsNullOrEmpty(_file))
	            builder.Append(_file);
	        if (segment.Start.Ln > 0)
	        {
	            builder.Append('(').Append(segment.Start.Ln);
	            if (segment.Start.Col > 0)
	            {
	                builder.Append(',').Append(segment.Start.Col);
	                if (segment.End.Ln > 0)
	                    builder.Append(',').Append(segment.End.Ln).Append(',').Append(segment.End.Col);
	            }
	            builder.Append(')');
	        }
	        if (builder.Length > 0)
	            builder.Append(":\t");
	        if (messageArgs == null || messageArgs.Length == 0)
	            builder.Append(message);
	        else
	            builder.AppendFormat(message, messageArgs);
	        Log(color, builder.ToString());
	    }
	
	    public void Log(ConsoleColor color, string message, params object[] messageArgs)
	    {
	        if (messageArgs == null || messageArgs.Length == 0)
	            Log(color, message);
	        else
	            Log(color, string.Format(message, messageArgs));
	    }
	
	    public void Log(ConsoleColor color, string message)
	    {
	        Console.ForegroundColor = color;
	        Console.WriteLine(message);
	        Console.ForegroundColor = _oldColor;
	    }
	
		private static ConsoleColor ToColor(Importance importance)
		{
	        ConsoleColor color;
	        switch (importance)
	        {
	            case Importance.High:	color = ConsoleColor.White; break;
	            case Importance.Normal:	color = ConsoleColor.Gray; break;
	            default:				color = ConsoleColor.DarkGray; break;
	        }
			return color;
		}
	}
	#endregion
}
