using System;

namespace Opal
{
    public abstract class ScannerBase: IDisposable
    {
		protected readonly IBuffer buffer;

		protected ScannerBase(IBuffer buffer)
        {
			this.buffer = buffer;
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

		public abstract Token RawNextToken();

		public string Line(Position position) =>
			buffer.Line(position);


	}
}
