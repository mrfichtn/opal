using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Opal.Productions
{
    public interface ITerminals: IEnumerable<TerminalBase>
    {
        TerminalBase this[int index] { get; }
        int Length { get; }

        IReductionExpr ReduceEmpty(ReduceContext context);

        IReduction Reduce(ReduceContext context);
    }

    public class SingleTerminal: ITerminals
    {
        private readonly TerminalBase data;

        public SingleTerminal(TerminalBase data) =>
            this.data = data;

        public TerminalBase this[int index] => 
            (index == 0) ? 
            data : 
            throw new ArgumentOutOfRangeException(nameof(index));

        public int Length => 1;

        public IEnumerator<TerminalBase> GetEnumerator()
        {   yield return data; }


        public IReductionExpr ReduceEmpty(ReduceContext context) =>
            context.Attr.Reduction(context, this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReductionExpr Reduction(ReduceContext context) =>
            //new ArgReductionExpr(0);
            data.Reduce(context);

        public IReduction Reduce(ReduceContext context) =>
            new Reduce(1, context.Id, context.ReductionExpr());
    }


    public class Terminals: ITerminals
    {
        private readonly TerminalBase[] data;

        public Terminals(TerminalBase[] data) =>
            this.data = data;

        public int Length => data.Length;

        public TerminalBase this[int index] => data[index];

        public IEnumerator<TerminalBase> GetEnumerator() =>
            (data as IList<TerminalBase>).GetEnumerator();


        public IReduction Reduce(ReduceContext context) =>
            new Reduce(data.Length, context.Id, context.ReductionExpr());

        public IReductionExpr[] Reduction(ReduceContext context) =>
            data.Select(x => x.Reduce(context)).ToArray();

        public IReductionExpr ReduceEmpty(ReduceContext context) =>
            context.Attr.Reduction(context, this);

        IEnumerator IEnumerable.GetEnumerator() =>
            data.GetEnumerator();
    }

    public class EmptyTerminals: ITerminals
    {
        public int Length => 0;

        public TerminalBase this[int index] =>
            throw new ArgumentOutOfRangeException(nameof(index));

        public IEnumerator<TerminalBase> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReduction Reduce(ReduceContext context) =>
            new PushReduce(context.Id, context.ReductionExpr());

        public IReductionExpr ReduceEmpty(ReduceContext context) =>
            context.Attr.Reduction(context);

        public IReduction Reduction(ReduceContext context) =>
            new PushReduce(context.Id, context.Attr.Reduction(context));
    }
}
