using Generators;

namespace Opal.Productions
{
    public interface IReduction
    {
        void Write<T>(T generator) where T:Generator<T>;
    }
    
    public abstract class ReductionBase: IReduction
    {
        protected readonly int id;
        protected readonly IReduceExpr expr;
        
        public ReductionBase(int id, IReduceExpr expr)
        {
            this.expr = expr;
            this.id = id;

        }

        public abstract void Write<T>(T generator) where T:Generator<T>;
    }

    public class PushReduce: ReductionBase
    {
        public PushReduce(int id, IReduceExpr expr)
            : base(id, expr)
        {
        }

        public override void Write<T>(T generator) 
        {
            generator.Write("state = Push(")
                .Write(id)
                .Write(", ");
            expr.Write(generator);
            generator.WriteLine(");");
        }
    }


    public class Reduce: ReductionBase
    {
        private readonly int items;
        
        public Reduce(int items, int id, IReduceExpr expr)
            : base(id, expr)
        {
            this.items = items;
        }

        public override void Write<T>(T generator)
        {
            generator.WriteLine($"items = {items};");
            generator.Write($"state = Reduce({id}, ");
            expr.Write(generator);
            generator.WriteLine(");");
        }
    }
}
