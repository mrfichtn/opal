namespace Opal.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Logs the command line for an underlying tool, executable file, or shell command of a task.
        /// </summary>
        /// <param name="commandLine">The command line string.</param>
        void LogCommandLine(string commandLine);


        /// <summary>
        /// Logs the command line for an underlying tool, executable file, or shell command
        /// of a task using the specified importance level.
        /// </summary>
        /// <param name="importance">One of the values of Microsoft.Build.Framework.MessageImportance that indicates the importance level of the command line.</param>
        /// <param name="commandLine">The command line string.</param>
        void LogCommandLine(Importance importance, string commandLine);

        /// <summary>
        /// Logs an error with the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogError(string message, params object[] messageArgs);


        /// <summary>
        /// Logs an error using the specified message and other error details.
        /// </summary>
        /// <param name="subcategory">The description of the error type.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="helpKeyword">The Help keyword to associate with the error.</param>
        /// <param name="lineNumber">The line in the file where the error occurs.</param>
        /// <param name="columnNumber">The column in the file where the error occurs.</param>
        /// <param name="endLineNumber">The end line in the file where the error occurs.</param>
        /// <param name="endColumnNumber">The end column in the file where the error occurs.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogError(string subcategory, string errorCode, string helpKeyword, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs);

        /// <summary>
        /// Logs a message with the specified string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The arguments for formatting the message.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogMessage(string message, params object[] messageArgs);

        /// <summary>
        /// Logs a message with the specified string and importance.
        /// </summary>
        /// <param name="importance">One of the enumeration values that specifies the importance of the message.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The arguments for formatting the message.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogMessage(Importance importance, string message, params object[] messageArgs);

        /// <summary>
        /// Logs a warning using the specified message and other warning details.
        /// </summary>
        /// <param name="subcategory">The description of the error type.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="helpKeyword">The Help keyword to associate with the error.</param>
        /// <param name="lineNumber">The line in the file where the error occurs.</param>
        /// <param name="columnNumber">The column in the file where the error occurs.</param>
        /// <param name="endLineNumber">The end line in the file where the error occurs.</param>
        /// <param name="endColumnNumber">The end column in the file where the error occurs.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogWarning(string subcategory, string warningCode, string helpKeyword, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs);

        /// <summary>
        /// Logs an warning with the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
        /// <exception cref="System.ArgumentNullException">message is null</exception>
        void LogWarning(string message, params object[] messageArgs);
    }
}
