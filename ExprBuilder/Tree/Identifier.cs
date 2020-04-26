using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprBuilder.Tree
{
    public class Identifier: Segment
    {
        public Identifier(Token t)
            : base(t)
        {
            Value = t.Value;
        }

        public string Value { get; }
    }
}
