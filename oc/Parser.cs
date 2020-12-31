using Opal.Nfa;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.Text;

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
        
        #region Properties

        public Language? Language => Root as Language;

        #endregion
    }
}
