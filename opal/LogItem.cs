namespace Opal
{
    public class LogItem
    {
        public LogItem(LogLevel level, 
            string message,
            Token token,
            string line,
            string? suggestions = null)
        {
            Level = level;
            Message = message;
            Token = token;
            Line = line;
            Suggestions = suggestions;
        }

        public readonly LogLevel Level;
        public readonly string Message;
        public readonly Token Token;
        public readonly string Line;
        public readonly string? Suggestions;
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info
    }
}
