using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcTest
{
    public class Expr
    {
        public virtual int Calc()
        {
            return 0;
        }
    }

    public class BinaryExpr: Expr
    {
        protected BinaryExpr(Expr left, Expr right)
        {
            _left = left;
            _right = right;
        }
        protected Expr _left;
        protected Expr _right;
    }

    public class SubExpr : BinaryExpr
    {
        public SubExpr(Expr left, Expr right)
            : base(left, right)
        {
        }

        public override int Calc()
        {
            return _left.Calc() - _right.Calc();
        }
    }


    public class AddExpr : BinaryExpr
    {
        public AddExpr(Expr left, Expr right)
            : base(left, right)
        {
        }

        public override int Calc()
        {
            return _left.Calc() + _right.Calc();
        }
    }


    public class MultiExpr: BinaryExpr
    {
        public MultiExpr(Expr left, Expr right)
            : base(left, right)
        {
        }

        public override int Calc()
        {
            return _left.Calc() * _right.Calc();
        }
    }

    public class Constant: Expr
    {
        private readonly int _value;

        public Constant(Token token)
        {
            _value = int.Parse(token.Value);
        }

        public override int Calc()
        {
            return _value;
        }
    }
}
