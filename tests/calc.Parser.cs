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
				case 1: // expr = term
					items = 1;
					state = Reduce(8, At(0));
					break;
				case 2: // expr = expr "+" term
					items = 3;
					state = Reduce(8, new AddExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 3: // expr = expr "-" term
					items = 3;
					state = Reduce(8, new SubExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 4: // term = primary
					items = 1;
					state = Reduce(9, At(0));
					break;
				case 5: // term = term "*" primary
					items = 3;
					state = Reduce(9, new MultiExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 6: // term = term "/" primary
					items = 3;
					state = Reduce(9, new DivExpr(At<Expr>(0),At<Expr>(2)));
					break;
				case 7: // primary = Int
					items = 1;
					state = Reduce(10, new Constant(At<Token>(0)));
					break;
	
			}
			return state;
		}
	
		#region Actions Table
		private static readonly int[,] _actions = 
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
		protected const int _maxTerminal = 7;
		protected static readonly string[] _symbols =
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
	
	}
	
	public class Scanner: StateScannerBase
	{
	    private static readonly int[] _charToClass;
	    private static readonly int[,] _states;
	
		static Scanner()
		{
	        _charToClass = Opal.CharClasses.Decompress8(_charToClassCompressed);
			_states = Opal.ScannerStates.Decompress8(_compressedStates,
	  maxClasses: 10,
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
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xED, 0xC9, 0xB1, 0x0E, 0x80, 0x20, 
			0x10, 0x44, 0xC1, 0x53, 0x0E, 0xF5, 0xFF, 0xBF, 0x98, 0xC4, 0x40, 0x88, 0x05, 0xA1, 0xB5, 0x98, 
			0xA9, 0x36, 0xFB, 0x22, 0xBA, 0xCC, 0xB1, 0x3E, 0xE6, 0x7B, 0xD7, 0xB8, 0xE2, 0x39, 0xCE, 0xA1, 
			0xDF, 0x65, 0xE9, 0xAD, 0x9B, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x00, 0xC0, 0x4F, 0x34, 0xE7, 0xB9, 0x31, 0x08, 0x00, 0x00, 0x01, 0x00, 
		};
	
		private static readonly byte[] _compressedStates = 
		{
			0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x45, 0xC6, 0x49, 0x12, 0x00, 0x20, 
			0x08, 0x03, 0x41, 0x76, 0xCC, 0xFF, 0x1F, 0xAC, 0x55, 0xA0, 0x32, 0x87, 0xA4, 0x09, 0x2C, 0x6A, 
			0x1E, 0xB9, 0xC0, 0xF4, 0x63, 0x12, 0xB9, 0x14, 0x52, 0xD5, 0x66, 0x9D, 0x15, 0x7B, 0x2B, 0x1F, 
			0xC6, 0x30, 0x87, 0x1B, 0x4F, 0x38, 0x8F, 0x9D, 0x10, 0xD7, 0x6E, 0x00, 0x00, 0x00, 
		};
	
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

}
