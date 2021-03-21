using System;
using System.Text;

namespace Opal
{
    public abstract class ParserBase: IDisposable
    {
		private readonly ScannerBuffer scanner;
		protected readonly Logger logger;
		private readonly int maxTerminal;
		private readonly string[] symbols;
		private readonly int[,] actions;
		private LRStack stack;
		protected int items;

		protected ParserBase(ScannerBase scanner,
			int maxTerminal,
			string[] symbols,
			int[,] actions)
        {
			this.maxTerminal = maxTerminal;
			this.symbols = symbols;
			this.actions = actions;

			logger = new Logger(scanner);
			this.scanner = new ScannerBuffer(scanner, logger);
			stack = LRStack.Root;

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
			var token = scanner.NextToken();
			var errorState = 0U;
			var suppressError = 0;

			while (true)
			{
				if (suppressError > 0) 
					--suppressError;

				var state = stack.State;
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
							errorState = state;
						}
						suppressError = 2;
						break;

					case ActionType.Reduce:
						var rule = result;
						var reducedState = Reduce(rule);
						if (rule == 0)
						{
							Root = stack.Value;
							return !logger.HasErrors;
						}
						if (GetAction(reducedState, stack.State, out result) == ActionType.Error)
							goto case ActionType.Error;
						stack = stack.Replace(result);
						break;

					case ActionType.Shift:
						stack = stack.Shift(result, token);
						token = scanner.NextToken();
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
				var nextToken = scanner.PeekToken();
				if (nextToken.State >= 0)
				{
					stack.GetState(0, out var newState);
					if (actions[newState, nextToken.State] != -1)
					{
						token = scanner.NextToken();
						return true;
					}
				}
			}

			while (true)
			{
				for (var i = 0; stack.GetState(i, out var newState); i++)
				{
					if (actions[newState, token.State] != -1)
					{
						stack = stack.Pop(i);
						return true;
					}
				}
				if (token.State == 0)
					break;
				token = scanner.NextToken();
			}
			return isOk;
		}

		protected T? At<T>(int index) => (T)At(index);

		protected object? At(int index) => stack[items - index - 1].Value;

		protected uint Reduce(uint state, object? value)
		{
			var oldStack = stack[items];
			var newState = oldStack.State;
			stack = new LRStack(state, value, oldStack);
			return newState;
		}

		protected uint Push(uint state, object? value)
		{
			var oldState = stack.State;
			stack = new LRStack(state, value, stack);
			return oldState;
		}
	}
}
