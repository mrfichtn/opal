namespace Opal.Logging
{
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
}
