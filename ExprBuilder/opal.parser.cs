using Opal;
using System.Linq.Expressions;
using ExprBuilder.Tree;



namespace ExprBuilder
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
				case 1: // expr = assignable
					items = 1;
					state = Reduce(16, At(0));
					break;
				case 2: // expr = unassignable
					items = 1;
					state = Reduce(16, At(0));
					break;
				case 3: // assignable = sum
					items = 1;
					state = Reduce(17, At(0));
					break;
				case 4: // unassignable = lambda
					items = 1;
					state = Reduce(18, At(0));
					break;
				case 5: // lambda = identifier "=>" sum
					items = 3;
					state = Reduce(19, Lambda(At<Token>(0),At<Expression>(2)));
					break;
				case 6: // lambda_parameters = identifier
					items = 1;
					state = Reduce(20, At(0));
					break;
				case 7: // lambda_parameters = "(" ")"
					items = 2;
					state = Reduce(20, At(0));
					break;
				case 8: // sum = sum "+" term
					items = 3;
					state = Reduce(21, new AddBinary(At<Expr>(0),At<Expr>(2)));
					break;
				case 9: // sum = sum "-" term
					items = 3;
					state = Reduce(21, new SubtractBinary(At<Expr>(0),At<Expr>(2)));
					break;
				case 10: // sum = term
					items = 1;
					state = Reduce(21, At(0));
					break;
				case 11: // term = term "*" unary
					items = 3;
					state = Reduce(22, new MultiplyBinary(At<Expr>(0),At<Expr>(2)));
					break;
				case 12: // term = term "/" unary
					items = 3;
					state = Reduce(22, new DivideBinary(At<Expr>(0),At<Expr>(2)));
					break;
				case 13: // term = unary
					items = 1;
					state = Reduce(22, At(0));
					break;
				case 14: // unary = primary
					items = 1;
					state = Reduce(23, At(0));
					break;
				case 15: // unary = "-" primary
					items = 2;
					state = Reduce(23, new NegateUnary(At<Token>(0),At<Expr>(1)));
					break;
				case 16: // unary = "+" primary
					items = 2;
					state = Reduce(23, At(0));
					break;
				case 17: // unary = "!" primary
					items = 2;
					state = Reduce(23, new NotUnary(At<Token>(0),At<Expr>(1)));
					break;
				case 18: // unary = "~" primary
					items = 2;
					state = Reduce(23, new OnesComplementUnary(At<Token>(0),At<Expr>(1)));
					break;
				case 19: // primary = "(" expr ")"
					items = 3;
					state = Reduce(24, At(1));
					break;
				case 20: // primary = constant
					items = 1;
					state = Reduce(24, At(0));
					break;
				case 21: // primary = identifier
					items = 1;
					state = Reduce(24, new VarExpr(At<Identifier>(0)));
					break;
				case 22: // identifier = Identifier
					items = 1;
					state = Reduce(25, new Identifier(At<Token>(0)));
					break;
				case 23: // constant = int
					items = 1;
					state = Reduce(26, At(0));
					break;
				case 24: // constant = double
					items = 1;
					state = Reduce(26, At(0));
					break;
				case 25: // constant = bool
					items = 1;
					state = Reduce(26, At(0));
					break;
				case 26: // int = Int
					items = 1;
					state = Reduce(27, new IntConstant(At<Token>(0)));
					break;
				case 27: // double = Double
					items = 1;
					state = Reduce(28, new DoubleConstant(At<Token>(0)));
					break;
				case 28: // double = DoubleInt
					items = 1;
					state = Reduce(28, DoubleConstant.ParseInt(At<Token>(0)));
					break;
				case 29: // bool = "true"
					items = 1;
					state = Reduce(29, new Constant<bool>(At<Token>(0),true));
					break;
				case 30: // bool = "false"
					items = 1;
					state = Reduce(29, new Constant<bool>(At<Token>(0),false));
					break;
	
			}
			return state;
		}
	
		#region Actions Table
		private static readonly int[,] _actions = 
		{
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, 1, 2, 3, 5, -1, 4, 6, 8, 10, 7, 16, 17, 18, 19 },
			{ -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -5, -1, -1, -1, -1, -1, -1, -1, 25, 26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -12, -1, -1, -1, -1, -1, -1, -1, -12, -12, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -23, -1, -1, -1, -1, 29, -1, -1, -23, -23, -23, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -15, -1, -1, -1, -1, -1, -1, -1, -15, -15, -15, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -24, -1, -1, -1, -1, -24, -1, -1, -24, -24, -24, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -16, -1, -1, -1, -1, -1, -1, -1, -16, -16, -16, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, -1, -1, -1, -1, -1, -1, 23, 24, -1, -1, -1, -1, -1, -1, -1, -1, 30, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, -1, -1, -1, -1, -1, -1, 23, 24, -1, -1, -1, -1, -1, -1, -1, -1, 31, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, -1, -1, -1, -1, -1, -1, 23, 24, -1, -1, -1, -1, -1, -1, -1, -1, 32, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, -1, -1, -1, -1, -1, -1, 23, 24, -1, -1, -1, -1, -1, -1, -1, -1, 33, 7, 16, 17, 18, 19 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, 34, 35, 36, 38, -1, 37, 39, 41, 43, 40, 49, 50, 51, 52 },
			{ -22, -1, -1, -1, -1, -1, -1, -1, -22, -22, -22, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -25, -1, -1, -1, -1, -1, -1, -1, -25, -25, -25, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -26, -1, -1, -1, -1, -1, -1, -1, -26, -26, -26, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -27, -1, -1, -1, -1, -1, -1, -1, -27, -27, -27, -27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -28, -1, -1, -1, -1, -1, -1, -1, -28, -28, -28, -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -29, -1, -1, -1, -1, -1, -1, -1, -29, -29, -29, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -30, -1, -1, -1, -1, -1, -1, -1, -30, -30, -30, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -31, -1, -1, -1, -1, -1, -1, -1, -31, -31, -31, -31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -32, -1, -1, -1, -1, -1, -1, -1, -32, -32, -32, -32, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, -1, -1, -1, -1, -1, -1, 58, 8, 10, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, -1, -1, -1, -1, -1, -1, 59, 8, 10, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, -1, -1, -1, -1, -1, -1, -1, 60, 10, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, -1, -1, -1, -1, -1, -1, -1, 61, 10, 7, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, -1, 15, -1, 12, 11, -1, -1, 13, 14, 23, 24, -1, -1, -1, -1, -1, 62, 6, 8, 10, 7, 16, 17, 18, 19 },
			{ -17, -1, -1, -1, -1, -1, -1, -1, -17, -17, -17, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -18, -1, -1, -1, -1, -1, -1, -1, -18, -18, -18, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -19, -1, -1, -1, -1, -1, -1, -1, -19, -19, -19, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -20, -1, -1, -1, -1, -1, -1, -1, -20, -20, -20, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 63, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -5, 64, 65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -12, -12, -12, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, 68, -1, -23, -23, -23, -23, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -15, -15, -15, -15, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -24, -1, -24, -24, -24, -24, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -16, -16, -16, -16, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, -1, -1, -1, -1, -1, -1, 56, 57, -1, -1, -1, -1, -1, -1, -1, -1, 69, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, -1, -1, -1, -1, -1, -1, 56, 57, -1, -1, -1, -1, -1, -1, -1, -1, 70, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, -1, -1, -1, -1, -1, -1, 56, 57, -1, -1, -1, -1, -1, -1, -1, -1, 71, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, -1, -1, -1, -1, -1, -1, 56, 57, -1, -1, -1, -1, -1, -1, -1, -1, 72, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, 73, 35, 36, 38, -1, 37, 39, 41, 43, 40, 49, 50, 51, 52 },
			{ -1, -1, -1, -1, -1, -1, -1, -22, -22, -22, -22, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -25, -25, -25, -25, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -26, -26, -26, -26, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -27, -27, -27, -27, -27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -28, -28, -28, -28, -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -29, -29, -29, -29, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -30, -30, -30, -30, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -31, -31, -31, -31, -31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -32, -32, -32, -32, -32, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -10, -1, -1, -1, -1, -1, -1, -1, -10, -10, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -11, -1, -1, -1, -1, -1, -1, -1, -11, -11, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -13, -1, -1, -1, -1, -1, -1, -1, -13, -13, -13, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -14, -1, -1, -1, -1, -1, -1, -1, -14, -14, -14, -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -7, -1, -1, -1, -1, -1, -1, -1, 25, 26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -21, -1, -1, -1, -1, -1, -1, -1, -21, -21, -21, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, -1, -1, -1, -1, -1, -1, 74, 41, 43, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, -1, -1, -1, -1, -1, -1, 75, 41, 43, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, -1, -1, -1, -1, -1, -1, -1, 76, 43, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, -1, -1, -1, -1, -1, -1, -1, 77, 43, 40, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, -1, 48, -1, 45, 44, -1, -1, 46, 47, 56, 57, -1, -1, -1, -1, -1, 78, 39, 41, 43, 40, 49, 50, 51, 52 },
			{ -1, -1, -1, -1, -1, -1, -1, -17, -17, -17, -17, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -18, -18, -18, -18, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -19, -19, -19, -19, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -20, -20, -20, -20, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 79, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -10, -10, -10, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -11, -11, -11, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -13, -13, -13, -13, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -14, -14, -14, -14, -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -7, 64, 65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -21, -21, -21, -21, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		protected const int _maxTerminal = 15;
		protected static readonly string[] _symbols =
		{
			"𝜖",
			"Int",
			"Double",
			"DoubleInt",
			"Identifier",
			"=>",
			"(",
			")",
			"+",
			"-",
			"*",
			"/",
			"!",
			"~",
			"true",
			"false",
			"expr",
			"assignable",
			"unassignable",
			"lambda",
			"lambda_parameters",
			"sum",
			"term",
			"unary",
			"primary",
			"identifier",
			"constant",
			"int",
			"double",
			"bool"
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
	
			if (ch=='0') goto State1;
			if (ch=='.') goto State3;
			if (ch=='=') goto State6;
			if (ch=='(') goto State7;
			if (ch==')') goto State8;
			if (ch=='+') goto State9;
			if (ch=='-') goto State10;
			if (ch=='*') goto State11;
			if (ch=='/') goto State12;
			if (ch=='!') goto State13;
			if (ch=='~') goto State14;
			if (ch=='t') goto State15;
			if (ch=='f') goto State16;
			if ((ch>='1' && ch<='9')) goto State2;
			if (ch=='\t' || ch=='\n' || ch == ' ') goto State5;
			if ((ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='e') || (ch>='g' && ch<='s') || (ch>='u' && ch<='z')) goto State4;
			goto State26;
		State1:
			MarkAccepting(TokenStates.Int);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='.') goto State3;
			if (ch=='D') goto State25;
			goto EndState;
		State2:
			MarkAccepting(TokenStates.Int);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='.') goto State3;
			if (ch=='D') goto State25;
			if ((ch>='0' && ch<='9')) goto State2;
			goto EndState;
		State3:
			MarkAccepting(TokenStates.Double);
			NextChar();
			if (ch == -1) goto EndState;
			if ((ch>='0' && ch<='9')) goto State3;
			goto EndState;
		State4:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='z')) goto State4;
			goto EndState;
		State5:
			MarkAccepting(TokenStates.white_space);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='\t' || ch=='\n' || ch == ' ') goto State5;
			goto EndState;
		State6:
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='>') goto State24;
			goto EndState;
		State7:
			MarkAccepting(TokenStates.LeftParen);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State8:
			MarkAccepting(TokenStates.RightParen);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State9:
			MarkAccepting(TokenStates.Plus);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State10:
			MarkAccepting(TokenStates.Minus);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State11:
			MarkAccepting(TokenStates.Asterisk);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State12:
			MarkAccepting(TokenStates.Slash);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State13:
			MarkAccepting(TokenStates.Exclamation);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State14:
			MarkAccepting(TokenStates.Tilde);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State15:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='r') goto State21;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='q') || (ch>='s' && ch<='z')) goto State4;
			goto EndState;
		State16:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='a') goto State17;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='b' && ch<='z')) goto State4;
			goto EndState;
		State17:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='l') goto State18;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='k') || (ch>='m' && ch<='z')) goto State4;
			goto EndState;
		State18:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='s') goto State19;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='r') || (ch>='t' && ch<='z')) goto State4;
			goto EndState;
		State19:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='e') goto State20;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='d') || (ch>='f' && ch<='z')) goto State4;
			goto EndState;
		State20:
			MarkAccepting(TokenStates.@false);
			NextChar();
			if (ch == -1) goto EndState;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='z')) goto State4;
			goto EndState;
		State21:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='u') goto State22;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='t') || (ch>='v' && ch<='z')) goto State4;
			goto EndState;
		State22:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='e') goto State23;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='d') || (ch>='f' && ch<='z')) goto State4;
			goto EndState;
		State23:
			MarkAccepting(TokenStates.@true);
			NextChar();
			if (ch == -1) goto EndState;
			if ((ch>='0' && ch<='9') || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='z')) goto State4;
			goto EndState;
		State24:
			MarkAccepting(TokenStates.EqualGreaterThan);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State25:
			MarkAccepting(TokenStates.DoubleInt);
			NextChar();
			if (ch == -1) goto EndState;
			goto EndState;
		State26:
			MarkAccepting(TokenStates.SyntaxError);
			NextChar();
			if (ch == -1) goto EndState;
			if (ch=='\t' || ch=='\n' || ch==' ' || ch=='!' || (ch>='(' && ch<='+') || (ch>='-' && ch<='9') || ch == '=' || (ch>='A' && ch<='Z') || ch == '_' || (ch>='a' && ch<='z') || ch == '~') goto EndState;
			goto State26;
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
		public const int Int = 1;
		public const int Double = 2;
		public const int DoubleInt = 3;
		public const int Identifier = 4;
		public const int EqualGreaterThan = 5;
		public const int LeftParen = 6;
		public const int RightParen = 7;
		public const int Plus = 8;
		public const int Minus = 9;
		public const int Asterisk = 10;
		public const int Slash = 11;
		public const int Exclamation = 12;
		public const int Tilde = 13;
		public const int @true = 14;
		public const int @false = 15;
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
