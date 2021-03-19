using Opal;



namespace Opal
{
	
	public partial class Parser: ParserBase
	{
		private readonly string? srcFile;
		
		public Parser(Scanner scanner)
			: base(scanner, _maxTerminal, _symbols, _actions)
		{}
	
		public Parser(string srcFile)
			: this(Scanner.FromFile(srcFile)) 
		{
			this.srcFile = srcFile;
		}
	
		public static Parser FromString(string text)
		{
			var scanner = new Scanner(text);
			return new Parser(scanner);
		}
	
		protected override uint Reduce(uint rule)
		{
			uint state = 0;
	
			switch (rule)
			{
				case 1: // expr = "start" int_decl_list
					items = 2;
					state = Reduce(3, new Grammar(At<IntegerList>(1)));
					break;
				case 2: // int_decl = Int
					items = 1;
					state = Reduce(4, new Integer(At<Token>(0)));
					break;
				case 3: // int_decl_list =
					state = Push(5, new IntegerList());
					break;
				case 4: // int_decl_list = int_decl_list int_decl
					items = 2;
					state = Reduce(5, IntegerList(At<IntegerList>(0),At<IntegerList>(0)));
					break;
	
			}
			return state;
		}
	
		#region Actions Table
		private static readonly int[,] _actions = 
		{
			{ -1, -1, 2, 1, -1, -1 },
			{ -2, -1, -1, -1, -1, -1 },
			{ -5, -5, -1, -1, -1, 3 },
			{ -3, 5, -1, -1, 4, -1 },
			{ -6, -6, -1, -1, -1, -1 },
			{ -4, -4, -1, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		protected const int _maxTerminal = 2;
		protected static readonly string[] _symbols =
		{
			"𝜖",
			"Int",
			"start",
			"expr",
			"int_decl",
			"int_decl_list"
		};
		#endregion
	
	}
	
	public class Scanner: StateScannerBase
	{
	    private static readonly int[] _charToClass;
	    private static readonly int[,] _states;
	
		static Scanner()
		{
	        _charToClass = Opal.CharClasses.Decompress8(_charToClassCompressed);
			_states = Opal.ScannerStates.Decompress8(_compressedStates,
	  maxClasses: 9,
	  maxStates: 10);
	
		}
	
	    public Scanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column)
	    {
	    }
	
		public Scanner(IBuffer buffer, int line = 1, int column = 0)
			: base(_charToClass, _states, buffer, line, column)
		{
		}
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0) =>
			new Scanner(StringBuffer.FromFile(filePath), line, column);
	
		private static readonly byte[] _charToClassCompressed = 
		{
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xED, 0xCB, 0x21, 0x12, 0x80, 0x30, 
			0x10, 0x03, 0xC0, 0xB6, 0x57, 0xDA, 0xFF, 0xFF, 0x18, 0xC3, 0x09, 0x0E, 0x04, 0x0A, 0xB5, 0xAB, 
			0x32, 0x99, 0xA4, 0xB5, 0x4B, 0x44, 0xA6, 0x9B, 0xDA, 0xF6, 0x91, 0x5E, 0xE7, 0x4F, 0xAB, 0x16, 
			0xC7, 0x9E, 0x1F, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0xFC, 0xE7, 0x04, 0x31, 0x2A, 0x18, 0x73, 0x00, 0x00, 0x01, 0x00, 
		};
	
		private static readonly byte[] _compressedStates = 
		{
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x45, 0x8C, 0x0B, 0x0A, 0x00, 0x20, 
			0x0C, 0x42, 0xDD, 0xFA, 0x7A, 0xFF, 0xFB, 0x56, 0x46, 0xAD, 0x04, 0xE5, 0x21, 0x28, 0x68, 0x9E, 
			0x48, 0x66, 0x1A, 0x42, 0x06, 0xF7, 0x43, 0x43, 0x4E, 0xAF, 0x47, 0xF9, 0x88, 0x1A, 0xD0, 0x02, 
			0xFA, 0x8E, 0x3B, 0x94, 0x26, 0x15, 0x7A, 0x06, 0x17, 0x30, 0xB9, 0x69, 0x7F, 0x64, 0x00, 0x00, 
			0x00, 
		};
	
	}
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
		public const int Int = 1;
		public const int @start = 2;
		public const int white_space = -2;
	}
	
		public class Token : Segment
	{
		public int State;
		public string Value;
	
		public Token(int line, int column, int ch)
			: base(new Position(line, column, ch)) 
		{ 
			Value = string.Empty;
		}
	
		public Token(Position start, Position end, int state, string value)
			: base(start, end)
	    {
			State = state;
			Value = value;
	    }
	
		public static implicit operator string?(Token t) => t.Value;
	
		public override string ToString() =>
			string.Format("({0},{1}): '{2}', state = {3}", Start.Ln, Start.Col, Value, State);
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
		public override bool Equals(object? obj) => (obj != null) && Equals((Position)obj);
		public bool Equals(Position other) => Ch == other.Ch;
	
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
		        result = builder[^remaining];
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
				result = builder[^(remaining--)];
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
	
	    public StringBuffer(string text) => this.text = text;
	
		public void Dispose() => GC.SuppressFinalize(this);
	
		public long Length => text.Length;
	
	    public int Position { get; set;}
	
	    public int Read() => (Position < text.Length) ? text[Position++] : -1;
	
	    public int Peek() => (Position < text.Length) ? text[Position] : -1;
	
	    public string GetString(int start, int end) => text[start..end];
	
		public string PeekLine()
		{
			int i;
			for (i = Position; i < text.Length; i++)
			{
				var ch = text[i];
				if (ch == '\r' || ch == '\n')
					break;
			}
			return text.Substring(Position, i - Position + 1);
		}
	}
	
	#region Logger
	public class LogItem
	{
	    public LogItem(LogLevel level, 
	        string message,
	        Token token,
	        string line,
	        string? suggestions = null)
	    {
	        Level = level;
	        Message = message;
	        Token = token;
	        Line = line;
	        Suggestions = suggestions;
	    }
	
	    public readonly LogLevel Level;
	    public readonly string Message;
	    public readonly Token Token;
	    public readonly string Line;
	    public readonly string? Suggestions;
	}
	
	public enum LogLevel
	{
	    Error,
	    Warning,
	    Info
	}
	
	#endregion
}
