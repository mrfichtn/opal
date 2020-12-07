using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Nfa
{
    public class EofSymbol: Symbol
    {
        public EofSymbol()
            : base("Empty", 0)
        {
        }
    }
}
