using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Opal
{
    public class Logger: IEnumerable<LogItem>
    {
		private readonly ScannerBase scanner;
		private ImmutableQueue<LogItem> log;
        private bool hasErrors;

		public Logger(ScannerBase scanner)
        {
			this.scanner = scanner;
			log = ImmutableQueue<LogItem>.Empty;
		}

		public bool HasErrors => hasErrors;

		public IEnumerator<LogItem> GetEnumerator() =>
			(log as IEnumerable<LogItem>).GetEnumerator();

        public void Log(LogLevel level,
			string message,
			Segment segment,
			string? suggestions = null)
		{
			if (level == LogLevel.Error)
				hasErrors = true;
			var logItem = new LogItem(LogLevel.Error,
				message,
				segment,
				scanner.Line(segment.Start),
				suggestions);
			log = log.Enqueue(logItem);
		}

		public void LogError(string message, Segment segment, string? suggestions = null) =>
			Log(LogLevel.Error, message, segment, suggestions);

		public void LogWarning(string message, Segment segment, string? suggestions = null) =>
			Log(LogLevel.Warning, message, segment, suggestions);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
