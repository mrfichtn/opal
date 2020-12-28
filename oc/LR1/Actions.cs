using Generators;

namespace Opal.LR1
{
	// If >= 0, represents new state after shift. If < 0, represents one's complement 
	// (i.e. ~x) of reduction rule.
	public class Actions
	{
		///*state*/, /*lookahead*/
		private readonly int[,] _data;
		private readonly Symbols _symbols;

		public Actions(int[,] data, Symbols symbols)
		{
			_data = data;
			_symbols = symbols;
		}

		public ActionType GetAction(uint state, uint token, out uint arg)
		{
			var action = _data[state, token];
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
				action -= 2;
			}
			arg = (uint)action;
			return actionType;
		}

		public void Write(IGenerator generator)
		{
			generator.Indent(1);
			generator.WriteLine("private static readonly int[,] _actions = ");
			generator.StartBlock();

			var stateCount = _data.GetLength(0);
			var symbolCount = _data.GetLength(1);

            for (int j = 0; j < stateCount; j++)
			{
				generator.Write("{ ");
				for (var state = 0; state < symbolCount;)
				{
					generator.Write(_data[j, state].ToString());
					if (++state < symbolCount)
						generator.Write(", ");
				}
				generator.WriteLine(" },");
			}

			generator.EndBlock(";");
			generator.UnIndent(1);
		}
	}

	public enum ActionType
	{
		Shift,
		Reduce,
		Error
	}
}
