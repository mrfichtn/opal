using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Containers
{
    public struct Pair<T,U>
    {
        public readonly T Item1;
        public readonly U Item2;
        
        public Pair(T item1, U item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
