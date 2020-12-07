using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace Words
{
	
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
	       
		Token RawNextToken()
		{
			Token token;
	        if (ch == Eof)
	            return new Token(line, column, buffer.Position);
	        
	        var startPosition = new Position(line, column, buffer.Position - 1);
			MarkAccepting(TokenStates.SyntaxError);
	
			if (ch=='0') goto State2;
			if (ch=='/') goto State3;
			if ((ch>='A' && ch<='Z') || (ch>='a' && ch<='z')) goto State1;
			goto EndState2;
		State1:
			MarkAccepting(TokenStates.word);
			NextChar();
			if (ch == '-' || (ch>='A' && ch<='Z') || (ch>='a' && ch<='z')) goto State1;
			goto EndState;
		State2:
			NextChar();
			if (ch=='x') goto State8;
			goto EndState;
		State3:
			NextChar();
			if (ch=='/') goto State4;
			if (ch=='*') goto State5;
			goto EndState;
		State4:
			MarkAccepting(TokenStates.comment);
			NextChar();
			if (!((ch==-1) ||ch == '\n')) goto State4;
			goto EndState;
		State5:
			NextChar();
			if (ch=='*') goto State6;
			if (!((ch==-1) ||ch == '*')) goto State5;
			goto EndState;
		State6:
			NextChar();
			if (ch=='/') goto State7;
			if (!((ch==-1) ||ch == '/')) goto State5;
			goto EndState;
		State7:
			MarkAccepting(TokenStates.multi_line_comment);
			NextChar();
			goto EndState;
		State8:
			NextChar();
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='F') || (ch>='a' && ch<='f')) goto State9;
			goto EndState;
		State9:
			MarkAccepting(TokenStates.hexadecimal);
			NextChar();
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='F') || (ch>='a' && ch<='f')) goto State9;
			goto EndState;
		EndState:
			if (lastAcceptingState == TokenStates.SyntaxError)
			{
				lastAcceptingPosition = buffer.Position - 1;
				lastAcceptingState = -1;
			}
			token = new Token(startPosition, 
	            new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
	            lastAcceptingState, 
	            buffer.GetString(token.Beg, lastAcceptingPosition));
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
	            buffer.GetString(token.Beg, lastAcceptingPosition));
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
		public const int word = 1;
		public const int hexadecimal = 2;
		public const int comment = 3;
		public const int multi_line_comment = 4;
	}
	
	
	
}
