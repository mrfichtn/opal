using System.IO;

namespace Opal.Nfa
{
    public interface INfaWriter
    {
        void Write(Graph graph, string? srcFile);
    }

    /// <summary>
    /// Generates a "debug" file from the Nfa graph
    /// </summary>
    public class NfaWriter: INfaWriter
    {
        private readonly string? nfaPath;
        
        public NfaWriter(string? nfaPath) =>
            this.nfaPath = nfaPath;

        public void Write(Graph graph, string? srcFile)
        {
            var filePath = nfaPath;
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = string.IsNullOrEmpty(srcFile) ?
                    "nfa.txt" :
                    Path.ChangeExtension(srcFile, ".nfa.txt");
            }
            File.WriteAllText(filePath, graph.ToString());
        }
    }

    public class NullNfaWriter: INfaWriter
    {
        public void Write(Graph graph, string? srcFile)
        {
        }
    }
}
