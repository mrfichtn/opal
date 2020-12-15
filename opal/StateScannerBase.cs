using System;
using System.Text;

namespace Opal
{
    public class StateScannerBase : IDisposable
	{
		private readonly int[] classes;
		private readonly int[,] states;

		private readonly IBuffer buffer;
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
		{
			this.classes = classes;
			this.states = states;
			this.buffer = buffer;
			this.line = line;
			this.column = column;
			prevLine = string.Empty;
			curLine = new StringBuilder();
			NextChar();
		}

		public void Dispose()
		{
			buffer.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>Skips ingore-tokens</summary>
		public Token NextToken()
		{
			Token token;
			do
			{
				token = RawNextToken();
			} while (token.State < -1);

			return token;
		}

		/// <summary>Returns next token</summary>
		public Token RawNextToken()
		{
			if (ch == Buffer.Eof)
				return new Token(line, column, buffer.Position);

			var startPosition = new Position(line, column, buffer.Position - 1);
			var lastLine = line;
			var lastColumn = column;
			var lastAcceptingPosition = -1;
			var lastAcceptingState = -1;
			var stateIndex = 0;

			while (true)
			{
				var nextState = GetNextState(stateIndex);
				if (nextState <= 0)
				{
					if (lastAcceptingPosition == -1)
						lastAcceptingPosition = buffer.Position;

					var value = buffer.GetString(startPosition.Ch,
						lastAcceptingPosition);

					var token = new Token(startPosition,
						new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
						lastAcceptingState,
						value);

					if (buffer.Position > lastAcceptingPosition + 1)
					{
						buffer.Position = lastAcceptingPosition;
						line = lastLine;
						column = lastColumn;
						NextChar();
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
