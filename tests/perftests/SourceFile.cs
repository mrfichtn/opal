using System.IO;

namespace perftests
{
    public class SourceFile
    {
        public SourceFile(string path)
        {
            Name = Path.GetFileName(path);
            Source = File.ReadAllText(path);
        }

        public string Name { get; }
        public string Source { get; }

    }
}
