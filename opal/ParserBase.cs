using System;
using System.Collections.Immutable;
using System.Text;

namespace Opal
{
    public abstract class ParserBase: IDisposable
    {
        protected readonly ScannerBase scanner;
		protected readonly Logger logger;
		private ImmutableQueue<Token> peekToken;
		private readonly int maxTerminal;
		private readonly string[] symbols;
		private readonly int[,] actions;
		private LRStack _stack;
		protected int items;

		protected ParserBase(ScannerBase scanner,
			int maxTerminal,
			string[] symbols,
			int[,] actions)
        {
			this.maxTerminal = maxTerminal;
			this.symbols = symbols;
			this.actions = actions;

			this.scanner = scanner;
			logger = new Logger(scanner);
			peekToken = ImmutableQueue<Token>.Empty;
			_stack = LRStack.Root;

			Init();
		}

		public void Dispose()
        {
            scanner.Dispose();
            GC.SuppressFinalize(this);
        }

		public virtual void Init()
		{ }

		public object? Root { get; private set; }

		public Logger Logger => logger;

		public bool Parse()
		{
			var token = NextToken();
			var errorState = 0U;
			var suppressError = 0;

			while (true)
			{
				if (suppressError > 0) --suppressError;

				var state = _stack.State;
				var actionType = GetAction(state, (uint)token.State, out var result);

				switch (actionType)
				{
					case ActionType.Error:
						if (!TryRecover(state, ref token, (suppressError > 0)))
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
							Root = _stack.Value;
							return !logger.HasErrors;
						}
						if (GetAction(reducedState, _stack.State, out result) == ActionType.Error)
							goto case ActionType.Error;
						_stack = _stack.Replace(result);
						break;

					case ActionType.Shift:
						_stack = _stack.Shift(result, token);
						token = NextToken();
						break;
				}
			}
		}

		protected abstract uint Reduce(uint rule);

		private enum ActionType { Shift, Reduce, Error }

		private ActionType GetAction(uint state, uint token, out uint arg)
		{
			var action = actions[state, token];
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

		private Token NextToken()
		{
			while (!peekToken.IsEmpty)
			{
				peekToken = peekToken.Dequeue(out var token);
				if (token.State >= 0)
					return token;
				SyntaxError(token);
			}

			while (true)
			{
				var token = scanner.NextToken();
				if (token.State >= 0)
					return token;
				SyntaxError(token);
			}
		}

		private Token PeekToken()
		{
			while (true)
			{
				var token = scanner.NextToken();
				peekToken = peekToken.Enqueue(token);
				if (token.State >= 0)
					return token;
			}
		}

		private void SyntaxError(Token token) =>
			logger.LogError("Syntax error", token);


		private bool TryRecover(uint state, ref Token token, bool suppress)
		{
			var isOk = false;
			var builder = new StringBuilder("  Expecting one of the following token type(s):");
			var count = 0;
			for (var i = 0; i < maxTerminal; i++)
			{
				if (actions[state, i] != -1)
				{
					builder.Append("\n      ").Append(symbols[i]);
					count++;
				}
			}

			if (!suppress)
			{
				logger.LogError(
					(token.State != 0) ? $"Unexpected token" : $"Unexpected token [EOF]",
					token,
					count > 0 ? builder.ToString() : null);
			}

			if (token.State != 0)
			{
				var nextToken = PeekToken();
				if (nextToken.State >= 0)
				{
					_stack.GetState(0, out var newState);
					if (actions[newState, nextToken.State] != -1)
					{
						token = NextToken();
						return true;
					}
				}
			}

			while (true)
			{
				for (var i = 0; _stack.GetState(i, out var newState); i++)
				{
					if (actions[newState, token.State] != -1)
					{
						_stack = _stack.Pop(i);
						return true;
					}
				}
				if (token.State == 0)
					break;
				token = NextToken();
			}
			return isOk;
		}

		protected T? At<T>(int index) => (T)At(index);

		protected object? At(int index) => _stack[items - index - 1].Value;

		protected uint Reduce(uint state, object? value)
		{
			var oldStack = _stack[items];
			var newState = oldStack.State;
			_stack = new LRStack(state, value, oldStack);
			return newState;
		}

		protected uint Push(uint state, object? value)
		{
			var oldState = _stack.State;
			_stack = new LRStack(state, value, _stack);
			return oldState;
		}
	}
}
