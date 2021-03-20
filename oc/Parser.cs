using Opal.ParseTree;

namespace Opal
{
    public partial class Parser
    {
        public bool TryParse(out Language? language)
        {
            var result = Parse();
            language = Root as Language;
            return result;
        }
        
        public Language? Language => Root as Language;
    }
}
