using System.IO;

namespace Opal.Containers
{
    public static class Resources
    {
        public static string LoadText(string name)
        {
            var assm = typeof(Resources).Assembly;
            using var stream = assm.GetManifestResourceStream(name);
            if (stream == null)
                return string.Empty;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
