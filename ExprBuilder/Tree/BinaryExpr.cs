using System.Linq.Expressions;

namespace ExprBuilder.Tree
{
    public abstract class BinaryExpr: Expr
    {
        public BinaryExpr(Segment seg, Expr left, Expr right)
            : base(seg)
        {
            Left = left;
            Right = right;
        }

        public BinaryExpr(Expr left, Expr right)
            : base(left)
        {
            End = right.End;
            Left = left;
            Right = right;
        }


        public Expr Left { get; }
        public Expr Right { get; }

        public override Expression CreateExpression(Context ctx)
        {
            var left = Left.CreateExpression(ctx);
            var right = Right.CreateExpression(ctx);
            Coerce(ref left, ref right);
            return CreateExpression(ctx, left, right);
        }

        protected virtual Expression CreateExpression(Context ctx, Expression left, Expression right)
        {
            return left;
        }

        protected bool Coerce(Context ctx, out Expression left, out Expression right)
        {
            left = Left.CreateExpression(ctx);
            right = Right.CreateExpression(ctx);
            return Coerce(ref left, ref right);
        }

        protected bool Coerce(ref Expression left, ref Expression right)
        {
            return
                (left.Type == right.Type) ||
                Coerce<string>(ref left, ref right) ||
                Coerce<double>(ref left, ref right) ||
                Coerce<float>(ref left, ref right) ||
                Coerce<long>(ref left, ref right) ||
                Coerce<int>(ref left, ref right) ||
                Coerce<short>(ref left, ref right) ||
                Coerce<ulong>(ref left, ref right) ||
                Coerce<uint>(ref left, ref right) ||
                Coerce<ushort>(ref left, ref right);
        }

        private bool Coerce<T>(ref Expression left, ref Expression right)
        {
            var typeT = typeof(T);
            bool result;
            if (left.Type == typeT)
            {
                right = Expression.Convert(right, typeT);
                result = true;
            }
            else if (right.Type == typeT)
            {
                left = Expression.Convert(left, typeT);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }

    public class AddBinary: BinaryExpr
    {
        public AddBinary(Expr left, Expr right): base(left, right) {}

        protected override Expression CreateExpression(Context ctx, Expression left, Expression right)
        {
            return Expression.Add(left, right);
        }
    }

    public class SubtractBinary : BinaryExpr
    {
        public SubtractBinary(Expr left, Expr right)
            : base(left, right)
        {
        }

        protected override Expression CreateExpression(Context ctx, Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }
    }

    public class MultiplyBinary : BinaryExpr
    {
        public MultiplyBinary(Expr left, Expr right)
            : base(left, right)
        {
        }

        protected override Expression CreateExpression(Context ctx, Expression left, Expression right)
        {
            return Expression.Multiply(left, right);
        }
    }

    public class DivideBinary : BinaryExpr
    {
        public DivideBinary(Expr left, Expr right)
            : base(left, right)
        {
        }

        protected override Expression CreateExpression(Context ctx, Expression left, Expression right)
        {
            return Expression.Divide(left, right);
        }
    }

}
