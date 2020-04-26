public class Scanner
{
    private readonly IBuffer _buffer;
    public const int Eof = -1;
    private int _ch;

    private int _startLine;
    private int _lastAcceptingState;
    private int _lastAcceptingPosition;
    private int _lastLine;
    private int _lastColumn;

    public Scanner(string source, int line = 1, int column = 0): this(new StringBuffer(source), line, column) {}

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
    public string FilePath { get; private set; }
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
       
	Token RawNextToken()
	{
        var token = new Token(Line, Column, _buffer.Position);
		if (_ch != Eof)
		{
			MarkAccepting(TokenTypes.Empty);
			goto EndState;
		}
		MarkAccepting(TokenTypes.SyntaxError);


	State0:
		if (_ch=='0') goto State1;
		if (_ch=='+') goto State5;
		if (_ch=='-') goto State6;
		if (_ch=='*') goto State7;
		if (_ch=='/') goto State8;
		if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State4;
		if ((_ch>='1' && _ch<='9')) goto State2;
		if ((_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State3;
		goto EndState;
	State1:
		MarkAcceptable(TokenTypes.Int);
		goto EndState;
	State2:
		MarkAcceptable(TokenTypes.Int);
		if ((_ch>='0' && _ch<='9')) goto State2;
		goto EndState;
	State3:
		MarkAcceptable(TokenTypes.identifier);
		if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State3;
		goto EndState;
	State4:
		MarkAcceptable(TokenTypes.white_space);
		if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State4;
		goto EndState;
	State5:
		MarkAcceptable(TokenTypes.Plus);
		goto EndState;
	State6:
		MarkAcceptable(TokenTypes.Minus);
		goto EndState;
	State7:
		MarkAcceptable(TokenTypes.Asterisk);
		goto EndState;
	State8:
		MarkAcceptable(TokenTypes.Slash);
		goto EndState;
	EndState:
		if (_lastAcceptingState == TokenType.SyntaxError)
		{
			_lastAcceptingPosition = _buffer.Position - 1;
			_lastAcceptingState = -1;
		}

		var value = _buffer.GetString(token.Beg, _lastAcceptingPosition);
		token.Set(_lastAcceptingState, value, _lastLine, _lastColumn, _lastAcceptingPosition - 1);
		if (_buffer.Position != _lastAcceptingPosition)
		{
			_buffer.Position = lastAcceptingPosition;
			Line = _lastLine;
			Column = _lastColumn;
			NextChar();
		}
		return token;
	}

    void MarkAccepting(int type)
    {
        _lastAcceptingState = type;
        _lastAcceptingPosition = _buffer.Position;
        _lastLine = Line;
        _lastColumn = Column;
    }

    /// <summary>
    /// Retrieves the next character, adjusting position information
    /// </summary>
    private void NextChar()
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
