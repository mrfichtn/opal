namespace Opal
{
    public class LogItem: Segment
    {
        public LogItem(LogLevel level, 
            string message,
            Segment segment,
            string line,
            string? suggestions = null)
            : base(segment)
        {
            Level = level;
            Message = message;
            Line = line;
            Suggestions = suggestions;
        }

        public readonly LogLevel Level;
        public readonly string Message;
        public readonly string Line;
        public readonly string? Suggestions;
    }
}
