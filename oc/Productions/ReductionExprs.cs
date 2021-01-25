using Generators;
using System.Linq;

namespace Opal.Productions
{
    public interface IReductionExpr
    {
        void Write<T>(T generator) where T:Generator<T>;
    }

    public class NullReductionExpr: IReductionExpr
    {
        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write("null");
    }

    public class ValueReductionExpr: IReductionExpr
    {
        private readonly string value;

        public ValueReductionExpr(string value) =>
            this.value = value;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write(value);
    }

    public class StringReductionExpr: IReductionExpr
    {
        private readonly string value;

        public StringReductionExpr(string value) =>
            this.value = value;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.WriteEsc(value);
    }


    public class ArgReductionExpr: IReductionExpr
    {
        private readonly int arg;
        public ArgReductionExpr(int arg) =>
            this.arg = arg;

        public void Write<T>(T generator) where T: Generator<T> =>
            generator.Write("At(")
                .Write(arg)
                .Write(")");
    }

    public class CastedReductionExpr: IReductionExpr
    {
        public readonly string cast;
        public readonly IReductionExpr expr;

        public CastedReductionExpr(string cast, IReductionExpr expr)
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

    public class CastedArgReductionExpr: IReductionExpr
    {
        public readonly int arg;
        public readonly string type;

        public CastedArgReductionExpr(int arg, string type)
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

    public class MethodReductionExpr: IReductionExpr
    {
        private readonly string methodName;
        private readonly IReductionExpr[] args;

        public MethodReductionExpr(string methodName,
            params IReductionExpr[] args)
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

    public class NewReductionExpr: IReductionExpr
    {
        private readonly string typeName;
        private readonly IReductionExpr[] args;

        public NewReductionExpr(string typeName,
            params IReductionExpr[] args)
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

    public class FieldReductionExpr: IReductionExpr
    {
        private readonly IReductionExpr expr;
        private readonly string fieldName;
        
        public FieldReductionExpr(IReductionExpr expr, string fieldName)
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
