using System;

namespace Opal
{
    /// <summary>
    /// Buffer between text/file object and scanner
    /// </summary>
    public interface IBuffer : IDisposable
	{
		long Length { get; }

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

		string PeekLine();

		/// <summary>
		/// Returns string from beg to end
		/// </summary>
		/// <param name="beg"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		string GetString(int beg, int end);
	}
}
