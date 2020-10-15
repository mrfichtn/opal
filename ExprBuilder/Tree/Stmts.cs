using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprBuilder.Tree
{
    class Stmts: List<Stmt>
    {
        public Stmts()
        { }

        public static Stmts Add(Stmts stmts, Stmt stmt)
        {
            stmts.Add(stmt);
            return stmts;
        }
    }
}
