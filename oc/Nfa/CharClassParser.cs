using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Nfa
{
    public class CharClassParser
    {
        private readonly string text;
        
        public CharClassParser(string text)
        {
            this.text = text ?? string.Empty;
        }

        public bool Invert { get; }

        public static void Parse()
        {

        }
    }
}
