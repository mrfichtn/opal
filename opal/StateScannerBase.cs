using System;
using System.Text;

namespace Opal
{
    public class StateScannerBase : ScannerBase
	{
		private readonly int[] classes;
		private readonly int[,] states;

		private int ch;
		private int line;
		private int column;

		private string prevLine;
		private readonly StringBuilder curLine;

		public StateScannerBase(int[] classes,
			int[,] states,
			IBuffer buffer, 
			int line = 1, 
			int column = 0)
			: base(buffer)
		{
			this.classes = classes;
			this.states = states;
			this.line = line;
			this.column = column;
			prevLine = string.Empty;
			curLine = new StringBuilder();
			NextChar();
		}

		public override Token RawNextToken()
		{
			if (ch == Buffer.Eof)
				return new Token(line, column, buffer.Position);

			var startPosition = new Position(line, column, buffer.Position - 1);
			var lastLine = line;
			var lastColumn = column;
			var lastAcceptingPosition = buffer.Position;
			var lastAcceptingState = -1;
			var stateIndex = 0;


			while (true)
			{
				var nextState = GetNextState(stateIndex);
				if (nextState <= 0)
				{
					var pos = buffer.Position;
					var value = buffer.GetToken(lastAcceptingPosition - startPosition.Ch);

					var token = new Token(startPosition,
						new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
						lastAcceptingState,
						value);

					if (pos != lastAcceptingPosition + 1)
					{
						line = lastLine;
						column = lastColumn;
						NextChar();
					}
					else
                    {
						buffer.Read();
                    }
					return token;
				}
				else
				{
					var acceptingState = states[nextState, 0];
					if (acceptingState != 0)
					{
						lastAcceptingState = acceptingState;
						lastLine = line;
						lastColumn = column;
						lastAcceptingPosition = buffer.Position;
					}
				}
				stateIndex = nextState;
				NextChar();
			}
		}

		public int GetNextState(int stateIndex)
		{
			int nextState;
			if (ch >= 0)
			{
				var cls = classes[ch];
				nextState = states[stateIndex, cls];
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

		public string Line(int position)
		{
			if (position + 1 == line)
				return prevLine;
			if (position == line)
				return curLine.ToString() + buffer.PeekLine();
			return string.Empty;
		}
	}

}
