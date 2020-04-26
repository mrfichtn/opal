using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Opal.Nfa;
using Opal.ParseTree;


namespace Opal
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
				case 1: // language = using_section namespace_section option_section characters_section token_section production_section;
				{
					state = _stack.SetItems(6)
					    .Reduce(31, new Language((Identifier) _stack[1],(Graph) _stack[4],_productions));
					break;
				}
				case 2: // using_section = ;
				{
					state = _stack.Push(32, null);
					break;
				}
				case 3: // using_section = using_section using_decl;
				{
					state = _stack.SetItems(2)
					    .Reduce(32, null);
					break;
				}
				case 4: // using_decl = "using" Identifier ";";
				{
					state = _stack.SetItems(3)
					    .Reduce(38, AddNamespace((Identifier)_stack[1]));
					break;
				}
				case 5: // namespace_section = ;
				{
					state = _stack.Push(33, null);
					break;
				}
				case 6: // namespace_section = "namespace" Identifier ";";
				{
					state = _stack.SetItems(3)
					    .Reduce(33, _stack[1]);
					break;
				}
				case 7: // option_section = ;
				{
					state = _stack.Push(34, null);
					break;
				}
				case 8: // option_section = "Options" option_list;
				{
					state = _stack.SetItems(2)
					    .Reduce(34, null);
					break;
				}
				case 9: // option_list = option;
				{
					state = _stack.SetItems(1)
					    .Reduce(40, _stack[0]);
					break;
				}
				case 10: // option_list = option_list option;
				{
					state = _stack.SetItems(2)
					    .Reduce(40, null);
					break;
				}
				case 11: // option = identifier "=" String;
				{
					state = _stack.SetItems(3)
					    .Reduce(41, AddOption((Token)_stack[0],(StringConst) _stack[2]));
					break;
				}
				case 12: // characters_section = ;
				{
					state = _stack.Push(35, null);
					break;
				}
				case 13: // characters_section = "Characters" character_expr;
				{
					state = _stack.SetItems(2)
					    .Reduce(35, null);
					break;
				}
				case 14: // character_expr = character_pair;
				{
					state = _stack.SetItems(1)
					    .Reduce(43, Add((NamedCharClass)_stack[0]));
					break;
				}
				case 15: // character_expr = character_expr character_pair;
				{
					state = _stack.SetItems(2)
					    .Reduce(43, Add((NamedCharClass)_stack[1]));
					break;
				}
				case 16: // character_pair = identifier "=" char_class_concat;
				{
					state = _stack.SetItems(3)
					    .Reduce(44, new NamedCharClass((Token)_stack[0],(IMatch) _stack[2]));
					break;
				}
				case 17: // char_class_concat = char_class_unary;
				{
					state = _stack.SetItems(1)
					    .Reduce(45, _stack[0]);
					break;
				}
				case 18: // char_class_concat = char_class_concat "+" char_class_unary;
				{
					state = _stack.SetItems(3)
					    .Reduce(45, Match.Union((IMatch) _stack[0],(Token)_stack[1],(IMatch) _stack[2]));
					break;
				}
				case 19: // char_class_concat = char_class_concat "-" char_class_unary;
				{
					state = _stack.SetItems(3)
					    .Reduce(45, Match.Difference((IMatch) _stack[0],(Token)_stack[1],(IMatch) _stack[2]));
					break;
				}
				case 20: // char_class_unary = char_class_primary;
				{
					state = _stack.SetItems(1)
					    .Reduce(46, _stack[0]);
					break;
				}
				case 21: // char_class_unary = "!" char_class_unary;
				{
					state = _stack.SetItems(2)
					    .Reduce(46, Match.Invert((Token)_stack[0],(IMatch) _stack[1]));
					break;
				}
				case 22: // char_class_primary = CharClass;
				{
					state = _stack.SetItems(1)
					    .Reduce(47, _stack[0]);
					break;
				}
				case 23: // char_class_primary = identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(47, FindCharClass((Token)_stack[0]));
					break;
				}
				case 24: // char_class_primary = Char;
				{
					state = _stack.SetItems(1)
					    .Reduce(47, new SingleChar((CharConst) _stack[0]));
					break;
				}
				case 25: // token_section = ;
				{
					state = _stack.Push(36, SetGraph());
					break;
				}
				case 26: // token_section = "Tokens" token_list;
				{
					state = _stack.SetItems(2)
					    .Reduce(36, _stack[1]);
					break;
				}
				case 27: // token_list = token;
				{
					state = _stack.SetItems(1)
					    .Reduce(50, SetGraph((Graph) _stack[0]));
					break;
				}
				case 28: // token_list = token_list token;
				{
					state = _stack.SetItems(2)
					    .Reduce(50, Graph.Union((Graph) _stack[1]));
					break;
				}
				case 29: // token = identifier token_attr "=" token_expr ";";
				{
					state = _stack.SetItems(5)
					    .Reduce(51, Graph.MarkEnd((Token)_stack[0],(Token) _stack[1],(Graph) _stack[3]));
					break;
				}
				case 30: // token_attr = ;
				{
					state = _stack.Push(52, null);
					break;
				}
				case 31: // token_attr = "<" identifier ">";
				{
					state = _stack.SetItems(3)
					    .Reduce(52, _stack[1]);
					break;
				}
				case 32: // token_expr = token_quantifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(53, _stack[0]);
					break;
				}
				case 33: // token_expr = token_expr "|" token_quantifier;
				{
					state = _stack.SetItems(3)
					    .Reduce(53, Graph.Union((Graph) _stack[0],(Graph) _stack[2]));
					break;
				}
				case 34: // token_expr = token_expr token_quantifier;
				{
					state = _stack.SetItems(2)
					    .Reduce(53, Graph.Concatenate((Graph) _stack[0],(Graph) _stack[1]));
					break;
				}
				case 35: // token_quantifier = token_primary;
				{
					state = _stack.SetItems(1)
					    .Reduce(54, _stack[0]);
					break;
				}
				case 36: // token_quantifier = token_primary "+";
				{
					state = _stack.SetItems(2)
					    .Reduce(54, Graph.PlusClosure((Graph) _stack[0]));
					break;
				}
				case 37: // token_quantifier = token_primary "*";
				{
					state = _stack.SetItems(2)
					    .Reduce(54, Graph.StarClosure((Graph) _stack[0]));
					break;
				}
				case 38: // token_quantifier = token_primary "?";
				{
					state = _stack.SetItems(2)
					    .Reduce(54, Graph.QuestionClosure((Graph) _stack[0]));
					break;
				}
				case 39: // token_quantifier = token_primary "{" Integer "}";
				{
					state = _stack.SetItems(4)
					    .Reduce(54, Graph.Quantifier((Graph) _stack[0],(Integer) _stack[2]));
					break;
				}
				case 40: // token_quantifier = token_primary "{" Integer "," Integer "}";
				{
					state = _stack.SetItems(6)
					    .Reduce(54, Graph.RangeQuantifier((Graph) _stack[0],(Integer) _stack[2],(Integer) _stack[4]));
					break;
				}
				case 41: // token_primary = identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(55, CreateGraph(FindCharClass((Token)_stack[0])));
					break;
				}
				case 42: // token_primary = Char;
				{
					state = _stack.SetItems(1)
					    .Reduce(55, CreateGraph((EscChar) _stack[0]));
					break;
				}
				case 43: // token_primary = CharClass;
				{
					state = _stack.SetItems(1)
					    .Reduce(55, CreateGraph((IMatch) _stack[0]));
					break;
				}
				case 44: // token_primary = String;
				{
					state = _stack.SetItems(1)
					    .Reduce(55, Graph.Create((StringConst) _stack[0]));
					break;
				}
				case 45: // token_primary = "(" token_expr ")";
				{
					state = _stack.SetItems(3)
					    .Reduce(55, _stack[1]);
					break;
				}
				case 46: // production_section = "Productions" identifier prod_list;
				{
					state = _stack.SetItems(3)
					    .Reduce(37, _productions.SetLanguage((Token)_stack[1]));
					break;
				}
				case 47: // prod_list = ;
				{
					state = _stack.Push(57, null);
					break;
				}
				case 48: // prod_list = prod_list production;
				{
					state = _stack.SetItems(2)
					    .Reduce(57, null);
					break;
				}
				case 49: // production = identifier production_attr "=" prod_def_list;
				{
					state = _stack.SetItems(4)
					    .Reduce(58, _productions.Add((Token)_stack[0],(ProductionAttr)_stack[1],(ProdDefList) _stack[3]));
					break;
				}
				case 50: // production_attr = ;
				{
					state = _stack.Push(59, null);
					break;
				}
				case 51: // production_attr = "<" Identifier func_opt ">";
				{
					state = _stack.SetItems(4)
					    .Reduce(59, new ProductionAttr((Identifier)_stack[1],(FuncOption)_stack[2]));
					break;
				}
				case 52: // func_opt = ;
				{
					state = _stack.Push(61, null);
					break;
				}
				case 53: // func_opt = "(" func_opt_arg_type ")";
				{
					state = _stack.SetItems(3)
					    .Reduce(61, new FuncOption((Identifier) _stack[1]));
					break;
				}
				case 54: // func_opt_arg_type = ;
				{
					state = _stack.Push(62, null);
					break;
				}
				case 55: // func_opt_arg_type = Identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(62, _stack[0]);
					break;
				}
				case 56: // prod_def_list = prod_def_start prod_exprs ";";
				{
					state = _stack.SetItems(3)
					    .Reduce(60, ProdDefList.Add((ProdDefList)_stack[0],(ProductionExprs)_stack[1]));
					break;
				}
				case 57: // prod_def_list = prod_def_start prod_def;
				{
					state = _stack.SetItems(2)
					    .Reduce(60, ProdDefList.Add((ProdDefList)_stack[0],(ProdDef)_stack[1]));
					break;
				}
				case 58: // prod_def_start = ;
				{
					state = _stack.Push(63, new ProdDefList());
					break;
				}
				case 59: // prod_def_start = prod_def_start prod_exprs "|";
				{
					state = _stack.SetItems(3)
					    .Reduce(63, ProdDefList.Add((ProdDefList)_stack[0],(ProductionExprs)_stack[1]));
					break;
				}
				case 60: // prod_def_start = prod_def_start prod_def "|";
				{
					state = _stack.SetItems(3)
					    .Reduce(63, ProdDefList.Add((ProdDefList)_stack[0],(ProdDef)_stack[1]));
					break;
				}
				case 61: // prod_def = prod_exprs prod_action optional_semicolon;
				{
					state = _stack.SetItems(3)
					    .Reduce(65, new ProdDef((ProductionExprs)_stack[0],(ActionExpr) _stack[1]));
					break;
				}
				case 62: // optional_semicolon = ;
				{
					state = _stack.Push(67, null);
					break;
				}
				case 63: // optional_semicolon = ";";
				{
					state = _stack.SetItems(1)
					    .Reduce(67, _stack[0]);
					break;
				}
				case 64: // prod_exprs = ;
				{
					state = _stack.Push(64, new ProductionExprs());
					break;
				}
				case 65: // prod_exprs = prod_exprs prod_expr;
				{
					state = _stack.SetItems(2)
					    .Reduce(64, ProductionExprs.Add((ProductionExprs)_stack[0],(ProductionExpr) _stack[1]));
					break;
				}
				case 66: // prod_expr = prod_expr_primary production_attr;
				{
					state = _stack.SetItems(2)
					    .Reduce(68, ProductionExpr.SetAttributes((ProductionExpr) _stack[0],(ProductionAttr) _stack[1]));
					break;
				}
				case 67: // prod_expr_primary = identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(69, new SymbolProd((Token)_stack[0]));
					break;
				}
				case 68: // prod_expr_primary = String;
				{
					state = _stack.SetItems(1)
					    .Reduce(69, AddStringTokenProd((StringConst) _stack[0]));
					break;
				}
				case 69: // prod_expr_primary = Char;
				{
					state = _stack.SetItems(1)
					    .Reduce(69, AddStringTokenProd((CharConst) _stack[0]));
					break;
				}
				case 70: // prod_action = "{" action_stmt "}";
				{
					state = _stack.SetItems(3)
					    .Reduce(66, _stack[1]);
					break;
				}
				case 71: // action_stmt = ;
				{
					state = _stack.Push(70, new ActionNullExpr());
					break;
				}
				case 72: // action_stmt = action_expr ";";
				{
					state = _stack.SetItems(2)
					    .Reduce(70, _stack[0]);
					break;
				}
				case 73: // action_expr = "new" type "(" action_args ")";
				{
					state = _stack.SetItems(5)
					    .Reduce(71, new ActionNewExpr((Identifier) _stack[1],(ActionArgs)_stack[3]));
					break;
				}
				case 74: // action_expr = Identifier "(" action_args ")";
				{
					state = _stack.SetItems(4)
					    .Reduce(71, new ActionFuncExpr((Identifier)_stack[0],(ActionArgs)_stack[2]));
					break;
				}
				case 75: // action_expr = action_primary_expr;
				{
					state = _stack.SetItems(1)
					    .Reduce(71, _stack[0]);
					break;
				}
				case 76: // action_args = ;
				{
					state = _stack.Push(73, new ActionArgs());
					break;
				}
				case 77: // action_args = action_expr;
				{
					state = _stack.SetItems(1)
					    .Reduce(73, new ActionArgs((ActionExpr) _stack[0]));
					break;
				}
				case 78: // action_args = action_args "," action_expr;
				{
					state = _stack.SetItems(3)
					    .Reduce(73, ActionArgs.Add((ActionArgs)_stack[0],(ActionExpr) _stack[2]));
					break;
				}
				case 79: // action_primary_expr = arg action_cast;
				{
					state = _stack.SetItems(2)
					    .Reduce(74, new ActionArg((Token)_stack[0],(Identifier) _stack[1]));
					break;
				}
				case 80: // action_primary_expr = Identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(74, new ActionMember((Identifier)_stack[0]));
					break;
				}
				case 81: // action_primary_expr = Integer;
				{
					state = _stack.SetItems(1)
					    .Reduce(74, new ActionIntConstant((DecInteger)_stack[0]));
					break;
				}
				case 82: // action_primary_expr = String;
				{
					state = _stack.SetItems(1)
					    .Reduce(74, new ActionStringConstant(_stack[0]));
					break;
				}
				case 83: // action_cast = ;
				{
					state = _stack.Push(75, null);
					break;
				}
				case 84: // action_cast = ":" Identifier;
				{
					state = _stack.SetItems(2)
					    .Reduce(75, _stack[1]);
					break;
				}
				case 85: // type = Identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(72, (Identifier) _stack[0]);
					break;
				}
				case 86: // type = Identifier "<" generic_args ">";
				{
					state = _stack.SetItems(4)
					    .Reduce(72, Identifier.MakeType((Identifier) _stack[0],(GenericArgs) _stack[2]));
					break;
				}
				case 87: // generic_args = type;
				{
					state = _stack.SetItems(1)
					    .Reduce(76, new GenericArgs((Identifier) _stack[0]));
					break;
				}
				case 88: // generic_args = generic_args "," type;
				{
					state = _stack.SetItems(3)
					    .Reduce(76, GenericArgs.Add((GenericArgs)_stack[0],(Identifier) _stack[2]));
					break;
				}
				case 89: // Identifier = Identifier "." identifier;
				{
					state = _stack.SetItems(3)
					    .Reduce(39, Identifier.Add((Identifier)_stack[0],(Token)_stack[2]));
					break;
				}
				case 90: // Identifier = identifier;
				{
					state = _stack.SetItems(1)
					    .Reduce(39, new Identifier((Token)_stack[0]));
					break;
				}
				case 91: // CharClass = char_class;
				{
					state = _stack.SetItems(1)
					    .Reduce(48, new CharClass((Token)_stack[0]));
					break;
				}
				case 92: // Char = char_string;
				{
					state = _stack.SetItems(1)
					    .Reduce(49, new EscChar((Token)_stack[0]));
					break;
				}
				case 93: // String = EscString;
				{
					state = _stack.SetItems(1)
					    .Reduce(42, _stack[0]);
					break;
				}
				case 94: // EscString = string;
				{
					state = _stack.SetItems(1)
					    .Reduce(77, new EscString((Token)_stack[0]));
					break;
				}
				case 95: // Integer = integer;
				{
					state = _stack.SetItems(1)
					    .Reduce(56, new DecInteger((Token)_stack[0]));
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
			{ -4, -1, -1, -1, -1, -1, -1, -4, -1, -4, -4, -1, -4, -1, -1, -1, -4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -4, -1, -1, -1, 1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -7, -1, -1, -1, -1, -1, -1, 6, -1, 5, -7, -1, -7, -1, -1, -1, -7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -7, -1, -1, -1, -1, -1, 3, -1, -1, -1, -1, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -9, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, -1, -9, -1, -1, -1, -9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -9, -1, -1, -1, -1, -1, -1, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -5, -1, -1, -1, -1, -1, -1, -5, -1, -5, -5, -1, -5, -1, -1, -1, -5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 13, -1, -1, -1, -14, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -14, -1, -1, -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -27, -1, -1, -1, -1, -1, -1, -1, -1, 20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 22, 23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -10, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -10, -1, -1, -1, -10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -11, -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -11, -1, -1, -1, -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -8, -1, -8, -1, -1, -1, -8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 27, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -6, -1, -1, -1, -1, -1, -1, -6, -1, -6, -6, -1, -6, -1, -1, -1, -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 29, -1, -1, -1, -1, -1, -1, -1, -1, -1, 28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 32, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 30, 31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -15, 24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 33, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -16, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -12, -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -12, -1, -1, -1, -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, 37, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 36 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 38, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 32, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -28, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 39, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -29, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -32, -1, -1, -1, -1, -1, 41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -17, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 47, 49, -1, 50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 42, 43, 44, 46, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -13, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -13, -1, -1, -1, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -95, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -95, -1, -1, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -96, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -96, -1, -1, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -49, -49, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 51, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -30, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 52, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 53, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -18, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 54, 55, -1, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -19, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -19, -19, -1, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -19, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -22, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -22, -22, -1, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -22, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 47, 49, -1, 50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 56, 44, 46, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -24, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -24, -24, -1, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -24, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -25, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -25, -25, -1, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -25, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -26, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -26, -26, -1, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -26, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -93, -93, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -93, -93, -1, -93, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -93, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -94, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -94, -94, -1, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -48, 58, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 57, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 62, 68, 70, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 66, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 65, -1, -1, -1, -1, -1, 64, 63, -1, -1, -1, 59, 60, 61, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 71, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 47, 49, -1, 50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 72, 44, 46, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 47, 49, -1, 50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 73, 44, 46, 48, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -23, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -23, -23, -1, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -50, -50, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -52, -1, -1, -1, -1, -1, 75, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 74, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 62, 68, 70, 67, -1, -1, -1, 76, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 77, -1, -1, -1, -1, -1, 66, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 65, -1, -1, -1, -1, -1, 64, 63, -1, -1, -1, -1, 78, 61, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69 },
			{ -1, -34, -34, -34, -34, -1, -1, -1, -34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -34, -1, -1, -1, -1, -1, -34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -37, -37, -37, -37, -1, -1, -1, -37, -1, -1, -1, -1, 79, -1, -1, -1, -1, -1, -37, 80, 81, 82, -1, -1, -37, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -43, -43, -43, -43, -1, -1, -1, -43, -1, -1, -1, -1, -43, -1, -1, -1, -1, -1, -43, -43, -43, -43, -1, -1, -43, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -44, -44, -44, -44, -1, -1, -1, -44, -1, -1, -1, -1, -44, -1, -1, -1, -1, -1, -44, -44, -44, -44, -1, -1, -44, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -45, -45, -45, -45, -1, -1, -1, -45, -1, -1, -1, -1, -45, -1, -1, -1, -1, -1, -45, -45, -45, -45, -1, -1, -45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -46, -46, -46, -46, -1, -1, -1, -46, -1, -1, -1, -1, -46, -1, -1, -1, -1, -1, -46, -46, -46, -46, -1, -1, -46, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 86, 92, 94, 91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 90, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 89, -1, -1, -1, -1, -1, 88, 87, -1, -1, -1, 83, 84, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 93 },
			{ -1, -94, -94, -94, -94, -1, -1, -1, -94, -1, -1, -1, -1, -94, -1, -1, -1, -1, -1, -94, -94, -94, -94, -1, -1, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -93, -93, -93, -93, -1, -1, -1, -93, -1, -1, -1, -1, -93, -1, -1, -1, -1, -1, -93, -93, -93, -93, -1, -1, -93, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -95, -95, -95, -95, -1, -1, -1, -95, -1, -1, -1, -1, -95, -1, -1, -1, -1, -1, -95, -95, -95, -95, -1, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -96, -96, -96, -96, -1, -1, -1, -96, -1, -1, -1, -1, -96, -1, -1, -1, -1, -1, -96, -96, -96, -96, -1, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -33, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -20, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -20, -20, -1, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -20, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -21, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -21, -21, -1, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -21, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 97, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 62, 68, 70, 67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 66, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 65, -1, -1, -1, -1, -1, 64, 63, -1, -1, -1, -1, 98, 61, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 69 },
			{ -1, -36, -36, -36, -36, -1, -1, -1, -36, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -36, -1, -1, -1, -1, -1, -36, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -38, -38, -38, -38, -1, -1, -1, -38, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -38, -1, -1, -1, -1, -1, -38, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -39, -39, -39, -39, -1, -1, -1, -39, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -39, -1, -1, -1, -1, -1, -39, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -40, -40, -40, -40, -1, -1, -1, -40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -40, -1, -1, -1, -1, -1, -40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, 100, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 99, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 86, 92, 94, 91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 102, -1, -1, -1, -1, -1, 90, 101, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 89, -1, -1, -1, -1, -1, 88, 87, -1, -1, -1, -1, 103, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 93 },
			{ -1, -34, -34, -34, -34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -34, -1, -1, -1, -1, -1, -34, -34, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -37, -37, -37, -37, -1, -1, -1, -1, -1, -1, -1, -1, 104, -1, -1, -1, -1, -1, -37, 105, 106, 107, -1, -1, -37, -37, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -43, -43, -43, -43, -1, -1, -1, -1, -1, -1, -1, -1, -43, -1, -1, -1, -1, -1, -43, -43, -43, -43, -1, -1, -43, -43, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -44, -44, -44, -44, -1, -1, -1, -1, -1, -1, -1, -1, -44, -1, -1, -1, -1, -1, -44, -44, -44, -44, -1, -1, -44, -44, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -45, -45, -45, -45, -1, -1, -1, -1, -1, -1, -1, -1, -45, -1, -1, -1, -1, -1, -45, -45, -45, -45, -1, -1, -45, -45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -46, -46, -46, -46, -1, -1, -1, -1, -1, -1, -1, -1, -46, -1, -1, -1, -1, -1, -46, -46, -46, -46, -1, -1, -46, -46, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 86, 92, 94, 91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 90, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 89, -1, -1, -1, -1, -1, 88, 87, -1, -1, -1, 108, 84, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 93 },
			{ -1, -94, -94, -94, -94, -1, -1, -1, -1, -1, -1, -1, -1, -94, -1, -1, -1, -1, -1, -94, -94, -94, -94, -1, -1, -94, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -93, -93, -93, -93, -1, -1, -1, -1, -1, -1, -1, -1, -93, -1, -1, -1, -1, -1, -93, -93, -93, -93, -1, -1, -93, -93, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -95, -95, -95, -95, -1, -1, -1, -1, -1, -1, -1, -1, -95, -1, -1, -1, -1, -1, -95, -95, -95, -95, -1, -1, -95, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -96, -96, -96, -96, -1, -1, -1, -1, -1, -1, -1, -1, -96, -1, -1, -1, -1, -1, -96, -96, -96, -96, -1, -1, -96, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -60, -60, -1, -60, -60, -1, -1, -1, -60, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -60, -1, -1, -60, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 109, -1, -1, 110, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -54, -1, -1, -1, -1, -1, -1, 113, -1, -1, -1, -1, 112, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 111, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -35, -35, -35, -35, -1, -1, -1, -35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -35, -1, -1, -1, -1, -1, -35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 114, 115, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -97, -97, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -47, -47, -47, -47, -1, -1, -1, -47, -1, -1, -1, -1, -47, -1, -1, -1, -1, -1, -47, -47, -47, -47, -1, -1, -47, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 86, 92, 94, 91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 90, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 89, -1, -1, -1, -1, -1, 88, 87, -1, -1, -1, -1, 116, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 93 },
			{ -1, -36, -36, -36, -36, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -36, -1, -1, -1, -1, -1, -36, -36, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -38, -38, -38, -38, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -38, -1, -1, -1, -1, -1, -38, -38, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -39, -39, -39, -39, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -39, -1, -1, -1, -1, -1, -39, -39, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -40, -40, -40, -40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -40, -1, -1, -1, -1, -1, -40, -40, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, 100, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 117, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 86, 92, 94, 91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 102, -1, -1, -1, -1, -1, 90, 118, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 89, -1, -1, -1, -1, -1, 88, 87, -1, -1, -1, -1, 103, 85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 93 },
			{ -51, -51, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -66, -1, -66, -66, -1, -1, -1, -66, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -66, -1, -1, -66, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 119, 120, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 121, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 122, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 125, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -56, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 124, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 123, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -41, -41, -41, -41, -1, -1, -1, -41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -41, -1, -1, -1, -1, -1, -41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, 100, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 126, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -35, -35, -35, -35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -35, -1, -1, -1, -1, -1, -35, -35, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 127, 128, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -47, -47, -47, -47, -1, -1, -1, -1, -1, -1, -1, -1, -47, -1, -1, -1, -1, -1, -47, -47, -47, -47, -1, -1, -47, -47, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 135, -1, 140, 139, -1, -1, -1, 129, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 130, -1, -1, 134, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 136, -1, -1, -1, -1, -1, -1, 137, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 132, -1, 131, 133, -1, -1, -1, -1, -1, -1, -1, 138 },
			{ -59, -59, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 141, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -53, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 142, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -57, -1, -1, -1, 143, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 144, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -41, -41, -41, -41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -41, -1, -1, -1, -1, -1, -41, -41, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, 100, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 145, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -58, -58, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -61, -61, -1, -61, -61, -1, -1, -1, -61, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -61, -1, -1, -61, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -67, -1, -67, -67, -1, -1, -1, -67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -67, -1, -1, -67, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -64, -64, -1, -1, -1, -1, -1, -1, 147, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -64, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 146, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -52, -1, -52, -52, -1, -1, -1, -52, -1, -1, -1, -1, -1, -1, -1, -1, 149, -1, -52, -1, -1, -52, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 148, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 155, -1, 70, -1, 159, 156, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -73, -1, -1, -1, -1, 152, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 153, -1, -1, 158, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 157, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 150, 151, -1, -1, 154, -1, -1, 69 },
			{ -69, -69, -1, -69, -69, -1, -1, -1, -69, -1, -1, -1, -1, -1, -1, -1, -1, -69, -1, -69, -1, -1, -69, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -70, -70, -1, -70, -70, -1, -1, -1, -70, -1, -1, -1, -1, -1, -1, -1, -1, -70, -1, -70, -1, -1, -70, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -71, -71, -1, -71, -71, -1, -1, -1, -71, -1, -1, -1, -1, -1, -1, -1, -1, -71, -1, -71, -1, -1, -71, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -95, -95, -1, -95, -95, -1, -1, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -95, -1, -95, -1, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -94, -94, -1, -94, -94, -1, -1, -1, -94, -1, -1, -1, -1, -1, -1, -1, -1, -94, -1, -94, -1, -1, -94, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -96, -96, -1, -96, -96, -1, -1, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -96, -1, -96, -1, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -62, -62, -1, -62, -62, -1, -1, -1, -62, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -62, -1, -1, -62, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -55, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 160, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -42, -42, -42, -42, -1, -1, -1, -42, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -42, -1, -1, -1, -1, -1, -42, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 161, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -63, -63, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -63, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -65, -65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -65, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -68, -1, -68, -68, -1, -1, -1, -68, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -68, -1, -1, -68, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 163, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 162, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 164, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, 165, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 168, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 167, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 166, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -82, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 169, -1, -1, -1, -1, 170, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -77, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -85, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 172, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 171, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -83, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -84, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -97, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -42, -42, -42, -42, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -42, -1, -1, -1, -1, -1, -42, -42, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -54, -1, -1, -1, -1, -1, -1, 113, -1, -1, -1, -1, 174, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 173, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -92, -92, -1, -92, -92, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -92, -1, -1, -92, -1, -1, -92, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -72, -72, -1, -1, -1, -1, -1, -1, -72, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -72, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -74, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 175, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 176, -1, -1, -1, -1, -1, -1, -1, -87, -1, -1, -1, -1, 177, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, 189, -1, 187, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -78, -1, -78, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 185, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 178, 182, -1, -1, 188 },
			{ -1, 190, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -81, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 191, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 192, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 193, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, 189, -1, 187, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -78, -1, -78, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 185, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 194, 182, -1, -1, 188 },
			{ -1, 198, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 197, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 196, -1, -1, -1, 195, -1 },
			{ -1, 199, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 200, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -79, -1, -79, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 168, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 167, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 202, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -82, 203, -82, -1, -1, -1, 204, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -77, -1, -77, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -92, -92, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -85, -1, -85, -1, -1, 206, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 205, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -83, -1, -83, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -84, -1, -84, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -97, -1, -97, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -95, -1, -95, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -96, -1, -96, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -86, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 18, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -53, -1, -53, -53, -1, -1, -1, -53, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -53, -1, -1, -53, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -91, -91, -1, -91, -91, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -91, -1, -1, -91, -1, -1, -91, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 207, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 208, -1, -1, -1, -1, -1, 209, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -89, -1, -1, -1, -1, -1, -89, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 210, -87, -1, -1, -1, -1, -1, -87, -1, -1, -1, -1, -1, 211, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -92, -92, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -92, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -76, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, 189, -1, 187, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 185, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 212, -1, -1, 182, -1, -1, 188 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 213, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, 189, -1, 187, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -78, -1, -78, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 185, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 214, 182, -1, -1, 188 },
			{ -1, 215, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -81, -1, -81, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 216, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -75, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -88, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 198, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 197, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 217, -1, -1, -1, -1, -1 },
			{ -1, 198, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 197, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 196, -1, -1, -1, 218, -1 },
			{ -1, 219, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -80, -1, -80, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, 183, -1, 189, -1, 187, 184, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -78, -1, -78, -1, 180, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 181, -1, -1, 186, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 185, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 179, -1, 220, 182, -1, -1, 188 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 221, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -91, -91, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -86, -1, -86, -1, -1, -1, 204, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -90, -1, -1, -1, -1, -1, -90, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 222, -1, -1, -1, -1, -1, 209, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -91, -91, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -91, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 201, -1, 223, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -76, -1, -76, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -88, -1, -1, -1, -1, -1, -88, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -75, -1, -75, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		};
	
		#endregion
		#region Symbols
		private const int _maxTerminal = 30;
		private readonly string[] _symbols =
		{
			"",
			"identifier",
			"char_class",
			"string",
			"char_string",
			"integer",
			"arg",
			"using",
			";",
			"namespace",
			"Options",
			"=",
			"Characters",
			"+",
			"-",
			"!",
			"Tokens",
			"<",
			">",
			"|",
			"*",
			"?",
			"{",
			"}",
			",",
			"(",
			")",
			"Productions",
			"new",
			":",
			".",
			"language",
			"using_section",
			"namespace_section",
			"option_section",
			"characters_section",
			"token_section",
			"production_section",
			"using_decl",
			"Identifier",
			"option_list",
			"option",
			"String",
			"character_expr",
			"character_pair",
			"char_class_concat",
			"char_class_unary",
			"char_class_primary",
			"CharClass",
			"Char",
			"token_list",
			"token",
			"token_attr",
			"token_expr",
			"token_quantifier",
			"token_primary",
			"Integer",
			"prod_list",
			"production",
			"production_attr",
			"prod_def_list",
			"func_opt",
			"func_opt_arg_type",
			"prod_def_start",
			"prod_exprs",
			"prod_def",
			"prod_action",
			"optional_semicolon",
			"prod_expr",
			"prod_expr_primary",
			"action_stmt",
			"action_expr",
			"type",
			"action_args",
			"action_primary_expr",
			"action_cast",
			"generic_args",
			"EscString",
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
	
				var maxClasses = 60 + 1;
				var maxStates = 96;
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
		0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xED, 0xCD, 0x47, 0x52, 0x02, 0x50, 
		0x14, 0x44, 0xD1, 0x42, 0x54, 0x50, 0xF8, 0xF2, 0x7D, 0x22, 0x49, 0x91, 0x20, 0x26, 0x14, 0x03, 
		0x19, 0xF7, 0xBF, 0x30, 0xAC, 0x12, 0x46, 0x2C, 0x80, 0xC9, 0x39, 0x83, 0x5B, 0x3D, 0xEB, 0x88, 
		0x9D, 0xEA, 0x6D, 0xC4, 0x4D, 0x1C, 0xB8, 0x7E, 0x3B, 0x8B, 0x8B, 0x88, 0xCA, 0x7C, 0xF1, 0xF5, 
		0x32, 0x7B, 0xFD, 0xAD, 0x9F, 0x94, 0xF7, 0x36, 0x9D, 0x8F, 0xC7, 0xC9, 0x77, 0xA4, 0xF4, 0x9C, 
		0x52, 0xCA, 0x3B, 0xC3, 0xD5, 0x5F, 0xDE, 0xFF, 0x77, 0xF1, 0xFC, 0xF2, 0x34, 0x47, 0xB7, 0x70, 
		0xBF, 0xBC, 0x2B, 0x34, 0x9E, 0x5A, 0x79, 0x9C, 0x7B, 0xCD, 0xFE, 0x43, 0x1E, 0xB5, 0x07, 0xA5, 
		0xAB, 0x75, 0x2D, 0xE7, 0x9F, 0xCF, 0xE9, 0xE1, 0x1F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0xB1, 0x05, 0x97, 0x6C, 0x0F, 0x55, 0x00, 
		0x00, 0x01, 0x00, 
	};
	
	private static readonly byte[] _compressedStates = 
	{
		0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xB5, 0x98, 0x07, 0x5B, 0xDB, 0x30, 
		0x10, 0x86, 0x4F, 0x20, 0x02, 0x66, 0xEF, 0x3D, 0x5A, 0x46, 0xCB, 0x6A, 0x19, 0x2D, 0x74, 0x0F, 
		0x3A, 0x68, 0x31, 0x90, 0x10, 0xA0, 0x85, 0x96, 0xBA, 0xFD, 0xFF, 0x3F, 0xA2, 0x3C, 0xC8, 0x16, 
		0x56, 0x1C, 0xB0, 0x25, 0xFB, 0xBE, 0xE4, 0x7B, 0xEC, 0x93, 0x64, 0xFB, 0xF5, 0xE9, 0x2E, 0x67, 
		0x27, 0x11, 0x91, 0x68, 0x69, 0x25, 0x49, 0x6D, 0xA5, 0x76, 0xEA, 0xF0, 0x84, 0x92, 0x47, 0x5E, 
		0xA7, 0x27, 0xBA, 0x84, 0xE8, 0x16, 0x5A, 0x3D, 0xBD, 0x42, 0xF4, 0xF5, 0x0F, 0x0C, 0x8A, 0xA1, 
		0xE1, 0x91, 0xD1, 0xB1, 0xF1, 0x89, 0xC9, 0xA9, 0xE9, 0x19, 0x31, 0x2B, 0xE6, 0x1E, 0x90, 0x20, 
		0x21, 0x48, 0x29, 0xB6, 0xBA, 0xD5, 0x9D, 0x5B, 0x38, 0xEA, 0x98, 0x4B, 0x28, 0x3E, 0xAF, 0x6C, 
		0x1B, 0x41, 0x0A, 0x02, 0xB5, 0xFD, 0x0D, 0x82, 0xE0, 0x5F, 0xD8, 0x09, 0x45, 0x71, 0xC7, 0x21, 
		0x05, 0x4B, 0xA5, 0xAB, 0x3F, 0x32, 0x21, 0x95, 0x02, 0x99, 0x47, 0x96, 0x98, 0x2F, 0x72, 0xC5, 
		0x5C, 0x8A, 0x86, 0x25, 0x5E, 0xD0, 0x3F, 0x22, 0xFB, 0x93, 0xC5, 0xFA, 0x4A, 0x74, 0xE8, 0x27, 
		0x44, 0x3E, 0xF9, 0x79, 0x44, 0xD7, 0xC9, 0xFB, 0x78, 0xDA, 0x78, 0xBA, 0xC3, 0xD4, 0x41, 0xCE, 
		0xEB, 0x6C, 0x15, 0xF6, 0x41, 0x7C, 0xB4, 0x67, 0xBB, 0x03, 0x98, 0xA0, 0xAB, 0xB6, 0x5F, 0x59, 
		0x3D, 0x77, 0x35, 0xD1, 0xF3, 0x6D, 0xCC, 0x9B, 0xE9, 0x9E, 0x7B, 0x10, 0xCF, 0xBD, 0x08, 0xDC, 
		0x87, 0xC0, 0xCE, 0x98, 0xD7, 0x2C, 0xD9, 0x1E, 0x40, 0x3C, 0x0F, 0x22, 0xF0, 0x10, 0x02, 0x0F, 
		0x23, 0xF0, 0x08, 0x02, 0x8F, 0x22, 0xF0, 0x18, 0x02, 0x8F, 0x23, 0xF0, 0x04, 0x02, 0x4F, 0x22, 
		0x70, 0xAE, 0x47, 0xF2, 0x61, 0x46, 0x79, 0xCE, 0x20, 0x9E, 0x67, 0x11, 0xD8, 0x39, 0xED, 0x79, 
		0xCB, 0x53, 0xC5, 0xFC, 0x72, 0x5F, 0xA8, 0x83, 0x17, 0x39, 0xBF, 0x0C, 0xAC, 0x9E, 0x97, 0x10, 
		0x38, 0xD2, 0x23, 0x06, 0xFC, 0x18, 0x9D, 0x76, 0xA8, 0x65, 0x06, 0xBC, 0x82, 0x25, 0x6C, 0xD5, 
		0x0D, 0x4F, 0x21, 0x3F, 0xE2, 0x0A, 0x14, 0xC9, 0x7A, 0x21, 0xF8, 0x09, 0xD7, 0xF3, 0x53, 0x6C, 
		0xDA, 0x1B, 0x6E, 0xB8, 0xBF, 0x79, 0x09, 0xDB, 0x6A, 0x40, 0xB6, 0xB7, 0x59, 0xF0, 0x33, 0xC0, 
		0xF3, 0x73, 0xB8, 0x48, 0x76, 0x78, 0xF0, 0x6E, 0x43, 0xCA, 0xF3, 0x45, 0x61, 0xF8, 0xA5, 0xFB, 
		0x73, 0xEE, 0x6E, 0xEE, 0x53, 0xF5, 0x9A, 0x01, 0xBF, 0x69, 0x84, 0xE7, 0xB7, 0x0C, 0xF8, 0x1D, 
		0x16, 0xF3, 0x7B, 0x37, 0xDC, 0xD9, 0xDC, 0x6C, 0x67, 0xC0, 0xDF, 0x73, 0xC0, 0x7B, 0x5C, 0xCF, 
		0x9F, 0xF8, 0xD3, 0xFE, 0x0C, 0xC7, 0xFC, 0x05, 0x80, 0xBF, 0x02, 0x9E, 0xF7, 0x81, 0x69, 0x7F, 
		0x73, 0xC5, 0xEC, 0x21, 0x45, 0x32, 0x8D, 0xC0, 0xFF, 0xE9, 0x20, 0x45, 0xA9, 0x07, 0xEF, 0x5F, 
		0x95, 0xD4, 0x09, 0x15, 0x96, 0xFA, 0xFB, 0xAE, 0xF6, 0xA3, 0xB0, 0xA7, 0xB6, 0xE3, 0xF8, 0xA8, 
		0xB1, 0x7E, 0x3C, 0x48, 0xD3, 0xE9, 0xA9, 0x6E, 0x8C, 0x49, 0x9C, 0x31, 0xE3, 0xFA, 0x13, 0xB5, 
		0x71, 0xB9, 0xAC, 0x1B, 0x63, 0x8C, 0xCA, 0x65, 0x33, 0xAE, 0x3F, 0x51, 0x1B, 0x57, 0x2A, 0xBA, 
		0x09, 0x63, 0xAE, 0x64, 0xCE, 0x2F, 0x43, 0xD5, 0xAA, 0x6E, 0x42, 0xB8, 0x5A, 0x0C, 0x95, 0x45, 
		0x7D, 0xD5, 0xCB, 0x37, 0x89, 0x3D, 0xB1, 0xA4, 0x35, 0x43, 0x67, 0x67, 0xBA, 0x31, 0xA6, 0x88, 
		0xCE, 0xCF, 0x75, 0x63, 0x4C, 0x11, 0xD5, 0xA6, 0x6D, 0xAB, 0x86, 0x54, 0x31, 0x57, 0xB2, 0x0C, 
		0xAC, 0x17, 0xB3, 0x38, 0x4B, 0x5A, 0x99, 0x2F, 0x83, 0x4B, 0xE0, 0xA5, 0xFF, 0x0B, 0xF8, 0xC6, 
		0xF8, 0x9D, 0xEB, 0x65, 0xD0, 0x8E, 0xBC, 0x49, 0x5A, 0x19, 0x79, 0x4A, 0x48, 0x4A, 0xBD, 0x47, 
		0x75, 0x6E, 0x4A, 0x5D, 0x1A, 0x2B, 0x6D, 0x0F, 0x40, 0xB8, 0x72, 0xCA, 0x5E, 0x71, 0x4D, 0x74, 
		0xD4, 0x6E, 0x6E, 0x69, 0x6C, 0xF2, 0xE8, 0x1D, 0xB5, 0x20, 0x11, 0xDF, 0x00, 0x61, 0x0B, 0x7D, 
		0x21, 0xE0, 0x16, 0x00, 0x00, 
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
		public const int identifier = 1;
		public const int char_class = 2;
		public const int String = 3;
		public const int char_string = 4;
		public const int integer = 5;
		public const int arg = 6;
		public const int AtSignusing = 7;
		public const int Semicolon = 8;
		public const int AtSignnamespace = 9;
		public const int AtSignOptions = 10;
		public const int Equal = 11;
		public const int AtSignCharacters = 12;
		public const int Plus = 13;
		public const int Minus = 14;
		public const int Exclamation = 15;
		public const int AtSignTokens = 16;
		public const int LessThan = 17;
		public const int GreaterThan = 18;
		public const int VerticalBar = 19;
		public const int Asterisk = 20;
		public const int QuestionMark = 21;
		public const int LeftCurly = 22;
		public const int RightCurly = 23;
		public const int Comma = 24;
		public const int LeftParen = 25;
		public const int RightParen = 26;
		public const int AtSignProductions = 27;
		public const int AtSignnew = 28;
		public const int Colon = 29;
		public const int Period = 30;
		public const int Space = -2;
		public const int Comment = -3;
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
