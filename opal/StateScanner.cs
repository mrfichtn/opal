using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Opal
{
    public abstract class StateScanner : IDisposable
	{
		private readonly int[] _charToClass;
		private readonly int[,] _states;

		private readonly IBuffer buffer;
		private int _ch;
		private int _line;
		private int _column;

		public StateScanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column)
		{
		}

		public StateScanner(IBuffer buffer, int line = 1, int column = 0)
		{
			this.buffer = buffer;
			_line = line;
			_column = column;
			_charToClass = CharClasses;
			_states = States;
			prevLine = string.Empty;

			NextChar();
		}

		protected abstract int[] CharClasses { get; }
		protected abstract int[,] States { get; }

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
			if (_ch == Buffer.Eof)
				return new Token(_line, _column, buffer.Position);

			var startPosition = new Position(_line, _column, buffer.Position - 1);
			var lastLine = _line;
			var lastColumn = _column;
			var lastAcceptingPosition = -1;
			var lastAcceptingState = -1;
			var stateIndex = 0;

			while (true)
			{
				var nextState = GetNextState(stateIndex);
				if (nextState <= 0)
				{
					if (lastAcceptingState == -1)
						lastAcceptingPosition = buffer.Position - 1;

					var value = buffer.GetString(startPosition.Ch,
						lastAcceptingPosition);

					var token = new Token(startPosition,
						new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
						lastAcceptingState,
						value);

					if (buffer.Position > lastAcceptingPosition + 1)
					{
						buffer.Position = lastAcceptingPosition;
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
						lastLine = _line;
						lastColumn = _column;
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
			_ch = buffer.Read();
			if (_ch == '\n')
			{
				++_line;
				_column = 0;

				prevLine = curLine.ToString();
				curLine.Clear();
			}
			else if (_ch == '\r')
			{
				++_column;
			}
			else
			{
				++_column;
				curLine.Append((char)_ch);
			}
		}

		private string prevLine;
		private StringBuilder curLine = new StringBuilder();

		public string Line(int position)
		{
			if (position + 1 == _line)
				return prevLine;
			if (position == _line)
				return curLine.ToString() + buffer.PeekLine();
			return string.Empty;
		}

		protected static void Decompress(byte[] data, Stream outStream)
		{
			using (var inStream = new MemoryStream(data))
			{
				var s = new GZipStream(inStream, CompressionMode.Decompress, false);
				s.CopyTo(outStream);
			}
		}

		#region Read Methods

		public static int Read8(Stream stream) => (sbyte)stream.ReadByte();

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

}
