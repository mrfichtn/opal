namespace Opal.Logging
{
    public static class ILoggerExt
    {
        public static void LogMessage(this ILogger logger, string message, params object[] messageArgs)
        {
            logger.LogMessage(Importance.Normal, 
                message, 
                messageArgs);
        }
        
        /// <summary>
        /// Logs an error using the specified message and other error details.
        /// </summary>
        /// <param name="file">The path to the file containing the error.</param>
        /// <param name="lineNumber">The line in the file where the error occurs.</param>
        /// <param name="columnNumber">The column in the file where the error occurs.</param>
        /// <param name="endLineNumber">The end line in the file where the error occurs.</param>
        /// <param name="endColumnNumber">The end column in the file where the error occurs.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        public static void LogError(this ILogger logger, 
            int lineNumber, 
            int columnNumber, 
            int endLineNumber, 
            int endColumnNumber, 
            string message, 
            params object[] messageArgs)
        {
            logger.LogError(null, 
                null, 
                null, 
                lineNumber, 
                columnNumber, 
                endLineNumber, 
                endColumnNumber, 
                message, 
                messageArgs);
        }

        /// <summary>
        /// Logs an error using the specified message and other error details.
        /// </summary>
        /// <param name="file">The path to the file containing the error.</param>
        /// <param name="segment">Identifies location of error</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        public static void LogError(this ILogger logger, 
            Segment segment, 
            string message, 
            params object[] messageArgs)
        {
            logger.LogError(segment.Start.Ln, segment.Start.Col, segment.End.Ln, segment.End.Col, message, messageArgs);
        }


        /// <summary>
        /// Logs a warning using the specified message and other warning details.
        /// </summary>
        /// <param name="lineNumber">The line in the file where the error occurs.</param>
        /// <param name="columnNumber">The column in the file where the error occurs.</param>
        /// <param name="endLineNumber">The end line in the file where the error occurs.</param>
        /// <param name="endColumnNumber">The end column in the file where the error occurs.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        public static void LogWarning(this ILogger logger, 
            int lineNumber, 
            int columnNumber, 
            int endLineNumber, 
            int endColumnNumber, 
            string message, 
            params object[] messageArgs)
        {
            logger.LogWarning(null, 
                null, 
                null, 
                lineNumber, 
                columnNumber, 
                endLineNumber, 
                endColumnNumber, 
                message, 
                messageArgs);
        }

        /// <summary>
        /// Logs an error using the specified message and other error details.
        /// </summary>
        /// <param name="segment">Identifies location of error</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        public static void LogWarning(this ILogger logger, 
            Segment segment, 
            string message, 
            params object[] messageArgs)
        {
            logger.LogWarning(segment.Start.Ln, 
                segment.Start.Col, 
                segment.End.Ln, 
                segment.End.Col, 
                message, 
                messageArgs);
        }
    }
}
