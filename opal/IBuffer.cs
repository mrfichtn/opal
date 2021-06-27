using System;

namespace Opal
{
    /// <summary>
    /// Buffer between text/file object and scanner
    /// </summary>
    public interface IBuffer : IDisposable
	{
		/// <summary>
		/// Current buffer position
		/// </summary>
		int Position { get; }

		/// <summary>
		/// Returns next character, moves the position one forward
		/// </summary>
		int Read();

		string PeekLine();

		/// <summary>
		/// Returns string from beg to end
		/// </summary>
		/// <param name="beg"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		string GetToken(int end);

		string Line(Position position);
	}
}
