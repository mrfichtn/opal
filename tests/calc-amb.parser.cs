using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;


namespace CalcAmbTest
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
				case 1: // expr = primary;
				{
					state = _stack.SetItems(1)
					    .Reduce(8, _stack[0]);
					break;
				}
				case 2: // expr = expr op primary;
				{
					state = _stack.SetItems(3)
					    .Reduce(8, _stack[0]);
					break;
				}
				case 3: // op = "*";
				{
					state = _stack.SetItems(1)
					    .Reduce(10, _stack[0]);
					break;
				}
				case 4: // op = "/";
				{
					state = _stack.SetItems(1)
					    .Reduce(10, _stack[0]);
					break;
				}
				case 5: // op = "+";
				{
					state = _stack.SetItems(1)
					    .Reduce(10, _stack[0]);
					break;
				}
				case 6: // op = "-";
				{
					state = _stack.SetItems(1)
					    .Reduce(10, _stack[0]);
					break;
				}
				case 7: // primary = Int;
				{
					state = _stack.SetItems(1)
					    .Reduce(9, _stack[0]);
					break;
				}
	
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
			{ -1, 3, -1, -1, -1, -1, -1, -1, 1, 2, -1 },
			{ -2, -1, -1, -1, 5, 6, 7, 8, -1, -1, 4 },
			{ -3, -1, -1, -1, -3, -3, -3, -3, -1, -1, -1 },
			{ -9, -1, -1, -1, -9, -9, -9, -9, -1, -1, -1 },
			{ -1, 3, -1, -1, -1, -1, -1, -1, -1, 9, -1 },
			{ -1, -5, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -6, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -7, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -8, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -4, -1, -1, -1, -4, -4, -4, -4, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		protected const int _maxTerminal = 7;
		protected readonly string[] _symbols =
		{
			"",
			"Int",
			"identifier",
			"white_space",
			"*",
			"/",
			"+",
			"-",
			"expr",
			"primary",
			"op",
		};
	
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
	
	    private readonly IBuffer _buffer;
	    private int _ch;
		private int _line;
		private int _column;
	
	    private static readonly int[] _charToClass;
	    private static readonly int[,] _states;
	
		static Scanner()
		{
	        _charToClass = new int[char.MaxValue + 1];
	        using (var outStream = new MemoryStream())
			{
	            Decompress(_charToClassCompressed, outStream);
	            outStream.Seek(0, SeekOrigin.Begin);
	            for (var i = 0; i < _charToClass.Length; i++)
	                _charToClass[i] = Read8(outStream) + 1;
	                
	            outStream.SetLength(0);
				Decompress(_compressedStates, outStream);
	            outStream.Seek(0, SeekOrigin.Begin);
	
				var maxClasses = 10 + 1;
				var maxStates = 9;
	            _states = new int[maxStates + 1, maxClasses];
	
	            for (var i = 0; i < maxStates; i++)
	                for (var j = 0; j < maxClasses; j++)
	                    _states[i, j] = Read8(outStream);
	
	            for (int i = 1; i < maxClasses; i++)
	            {
	                if (_states[0, i] == 0)
	                {
	                    _states[0, i] = maxStates;
	                    _states[maxStates, i] = maxStates;
	                }
	            }
				_states[maxStates, 0] = -1;
			}
		}
	
	
	    public Scanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column)
	    {
	    }
	
		public Scanner(IBuffer buffer, int line = 1, int column = 0)
		{
			_buffer = buffer;
			_line = line;
			_column = column;
	        NextChar();
		}
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0)
		{
			return new Scanner(new FileBuffer(filePath), line, column);
		}
	
		public void Dispose()
		{
			_buffer.Dispose();
		}
	
		public string FilePath { get; private set; }
	
	    /// <summary>Skips ingore-tokens</summary>
	    public Token NextToken()
	    {
	        Token token;
	        do
	        {
	            token = RawNextToken();
	        } while (token.State < TokenStates.SyntaxError);
	
	        return token;
	    }
	        
	    /// <summary>Returns next token</summary>
	    public Token RawNextToken()
	    {
	        Token token;
	        if (_ch == Eof)
	        {
	            token = new Token(_line, _column, _buffer.Position)
	            {
	                End = new Position(_line, _column, _buffer.Position)
	            };
	            return token;
	        }
	            
			var startPosition = _buffer.Position - 1;
	        token = new Token(_line, _column, startPosition);
	
	        var lastLine = _line;
	        var lastColumn = _column;
	        var lastAcceptingState = TokenStates.SyntaxError;
	        var lastAcceptingPosition = -1;
	        var stateIndex = 0;
	
	        while (true)
	        {
	            var nextState = GetNextState(stateIndex);
	            if (nextState <= 0)
	            {
	                if (lastAcceptingState == TokenStates.SyntaxError)
	                    lastAcceptingPosition = _buffer.Position - 1;
	
	                var value = _buffer.GetString(startPosition, lastAcceptingPosition);
	                token.Set(lastAcceptingState, value, lastLine, lastColumn, lastAcceptingPosition - 1);
					if (_buffer.Position > lastAcceptingPosition + 1)
					{
						_buffer.Position = lastAcceptingPosition;
						_line = lastLine;
						_column = lastColumn;
						NextChar();
					}
	                return token;
	            }
	            else
	            {
	                var acceptingState = _states[nextState, 0];
	                if (acceptingState != 0)
	                {
	                    lastAcceptingState = acceptingState;
	                    lastAcceptingPosition = _buffer.Position;
	                    lastLine = _line;
	                    lastColumn = _column;
	                }
	            }
	            stateIndex = nextState;
	            NextChar();
	        }
	    }
	
	    public int GetNextState(int stateIndex)
	    {
	        int nextState;
	        if (_ch >= 0)
	        {
	            var cls = _charToClass[_ch];
	            nextState = _states[stateIndex, cls];
	        }
	        else
	        {
	            nextState = 0;
	        }
	        return nextState;
	    }
	
	    /// <summary>
	    /// Retrieves the next character, adjusting position information
	    /// </summary>
	    public void NextChar()
	    {
	        _ch = _buffer.Read();
	        if (_ch == '\n')
	        {
	            ++_line;
	            _column = 0;
	        }
	        //Normalize \r\n -> \n
	        else if (_ch == '\r' && _buffer.Peek() == '\n')
	        {
	            _ch = _buffer.Read();
	            ++_line;
	            _column = 0;
	        }
	        else
	        { 
	            ++_column;
	        }
	    }
	
	private static readonly byte[] _charToClassCompressed = 
	{
		0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xED, 0xC9, 0x31, 0x0E, 0x80, 0x20, 
		0x00, 0x04, 0xC1, 0x43, 0x50, 0xF8, 0xFF, 0x8B, 0x4D, 0x0C, 0xC4, 0x46, 0x63, 0x6B, 0x31, 0x53, 
		0xEE, 0x26, 0x53, 0x6B, 0x79, 0x72, 0xD7, 0xBD, 0x67, 0xE4, 0x28, 0xDB, 0x32, 0x73, 0x7D, 0x75, 
		0xDD, 0x8F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0xC0, 0x4F, 0x9C, 0xC1, 0xFD, 0xF5, 0x34, 0x00, 0x00, 0x01, 0x00, 
	};
	
	private static readonly byte[] _compressedStates = 
	{
		0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x45, 0xC6, 0x41, 0x0E, 0x00, 0x20, 
		0x0C, 0x02, 0x41, 0x0A, 0xAD, 0xF5, 0xFF, 0x2F, 0x36, 0xA9, 0x46, 0xF6, 0x00, 0x03, 0x04, 0x95, 
		0xB5, 0x7A, 0x23, 0xF0, 0x0B, 0x90, 0x8F, 0x84, 0xA4, 0xCB, 0xB9, 0x84, 0x77, 0x2A, 0x73, 0x99, 
		0x6D, 0x1E, 0x36, 0x0F, 0x4D, 0x07, 0x63, 0x00, 0x00, 0x00, 
	};
	
	
	    private static void Decompress(byte[] data, Stream outStream)
	    {
	        using (var inStream = new MemoryStream(data))
	        {
	            var s = new GZipStream(inStream, CompressionMode.Decompress, false);
	                s.CopyTo(outStream);
	        }
	    }
	
		#region Read Methods
	
	    public static int Read8(Stream stream)
	    {
	        return (sbyte) stream.ReadByte();
	    }
	
	    public static int Read16(Stream stream)
	    {
	        var b1 = stream.ReadByte();
	        var b2 = stream.ReadByte();
			return (short)(b1 | (b2 << 8));
	    }
	
	    public static int Read24(Stream stream)
	    {
	        var b1 = stream.ReadByte();
	        var b2 = stream.ReadByte();
	        var b3 = stream.ReadByte();
			return b1 | (b2 << 8) | (b3 << 16);
	    }
	
	    public static int Read32(Stream stream)
	    {
	        var b1 = stream.ReadByte();
	        var b2 = stream.ReadByte();
	        var b3 = stream.ReadByte();
	        var b4 = stream.ReadByte();
			return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
	    }
	
		#endregion
	}
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
		public const int Int = 1;
		public const int identifier = 2;
		public const int white_space = 3;
		public const int Asterisk = 4;
		public const int Slash = 5;
		public const int Plus = 6;
		public const int Minus = 7;
	};
	
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
	
	    public static implicit operator string(Token t)
	    {
	        return t.Value;
	    }
	
		public override string ToString()
		{
			return string.Format("({0},{1}): '{2}', state = {3}", Start.Ln, Start.Col, Value, State);
		}
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
	
		#region Methods
	
		public override int GetHashCode()
		{
			return Ch.GetHashCode();
		}
	
		public override bool Equals(object obj)
		{
			return Equals((Position)obj);
		}
	
		public bool Equals(Position other)
		{
			return Ch == other.Ch;
		}
	
		public override string ToString()
		{
			return string.Format("Ln {0}, Col {1}, Ch {2}", Ln, Col, Ch);
		}
	
		public static bool operator ==(Position left, Position right)
		{
			return left.Ch == right.Ch;
		}
	
		public static bool operator !=(Position left, Position right)
		{
			return left.Ch != right.Ch;
		}
		#endregion
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
	
		#region Properties
			
		public bool IsEmpty
		{
			get { return End == Start; }
		}
	
		public int Beg
		{
			get { return Start.Ch; }
		}
	
		public int Length
		{
			get { return End.Ch - Start.Ch; }
		}
			
		#endregion
	
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
	    /// <summary>
	    /// Returns the index within the buffer
	    /// </summary>
	    int Position { get; set; }
	
	    /// <summary>
	    /// Returns the next character, moves the position one forward
	    /// </summary>
	    /// <returns></returns>
	    int Read();
	
	    /// <summary>
	    /// Examines the next character in the stream, leaves position at the same place
	    /// </summary>
	    /// <returns></returns>
	    int Peek();
	
	    /// <summary>
	    /// Returns string from beg to end
	    /// </summary>
	    /// <param name="beg"></param>
	    /// <param name="end"></param>
	    /// <returns></returns>
	    string GetString(int beg, int end);
	}
	
	public class FileBuffer : IBuffer, IDisposable
	{
	    private readonly StreamReader _reader;
	    private readonly StringBuilder _builder;
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
	        set { _remaining = _filePos - value; }
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
	            _builder.Append((char)result);
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
	
	public class StringBuffer: IBuffer
	{
	    private readonly string _text;
	
	    public StringBuffer(string text)
	    {
	        _text = text;
	    }
	
		public void Dispose() {}
	
	    public int Position { get; set;}
	
	    public int Read()
	    {
	        return (Position < _text.Length) ? _text[Position++] : -1;
	    }
	
	    public int Peek()
	    {
	        return (Position < _text.Length) ? _text[Position] : -1;
	    }
	
	    public string GetString(int start, int end)
	    {
	        return _text.Substring(start, end - start);
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
