using Microsoft.VisualStudio.Shell.Interop;
using Opal.Logging;
using System;
using System.IO;

namespace Opal
{
    public class CompileArgs: ILogger
    {
        private readonly IVsGeneratorProgress progress;

        public CompileArgs(string inputFilePath, 
            string inputFileContents,
            string defaultNamespace,
            TextWriter output,
            IVsGeneratorProgress progress)
        {
            InputFilePath = inputFilePath;
            InputContents = inputFileContents;
            DefaultNamespace = defaultNamespace;
            Output = output;
            this.progress = progress;
        }

        public CompileArgs(string inputFilePath,
            string fileContents,
            string defaultNamespace,
            TextWriter output,
            Action<bool, int, string, int, int> callback)
        {
            InputFilePath = inputFilePath;
            InputContents = fileContents;
            DefaultNamespace = defaultNamespace;
            Output = output;
            progress = new VsGeneratorProgress(callback);
        }


        public string InputContents { get; }
        
        public string InputFilePath { get; }
        public string DefaultNamespace { get; }

        public TextWriter Output { get; }

        public void Warn(int line, int column, string message) =>
            progress.GeneratorError(1, 0, message, (uint) line, (uint) column);

        public void Error(int line, int column, string message) =>
            progress.GeneratorError(0, 0, message, (uint)line, (uint)column);

        public void LogError(string message, params object[] messageArgs)
        {
            Error(0, 0, string.Format(message, messageArgs));
        }

        public void LogError(Segment segment, string message, params object[] messageArgs)
        {
            Error(segment.Start.Ln, segment.Start.Col, string.Format(message, messageArgs));
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            Warn(0, 0, string.Format(message, messageArgs));
        }

        public void LogWarning(Segment segment, string message, params object[] messageArgs)
        {
            Warn(segment.Start.Ln, segment.Start.Col, string.Format(message, messageArgs));
        }

        public void LogMessage(Importance importance, string message, params object[] messageArgs)
        {
        }

        public void LogMessage(Importance importance, Segment segment, string message, params object[] messageArgs)
        {
        }

        class VsGeneratorProgress: IVsGeneratorProgress
        {
            Action<bool, int, string, int, int> callback;

            public VsGeneratorProgress(Action<bool, int, string, int, int> callback) =>
                this.callback = callback;

            public int GeneratorError(int fWarning, uint dwLevel, string bstrError, uint dwLine, uint dwColumn)
            {
                callback(fWarning != 0,
                    (int) dwLevel,
                    bstrError,
                    (int) dwLine,
                    (int) dwColumn);
                return 0;
            }

            public int Progress(uint nComplete, uint nTotal)
            {
                throw new NotImplementedException();
            }
        }
    }
}
