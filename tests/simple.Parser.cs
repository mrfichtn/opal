using Opal;
using System.Linq.Expressions;



namespace CalcTest
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
				case 1: // expr = t1
					items = 1;
					state = Reduce(5, Expr.Add(At<Token>(0)));
					break;
				case 2: // expr = t2
					items = 1;
					state = Reduce(5, At(0));
					break;
				case 3: // expr = t3
					items = 1;
					state = Reduce(5, At(0));
					break;
				case 4: // t3_list = t3
					items = 1;
					state = Reduce(6, new List<T3>(At<Token>(0)));
					break;
	
			}
			return state;
		}
	
		#region Actions Table
		private static readonly int[,] _actions = 
		{
			{ -1, -1, 2, 3, 4, 1, -1 },
			{ -2, -1, -1, -1, -1, -1, -1 },
			{ -3, -1, -1, -1, -1, -1, -1 },
			{ -4, -1, -1, -1, -1, -1, -1 },
			{ -5, -1, -1, -1, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		protected const int _maxTerminal = 4;
		protected static readonly string[] _symbols =
		{
			"𝜖",
			"comma",
			"t1",
			"t2",
			"t3",
			"expr",
			"t3_list"
		};
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
	
		public void Dispose()
	    {
	        buffer.Dispose();
	        GC.SuppressFinalize(this);
	    }
	
	    public string FilePath { get; private set; }
	
	    /// <summary>Skipping ignore, returns next token</summary>
	    public Token NextToken()
	    {
	        Token token;
	        do { token = RawNextToken(); } 
	        while (token.State < TokenStates.SyntaxError);
	        return token;
	    }
	       
		public Token RawNextToken()
		{
			Token token;
	        if (ch == Eof)
	            return new Token(line, column, buffer.Position);
	        
	        var startPosition = new Position(line, column, buffer.Position - 1);
			MarkAccepting(TokenStates.SyntaxError);
	
			if (ch==',') goto State1;
			if (ch=='a') goto State2;
			if (ch=='b') goto State3;
			if (ch=='x') goto State4;
			goto State13;
		State1:
			MarkAccepting(TokenStates.comma);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State2:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='a') goto State7;
			goto EndState;
		State3:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State5;
			goto EndState;
		State4:
			MarkAccepting(TokenStates.t3);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State5:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State6;
			goto EndState;
		State6:
			MarkAccepting(TokenStates.t2);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State7:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='a') goto State8;
			goto EndState;
		State8:
			MarkAccepting(TokenStates.t1);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State9;
			goto EndState;
		State9:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State10;
			goto EndState;
		State10:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State11;
			goto EndState;
		State11:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='b') goto State12;
			goto EndState;
		State12:
			MarkAccepting(TokenStates.t1);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State13:
			MarkAccepting(TokenStates.SyntaxError);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch == ',' || ch=='a' || ch=='b' || ch == 'x') goto EndState;
			goto State13;
		EndState:
			if (lastAcceptingPosition == -1)
			{
				lastAcceptingPosition = buffer.Position;
			}
			token = new Token(startPosition, 
	            new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
	            lastAcceptingState, 
	            buffer.GetString(startPosition.Ch, lastAcceptingPosition));
			if (buffer.Position != lastAcceptingPosition)
			{
				buffer.Position = lastAcceptingPosition;
				line = lastLine;
				column = lastColumn;
				NextChar();
			}
			return token;
	
	    EndState2:
			token = new Token(startPosition, 
	            new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
	            lastAcceptingState,
	            buffer.GetString(startPosition.Ch, lastAcceptingPosition));
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
	
				prevLine = curLine.ToString();
				curLine.Clear();
		    }
		    else if (ch == '\r')
		    {
				++column;
		    }
		    else
		    { 
		        ++column;
				curLine.Append((char)ch);
		    }
	    }
	
		private string prevLine;
		private StringBuilder curLine = new StringBuilder();
	
		public string Line(int position)
		{
			if (position + 1 == line)
				return prevLine;
			if (position == line)
				return curLine.ToString() + buffer.PeekLine();
			return string.Empty;
		}
	}
	
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
		public const int comma = 1;
		public const int t1 = 2;
		public const int t2 = 3;
		public const int t3 = 4;
	}
	
	
	
}
