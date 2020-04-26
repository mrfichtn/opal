using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq.Expressions;


namespace ExprBuilder
{
	public partial class Parser
	{
        private Expression ParseDoubleAsInt(Token token)
        {
            var text = token.Value.Substring(0, token.Value.Length - 1);
            var value = double.Parse(text);
            return Expression.Constant(value);
        }

        private Expression Divide(Expression left, Expression right)
        {
            Coerce(ref left, ref right);
            return Expression.Divide(left, right);
        }

        private Expression Multiply(Expression left, Expression right)
        {
            Coerce(ref left, ref right);
            return Expression.Multiply(left, right);
        }

        private Expression Subtract(Expression left, Expression right)
        {
            Coerce(ref left, ref right);
            return Expression.Subtract(left, right);
        }

        private Expression Add(Expression left, Expression right)
        {
            Coerce(ref left, ref right);
            return Expression.Add(left, right);

        }

        private bool Coerce(ref Expression left, ref Expression right)
        {
            return
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

        private object Lambda(string identifier, Expression expr)
        {
            return null;
        }
    }
}
