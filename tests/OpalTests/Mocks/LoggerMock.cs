using Opal;
using System.Text;

namespace OpalTests.Mocks
{
    public class LoggerMock : ILogger
    {
        private readonly StringBuilder _builder;

        public LoggerMock()
        {
            _builder = new StringBuilder();
        }

        public void LogError(string message, params object[] messageArgs)
        {
            _builder.AppendFormat(message, messageArgs).AppendLine();
        }

        public void LogError(Segment segment, string message, params object[] messageArgs)
        {
            Log(segment, message, messageArgs);
        }

        public void LogMessage(Importance importance, string message, params object[] messageArgs)
        {
            _builder.AppendFormat(message, messageArgs).AppendLine();
        }

        public void LogMessage(Importance importance, Segment segment, string message, params object[] messageArgs)
        {
            Log(segment, message, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            _builder.AppendFormat(message, messageArgs).AppendLine();
        }

        public void LogWarning(Segment segment, string message, params object[] messageArgs)
        {
            Log(segment, message, messageArgs);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        private void Log(Segment segment, string message, params object[] messageArgs)
        {
            if (segment.Start.Ln > 0)
            {
                _builder.Append('(').Append(segment.Start.Ln);
                if (segment.Start.Col > 0)
                {
                    _builder.Append(',').Append(segment.Start.Col);
                    if (segment.End.Ln > 0)
                        _builder.Append(',').Append(segment.End.Ln).Append(',').Append(segment.End.Col);
                }
                _builder.Append(')');
            }
            if (_builder.Length > 0)
                _builder.Append(":\t");
            if (messageArgs == null || messageArgs.Length == 0)
                _builder.Append(message);
            else
                _builder.AppendFormat(message, messageArgs);
            _builder.AppendLine();
        }

    }
}
