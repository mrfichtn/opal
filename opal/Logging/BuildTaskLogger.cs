using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Opal.Logging
{
    /// <summary>
    /// Adapts TaskLoggingHelper to ILogger
    /// </summary>
    public class BuildTaskLogger: ILogger
    {
        private readonly TaskLoggingHelper loggingHelper;
        private readonly string filePath;

        public BuildTaskLogger(TaskLoggingHelper loggingHelper, string filePath)
        {
            this.loggingHelper = loggingHelper;
            this.filePath = filePath;
        }

        public void LogError(string message, params object[] messageArgs) =>
            loggingHelper.LogError(message: message, 
                messageArgs: messageArgs);

        public void LogError(Segment segment, string message, params object[] messageArgs) =>
            loggingHelper.LogError(subcategory:null, 
                errorCode:null, 
                helpKeyword:null,
                file:filePath,
                lineNumber:segment.Start.Ln, 
                columnNumber:segment.Start.Col, 
                endLineNumber:segment.End.Ln, 
                endColumnNumber:segment.End.Col,
                message:message, 
                messageArgs:messageArgs);

        public void LogWarning(string message, params object[] messageArgs) =>
            loggingHelper.LogWarning(message: message, 
                messageArgs: messageArgs);

        public void LogWarning(Segment segment, string message, params object[] messageArgs) =>
            loggingHelper.LogWarning(subcategory:null,
                warningCode: null,
                helpKeyword: null,
                file: filePath,
                lineNumber: segment.Start.Ln,
                columnNumber: segment.Start.Col,
                endLineNumber: segment.End.Ln,
                endColumnNumber: segment.End.Col,
                message: message,
                messageArgs: messageArgs);

        public void LogMessage(Importance importance, string message, params object[] messageArgs) =>
            loggingHelper.LogMessage(
                importance:(MessageImportance)importance, 
                message: message, 
                messageArgs: messageArgs);

        public void LogMessage(Importance importance, Segment segment, string message, params object[] messageArgs) =>
            loggingHelper.LogMessage(subcategory:null, 
                code:null, 
                helpKeyword:null, 
                file: filePath, 
                lineNumber: segment.Start.Ln, 
                columnNumber: segment.Start.Col,
                endLineNumber: segment.End.Ln, 
                endColumnNumber: segment.End.Col, 
                importance: (MessageImportance)importance,
                message: message, 
                messageArgs: messageArgs);
    }
}
