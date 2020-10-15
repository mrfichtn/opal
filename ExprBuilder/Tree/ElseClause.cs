using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprBuilder.Tree
{
    public class ElseClause
    {
        public readonly Stmt stmt;

        public ElseClause(Stmt stmt)
        {
            this.stmt = stmt;
        }
    }
}
