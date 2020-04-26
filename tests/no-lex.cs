using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Diagnostics;


namespace NoLex
{
	public partial class Parser
	{
		private readonly Scanner _scanner;
		private readonly LRStack _stack = new LRStack();
		private object _root;
			
		public Parser(Scanner scanner)
		{
			_scanner = scanner;
			Messages = new Messages();
			Init();
		}
	
		#region Properties
		public Messages Messages { get; }
		#endregion
	
		partial void Init();
	
		public static Parser FromFile(string filePath)
		{
			var scanner = Scanner.FromFile(filePath);
			return new Parser(scanner);
		}
	
		public bool Parse()
		{
			var token = _scanner.NextToken();
			var hasErrors = false;
	
			while (true)
			{
	            if (token.State < 0)
	            {
	                Messages.Error("{0}: syntax error - {1}", token.Start, token.Value);
					hasErrors = true;
	                token = _scanner.NextToken();
	            }
					
				var state = _stack.PeekState();
				var actionType = GetAction(state, (uint) token.State, out var result);
	
				switch (actionType)
				{
					case ActionType.Error:
						Messages.Error("{0}: unexpected token - {1}", token.Start, token.Value);
						hasErrors = true;
		                token = _scanner.NextToken();
	                    if (token.State == 0)
	                        return false;
						break;
	
					case ActionType.Reduce:
						var rule = result;
						var reducedState = Reduce(rule);
						if (rule == 0)
						{
							_root = _stack.PopValue();
							return !hasErrors;
						}
						GetAction(reducedState, _stack.PeekState(), out result);
						_stack.Replace((uint)result);
						break;
	
					case ActionType.Shift:
						_stack.Push(result, token);
						token = _scanner.NextToken();
						break;
				}
			}
		}
	
		private uint Reduce(uint rule)
		{
			uint state = 0;
	
			switch (rule)
			{
				case 0: // language = expr;
				{
					state = _stack.SetItems(1)
					    .Reduce(5, _stack[0]);
					break;
				}
				case 1: // expr = expr "+" term;
				{
					state = _stack.SetItems(3)
					    .Reduce(6, new AddExr((Token)_stack[1],_stack[3]));
					break;
				}
				case 2: // expr = term;
				{
					state = _stack.SetItems(1)
					    .Reduce(6, _stack[0]);
					break;
				}
				case 3: // term = term "*" primary;
				{
					state = _stack.SetItems(3)
					    .Reduce(7, new MultiExpr((Expr) _stack[1],(Expr) _stack[3]));
					break;
				}
				case 4: // term = term "/" primary;
				{
					state = _stack.SetItems(3)
					    .Reduce(7, new MultiExpr((Expr) _stack[1],(Expr) _stack[3]));
					break;
				}
				case 5: // term = primary;
				{
					state = _stack.SetItems(1)
					    .Reduce(7, _stack[0]);
					break;
				}
				case 6: // primary = id;
				{
					state = _stack.SetItems(1)
					    .Reduce(8, new Constant((Token) _stack[1]));
					break;
				}
	
			}
			return state;
		}
	
		private enum ActionType { Shift, Reduce, Error }
	
		// If == -1, invalid entry; If >= 0, represents new state after shift; else . If < 0, reduction rule.
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
	
		private readonly int[,] _actions = 
		{
			{ -1, -1, -1, -1, 4, -1, 1, 2, 3 },
			{ -2, 5, -1, -1, -1, -1, -1, -1, -1 },
			{ -4, -4, 6, 7, -1, -1, -1, -1, -1 },
			{ -7, -7, -7, -7, -1, -1, -1, -1, -1 },
			{ -8, -8, -8, -8, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, 4, -1, -1, 8, 3 },
			{ -1, -1, -1, -1, 4, -1, -1, -1, 9 },
			{ -1, -1, -1, -1, 4, -1, -1, -1, 10 },
			{ -3, -3, 6, 7, -1, -1, -1, -1, -1 },
			{ -5, -5, -5, -5, -1, -1, -1, -1, -1 },
			{ -6, -6, -6, -6, -1, -1, -1, -1, -1 },
		};
	
	
		#region Symbols
		private string[] _symbols = 
		{
		private const int _maxTerminal = 4;
		private string[] _symbols =
		{
			"",
			"+",
			"*",
			"/",
			"id",
			"language",
			"expr",
			"term",
			"primary",
		};
	
		};
		#endregion
		
		#region LRStack
		[DebuggerDisplay("Count = {Count}")]
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
		
			public uint PeekState()
			{
				return _array[_count - 1].State;
			}
		
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
		}
		#endregion
	}
	
	public class Scanner
	{
	    private readonly IBuffer _buffer;
	    private int _ch;
	    public const int Eof = -1;
	
	    private static readonly int[] _charToClass;
	    private static readonly int[,] _states;
	
		static Scanner()
		{
	        _charToClass = new int[char.MaxValue + 1];
	        using (var outStream = new MemoryStream())
			{
	            using (var inStream = new MemoryStream(_charToClassCompressed))
	            using (var s = new GZipStream(inStream, CompressionMode.Decompress, false))
	                s.CopyTo(outStream);
	
	            outStream.Seek(0, SeekOrigin.Begin);
	            for (var i = 0; i < _charToClass.Length; i++)
	                _charToClass[i] = Read8(outStream) + 1;
	                
	            outStream.SetLength(0);
	            using (var inStream = new MemoryStream(_compressedStates))
	            using (var s = new GZipStream(inStream, CompressionMode.Decompress, false))
	                s.CopyTo(outStream);
	            outStream.Seek(0, SeekOrigin.Begin);
	
				var maxClasses = 5 + 1;
				var maxStates = 4;
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
			Line = line;
			Column = column;
	        NextChar();
		}
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0)
		{
			var text = File.ReadAllText(filePath);
			return new Scanner(text, line, column);
		}
	
		#region Properties
	
		public int Line { get; private set; }
		public int Column { get; private set; }
	
		#endregion
	
	    /// <summary>Skipping ignore, returns next token</summary>
	    public Token NextToken()
	    {
	        Token token;
	        do
	        {
	            token = RawNextToken();
	        } while (token.State < -1);
	
	        return token;
	    }
	        
	    /// <summary>Returns next token</summary>
	    public Token RawNextToken()
	    {
	        Token token;
	        if (_ch == Eof)
	        {
	            token = new Token(Line, Column, _buffer.Position)
	            {
	                End = new Position(Line, Column, _buffer.Position)
	            };
	            return token;
	        }
	            
			var startPosition = _buffer.Position - 1;
	        token = new Token(Line, Column, startPosition);
	
	        var lastLine = Line;
	        var lastColumn = Column;
	        var lastAcceptingState = int.MinValue;
	        var lastAcceptingPosition = -1;
	        var stateIndex = 0;
	
	        while (true)
	        {
	            var nextState = GetNextState(stateIndex);
	            if (nextState <= 0)
	            {
	                if (lastAcceptingState == int.MinValue)
	                {
	                    lastAcceptingPosition = _buffer.Position - 1;
	                    lastAcceptingState = -1;
	                }
	
	                var value = _buffer.GetString(startPosition, lastAcceptingPosition);
	                token.Set(lastAcceptingState, value, lastLine, lastColumn, lastAcceptingPosition - 1);
					if (_buffer.Position > lastAcceptingPosition + 1)
					{
						_buffer.Position = lastAcceptingPosition;
						Line = lastLine;
						Column = lastColumn;
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
	                    lastLine = Line;
	                    lastColumn = Column;
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
	            ++Line;
	            Column = 0;
	        }
	        //Normalize \r\n -> \n
	        else if (_ch == '\r' && _buffer.Peek() == '\n')
	        {
	            _ch = _buffer.Read();
	            ++Line;
	            Column = 0;
	        }
	        else
	        { 
	            ++Column;
	        }
	    }
	
		public class TokenTypes
		{
			public const int Empty = 0;
			public const int Plus = 1;
			public const int Asterisk = 2;
			public const int Slash = 3;
		};

		private static readonly byte[] _charToClassCompressed = 
		{
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xED, 0xCA, 0xB7, 0x0D, 0x00, 0x00, 
			0x08, 0xC0, 0x30, 0xCA, 0xFF, 0x3F, 0xC3, 0x09, 0xAC, 0x48, 0xF6, 0x90, 0x29, 0x11, 0x57, 0x95, 
			0x9B, 0x3E, 0xEF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0xBC, 0x32, 0xE5, 0x02, 0x18, 0x3B, 0x00, 0x00, 0x01, 0x00, 
		};

		private static readonly byte[] _compressedStates = 
		{
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x63, 0x60, 0x60, 0x64, 0x62, 0x66, 
			0x60, 0x64, 0x00, 0x01, 0x26, 0x30, 0xC9, 0x0C, 0x26, 0x01, 0x87, 0xB8, 0x79, 0x7A, 0x18, 0x00, 
			0x00, 0x00, 
		};
	
	
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
	
		public Segment()
		{
		}
	
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
	
	public class Token: Segment
	{
		public int State;
		public string Value;
	
		public Token()
		{
		}
			
		public Token(int line, int column, int ch)
			: base(new Position(line, column, ch))
		{
		}
	
		public Token Set(int state, string value, int ln, int col, int ch)
		{
			State = state;
			Value = value;
			End = new Position(ln, col, ch);
			return this;
		}
	
		public override string ToString()
		{
			return string.Format("({0},{1}): {2}", Start.Ln, Start.Col, Value);
		}
	}
	
	/// <summary>
	/// Buffer between text/file object and scanner
	/// </summary>
	public interface IBuffer
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
	
	public class StringBuffer: IBuffer
	{
	    private readonly string _text;
	
	    public StringBuffer(string text)
	    {
	        _text = text;
	    }
	
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
	
	public class Message
	{
	    public Message(bool isError, string msg)
	    {
	        IsError = isError;
	        Msg = msg;
	    }
	    public bool IsError;
	    public string Msg;
	}
	
	public class Messages: List<Message>
	{
	    public int ErrorCount { get; private set; }
			
		public void Log(bool isError, string msg)
	    {
	        if (isError)
				ErrorCount++;
			Add(new Message(isError, msg));
	    }
	
	    public void Error(string msg)
	    {
	        Log(true, msg);
	    }
	
	    public void Error(string fmt, params object[] args)
	    {
	        Error(string.Format(fmt, args));
	    }
	
	    public void Info(string msg)
	    {
	        Log(false, msg);
	    }
	
	    public void Info(string fmt, params object[] args)
	    {
	        Info(string.Format(fmt, args));
	    }
	}
}
