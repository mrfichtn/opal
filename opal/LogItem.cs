namespace Opal
{
    public class LogItem
    {
        public LogItem(LogLevel level, 
            string message,
            Token token,
            string line)
        {
            Level = level;
            Message = message;
            Token = token;
            Line = line;
        }

        public readonly LogLevel Level;
        public readonly string Message;
        public readonly Token Token;
        public readonly string Line;
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info
    }
}
