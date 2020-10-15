using ExprBuilder.Tree;
using System;
using System.Linq.Expressions;

namespace ExprBuilder
{
    class Program
    {
        static void Main()
        {
            string text;
            text = "if 1 print";
            var p2 = Ambiguity.Parser.FromString(text);
            var isOk = p2.Parse();
            var root = p2.Root as Stmts;
            
            
            text = "4D * (-4D-3)";
            var parser = Parser.FromString(text);

            parser.Parse();
            var item = parser.Root as Expr;
            var context = new Context();
            var expr = item.CreateExpression(context);
            var lambda = Expression.Lambda(expr).Compile();
            var result = lambda.DynamicInvoke();
            Console.WriteLine(result);
        }
    }
}
