using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq.Expressions;
using ExprBuilder.Tree;


namespace ExprBuilder
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
						GetAction(reducedState, _stack.PeekState(), out result);
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
	            token = _scanner.NextToken();
	        }
	    }
	
		private uint Reduce(uint rule)
		{
			uint state = 0;
	
			switch (rule)
			{
				case 1: // expr = assignable;
				{
					state = _stack.SetItems(1)
					    .Reduce(16, _stack[0]);
					break;
				}
				case 2: // expr = unassignable;
				{
					state = _stack.SetItems(1)
					    .Reduce(16, _stack[0]);
					break;
				}
				case 3: // assignable = sum;
				{
					state = _stack.SetItems(1)
					    .Reduce(17, _stack[0]);
					break;
				}
				case 4: // unassignable = lambda;
				{
					state = _stack.SetItems(1)
					    .Reduce(18, _stack[0]);
					break;
				}
				case 5: // lambda = identifier "=>" sum;
				{
					state = _stack.SetItems(3)
					    .Reduce(20, Lambda((Token) _stack[0],(Expression) _stack[2]));
					break;
				}
				case 6: // sum = sum "+" term;
				{
					state = _stack.SetItems(3)
					    .Reduce(19, new AddBinary((Expr) _stack[0],(Expr) _stack[2]));
					break;
				}
				case 7: // sum = sum "-" term;
				{
					state = _stack.SetItems(3)
					    .Reduce(19, new SubtractBinary((Expr) _stack[0],(Expr) _stack[2]));
					break;
				}
				case 8: // sum = term;
				{
					state = _stack.SetItems(1)
					    .Reduce(19, _stack[0]);
					break;
				}
				case 9: // term = term "*" unary;
				{
					state = _stack.SetItems(3)
					    .Reduce(22, new MultiplyBinary((Expr) _stack[0],(Expr) _stack[2]));
					break;
				}
				case 10: // term = term "/" unary;
				{
					state = _stack.SetItems(3)
					    .Reduce(22, new DivideBinary((Expr) _stack[0],(Expr) _stack[2]));
					break;
				}
				case 11: // term = unary;
				{
					state = _stack.SetItems(1)
					    .Reduce(22, _stack[0]);
					break;
				}
				case 12: // unary = primary;
				{
					state = _stack.SetItems(1)
					    .Reduce(23, _stack[0]);
					break;
				}
				case 13: // unary = "-" primary;
				{
					state = _stack.SetItems(2)
					    .Reduce(23, new NegateUnary((Token) _stack[0],(Expr) _stack[1]));
					break;
				}
				case 14: // unary = "+" primary;
				{
					state = _stack.SetItems(2)
					    .Reduce(23, _stack[0]);
					break;
				}
				case 15: // unary = "!" primary;
				{
					state = _stack.SetItems(2)
					    .Reduce(23, new NotUnary((Token) _stack[0],(Expr) _stack[1]));
					break;
				}
				case 16: // unary = "~" primary;
				{
					state = _stack.SetItems(2)
					    .Reduce(23, new OnesComplementUnary((Token) _stack[0],(Expr) _stack[1]));
					break;
				}
				case 17: // primary = "(" expr ")";
				{
					state = _stack.SetItems(3)
					    .Reduce(24, _stack[1]);
					break;
				}
				case 18: // primary = constant;
				{
					state = _stack.SetItems(1)
					    .Reduce(24, _stack[0]);
					break;
				}
				case 19: // primary = identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(24, new VarExpr((Identifier) _stack[0]));
					break;
				}
				case 20: // identifier = Identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(21, new Identifier((Token) _stack[0]));
					break;
				}
				case 21: // constant = int;
				{
					state = _stack.SetItems(1)
					    .Reduce(25, _stack[0]);
					break;
				}
				case 22: // constant = double;
				{
					state = _stack.SetItems(1)
					    .Reduce(25, _stack[0]);
					break;
				}
				case 23: // constant = bool;
				{
					state = _stack.SetItems(1)
					    .Reduce(25, _stack[0]);
					break;
				}
				case 24: // int = Int;
				{
					state = _stack.SetItems(1)
					    .Reduce(26, new IntConstant((Token) _stack[0]));
					break;
				}
				case 25: // double = Double;
				{
					state = _stack.SetItems(1)
					    .Reduce(27, new DoubleConstant((Token) _stack[0]));
					break;
				}
				case 26: // double = DoubleInt;
				{
					state = _stack.SetItems(1)
					    .Reduce(27, DoubleConstant.ParseInt((Token) _stack[0]));
					break;
				}
				case 27: // bool = true;
				{
					state = _stack.SetItems(1)
					    .Reduce(28, new Constant<bool>((Token) _stack[0],true));
					break;
				}
				case 28: // bool = false;
				{
					state = _stack.SetItems(1)
					    .Reduce(28, new Constant<bool>((Token) _stack[0],false));
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
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, 1, 2, 3, 4, 5, 7, 6, 8, 10, 16, 17, 18, 19 },
			{ -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -5, -1, -1, -1, -1, -1, -1, -1, -1, -1, 25, 26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -10, -10, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -21, -1, -1, -1, -1, -1, -1, 29, -1, -1, -21, -21, -21, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -13, -13, -13, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -22, -1, -1, -1, -1, -1, -1, -22, -1, -1, -22, -22, -22, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -14, -14, -14, -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, -1, -1, 30, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, -1, -1, 31, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, -1, -1, 32, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, -1, -1, 33, 16, 17, 18, 19 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, 34, 35, 36, 37, 38, 40, 39, 41, 43, 49, 50, 51, 52 },
			{ -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -20, -20, -20, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -23, -23, -23, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -24, -24, -24, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -25, -25, -25, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -26, -26, -26, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -27, -27, -27, -27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -28, -28, -28, -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -29, -29, -29, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -30, -30, -30, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, -1, -1, -1, -1, -1, 7, 58, 8, 10, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, -1, -1, -1, -1, -1, 7, 59, 8, 10, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, -1, -1, -1, -1, -1, 7, -1, 60, 10, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, -1, -1, -1, -1, -1, 7, -1, 61, 10, 16, 17, 18, 19 },
			{ -1, 20, 21, 22, 9, 23, 24, -1, 15, -1, 12, 11, -1, -1, 13, 14, -1, -1, -1, 62, -1, 7, 6, 8, 10, 16, 17, 18, 19 },
			{ -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -15, -15, -15, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -16, -16, -16, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -17, -17, -17, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -18, -18, -18, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 63, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -5, 64, 65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -10, -10, -10, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, 68, -1, -21, -21, -21, -21, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -13, -13, -13, -13, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -22, -1, -22, -22, -22, -22, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -14, -14, -14, -14, -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, 69, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, 70, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, 71, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, 72, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, 73, 35, 36, 37, 38, 40, 39, 41, 43, 49, 50, 51, 52 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -20, -20, -20, -20, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -23, -23, -23, -23, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -24, -24, -24, -24, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -25, -25, -25, -25, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -26, -26, -26, -26, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -27, -27, -27, -27, -27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -28, -28, -28, -28, -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -29, -29, -29, -29, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -30, -30, -30, -30, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -8, -8, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -9, -9, 27, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -11, -11, -11, -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -12, -12, -12, -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -7, -1, -1, -1, -1, -1, -1, -1, -1, -1, 25, 26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -19, -19, -19, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, -1, -1, -1, -1, -1, 40, 74, 41, 43, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, -1, -1, -1, -1, -1, 40, 75, 41, 43, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, -1, -1, -1, -1, -1, 40, -1, 76, 43, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, -1, -1, -1, -1, -1, 40, -1, 77, 43, 49, 50, 51, 52 },
			{ -1, 53, 54, 55, 42, 56, 57, -1, 48, -1, 45, 44, -1, -1, 46, 47, -1, -1, -1, 78, -1, 40, 39, 41, 43, 49, 50, 51, 52 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -15, -15, -15, -15, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -16, -16, -16, -16, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -17, -17, -17, -17, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -18, -18, -18, -18, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, 79, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -8, -8, -8, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -9, -9, -9, 66, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -11, -11, -11, -11, -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -12, -12, -12, -12, -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -7, 64, 65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -19, -19, -19, -19, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		private const int _maxTerminal = 15;
		private string[] _symbols =
		{
			"",
			"Int",
			"Double",
			"DoubleInt",
			"Identifier",
			"true",
			"false",
			"=>",
			"(",
			")",
			"+",
			"-",
			"*",
			"/",
			"!",
			"~",
			"expr",
			"assignable",
			"unassignable",
			"sum",
			"lambda",
			"identifier",
			"term",
			"unary",
			"primary",
			"constant",
			"int",
			"double",
			"bool",
		};
	
		#endregion
		
	    /// <summary>
	    /// Called by reduction for an invalid, extra token
	    /// </summary>
	    /// <param name="t">Invalid token</param>
	    /// <param name="result">Value to return</param>
	    /// <returns>Result that allows parser to correctly</returns>
	    private object InvalidToken(Token t, object result)
	    {
	        _logger.LogError(t, "unexpected token {0}", t.Value);
	        return result;
	    }
	
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
	    private readonly IBuffer _buffer;
	    public const int Eof = -1;
	    private int _ch;
	    private int _line;
		private int _column;
	
		private int _lastAcceptingState;
	    private int _lastAcceptingPosition;
	    private int _lastLine;
	    private int _lastColumn;
	
	    public Scanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column) 
		{}
	
	    public Scanner(IBuffer buffer, int line = 1, int column = 0)
	    {
	        _buffer = buffer;
	        _line = line;
	        _column = column;
	        NextChar();
	    }
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0)
		{
			var text = File.ReadAllText(filePath);
			return new Scanner(text, line, column);
		}
	
		public void Dispose()
		{
			_buffer.Dispose();
		}
	
	    public string FilePath { get; private set; }
	
	    /// <summary>Skipping ignore, returns next token</summary>
	    public Token NextToken()
	    {
	        Token token;
	        do
	        {
	            token = RawNextToken();
	        } while (token.State < TokenStates.SyntaxError);
	
	        return token;
	    }
	       
		Token RawNextToken()
		{
	        Token token;
			if (_ch == Eof)
			{
	            token = new Token(_line, _column, _buffer.Position);
	            MarkAccepting(TokenStates.Empty);
				goto EndState;
			}
	        token = new Token(_line, _column, _buffer.Position - 1);
			MarkAccepting(TokenStates.SyntaxError);
	
			if (_ch=='0') goto State1;
			if (_ch=='.') goto State3;
			if (_ch=='t') goto State6;
			if (_ch=='f') goto State7;
			if (_ch=='=') goto State8;
			if (_ch=='(') goto State9;
			if (_ch==')') goto State10;
			if (_ch=='+') goto State11;
			if (_ch=='-') goto State12;
			if (_ch=='*') goto State13;
			if (_ch=='/') goto State14;
			if (_ch=='!') goto State15;
			if (_ch=='~') goto State16;
			if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State5;
			if ((_ch>='1' && _ch<='9')) goto State2;
			if ((_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='e') || (_ch>='g' && _ch<='s') || (_ch>='u' && _ch<='z')) goto State4;
			goto EndState;
		State1:
			MarkAccepting(TokenStates.Int);
			NextChar();
			if (_ch=='.') goto State3;
			if (_ch=='D') goto State25;
			goto EndState;
		State2:
			MarkAccepting(TokenStates.Int);
			NextChar();
			if (_ch=='.') goto State3;
			if (_ch=='D') goto State25;
			if ((_ch>='0' && _ch<='9')) goto State2;
			goto EndState;
		State3:
			MarkAccepting(TokenStates.Double);
			NextChar();
			if ((_ch>='0' && _ch<='9')) goto State3;
			goto EndState;
		State4:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State4;
			goto EndState;
		State5:
			MarkAccepting(TokenStates.white_space);
			NextChar();
			if (_ch=='\t' || _ch=='\n' || _ch == ' ') goto State5;
			goto EndState;
		State6:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='r') goto State22;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='q') || (_ch>='s' && _ch<='z')) goto State4;
			goto EndState;
		State7:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='a') goto State18;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='b' && _ch<='z')) goto State4;
			goto EndState;
		State8:
			NextChar();
			if (_ch=='>') goto State17;
			goto EndState;
		State9:
			MarkAccepting(TokenStates.LeftParen);
			NextChar();
			goto EndState;
		State10:
			MarkAccepting(TokenStates.RightParen);
			NextChar();
			goto EndState;
		State11:
			MarkAccepting(TokenStates.Plus);
			NextChar();
			goto EndState;
		State12:
			MarkAccepting(TokenStates.Minus);
			NextChar();
			goto EndState;
		State13:
			MarkAccepting(TokenStates.Asterisk);
			NextChar();
			goto EndState;
		State14:
			MarkAccepting(TokenStates.Slash);
			NextChar();
			goto EndState;
		State15:
			MarkAccepting(TokenStates.Exclaimation);
			NextChar();
			goto EndState;
		State16:
			MarkAccepting(TokenStates.Tilde);
			NextChar();
			goto EndState;
		State17:
			MarkAccepting(TokenStates.EqualGreaterThan);
			NextChar();
			goto EndState;
		State18:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='l') goto State19;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='k') || (_ch>='m' && _ch<='z')) goto State4;
			goto EndState;
		State19:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='s') goto State20;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='r') || (_ch>='t' && _ch<='z')) goto State4;
			goto EndState;
		State20:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='e') goto State21;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='d') || (_ch>='f' && _ch<='z')) goto State4;
			goto EndState;
		State21:
			MarkAccepting(TokenStates.False);
			NextChar();
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State4;
			goto EndState;
		State22:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='u') goto State23;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='t') || (_ch>='v' && _ch<='z')) goto State4;
			goto EndState;
		State23:
			MarkAccepting(TokenStates.Identifier);
			NextChar();
			if (_ch=='e') goto State24;
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='d') || (_ch>='f' && _ch<='z')) goto State4;
			goto EndState;
		State24:
			MarkAccepting(TokenStates.True);
			NextChar();
			if ((_ch>='0' && _ch<='9') || (_ch>='A' && _ch<='Z') || _ch == '_' || (_ch>='a' && _ch<='z')) goto State4;
			goto EndState;
		State25:
			MarkAccepting(TokenStates.DoubleInt);
			NextChar();
			goto EndState;
		EndState:
			if (_lastAcceptingState == TokenStates.SyntaxError)
			{
				_lastAcceptingPosition = _buffer.Position - 1;
				_lastAcceptingState = -1;
			}
	
			var value = _buffer.GetString(token.Beg, _lastAcceptingPosition);
			token.Set(_lastAcceptingState, value, _lastLine, _lastColumn, _lastAcceptingPosition - 1);
			if (_buffer.Position != _lastAcceptingPosition)
			{
				_buffer.Position = _lastAcceptingPosition;
				_line = _lastLine;
				_column = _lastColumn;
				NextChar();
			}
			return token;
		}
	
	    void MarkAccepting(int type)
	    {
	        _lastAcceptingState = type;
	        _lastAcceptingPosition = _buffer.Position;
	        _lastLine = _line;
	        _lastColumn = _column;
	    }
	
	    /// <summary>
	    /// Retrieves the next character, adjusting position information
	    /// </summary>
	    private void NextChar()
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
	}
	
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
		public const int Int = 1;
		public const int Double = 2;
		public const int DoubleInt = 3;
		public const int Identifier = 4;
		public const int True = 5;
		public const int False = 6;
		public const int EqualGreaterThan = 7;
		public const int LeftParen = 8;
		public const int RightParen = 9;
		public const int Plus = 10;
		public const int Minus = 11;
		public const int Asterisk = 12;
		public const int Slash = 13;
		public const int Exclaimation = 14;
		public const int Tilde = 15;
		public const int white_space = -2;
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
	    private StringBuilder _builder;
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
