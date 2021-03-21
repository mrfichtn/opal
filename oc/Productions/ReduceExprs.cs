using Generators;
using System.Linq;

namespace Opal.Productions
{
    public interface IReduceExpr
    {
        void Write<T>(T generator) where T:Generator<T>;
    }

    public class ReduceNullExpr: IReduceExpr
    {
        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write("null");
    }

    public class ReduceValueExpr: IReduceExpr
    {
        private readonly string value;

        public ReduceValueExpr(string value) =>
            this.value = value;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write(value);
    }

    public class ReduceStringExpr: IReduceExpr
    {
        private readonly string value;

        public ReduceStringExpr(string value) =>
            this.value = value;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.WriteEsc(value);
    }

    public class ReduceArgExpr: IReduceExpr
    {
        private readonly int arg;
        public ReduceArgExpr(int arg) =>
            this.arg = arg;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write("At(")
                .Write(arg)
                .Write(")");
    }

    public class ReduceCastedExpr: IReduceExpr
    {
        public readonly string cast;
        public readonly IReduceExpr expr;

        public ReduceCastedExpr(string cast, IReduceExpr expr)
        {
            this.cast = cast;
            this.expr = expr;
        }

        public void Write<T>(T generator) 
            where T:Generator<T>
        {
            generator.Write("(")
                .Write(cast)
                .Write(")");
            expr.Write(generator);
        }
    }

    public class ReduceCastedArgExpr: IReduceExpr
    {
        public readonly int arg;
        public readonly string type;

        public ReduceCastedArgExpr(int arg, string type)
        {
            this.arg = arg;
            this.type = type;
        }

        public void Write<T>(T generator)
            where T: Generator<T>
        {
            generator.Write("At<")
                .Write(type)
                .Write(">(")
                .Write(arg)
                .Write(")");
        }
    }

    public class ReduceMethodExpr: IReduceExpr
    {
        private readonly string methodName;
        private readonly IReduceExpr[] args;

        public ReduceMethodExpr(string methodName,
            params IReduceExpr[] args)
        {
            this.methodName = methodName;
            this.args = args;
        }

        public void Write<T>(T generator) where T:Generator<T>
        {
            generator.Write(methodName)
                .Write("(")
                .Join(args,
                    (generator, item) => item.Write(generator),
                    ",")
                .Write(")");
        }
    }

    public class ReduceNewExpr: IReduceExpr
    {
        private readonly string typeName;
        private readonly IReduceExpr[] args;

        public ReduceNewExpr(string typeName,
            params IReduceExpr[] args)
        {
            this.typeName = typeName;
            this.args = args;
        }

        public void Write<T>(T generator)
            where T:Generator<T>
        {
            generator
                .Write("new ")
                .Write(typeName)
                .Write("(")
                .Join(args,
                    (generator, item) => item.Write(generator),
                    ",")
                .Write(")");
        }
    }

    public class ReduceFieldExpr: IReduceExpr
    {
        private readonly IReduceExpr expr;
        private readonly string fieldName;
        
        public ReduceFieldExpr(IReduceExpr expr, string fieldName)
        {
            this.expr = expr;
            this.fieldName = fieldName;
        }

        public void Write<T>(T generator) where T : Generator<T>
        {
            expr.Write(generator);
            generator.Write('.')
                .Write(fieldName);
        }
    }
}
