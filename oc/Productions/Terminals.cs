using System;
using System.Collections;
using System.Collections.Generic;


namespace Opal.Productions
{
    public interface ITerminals: IEnumerable<TerminalBase>
    {
        TerminalBase this[int index] { get; }

        int Length { get; }

        IReduction Reduction(ReduceContext context);
        IReduceExpr Reduce(ReduceContext context);
    }

    public static class TerminalsExt
    {
        public static IReduceExpr[] CreateArgs(this ITerminals terminals, ReduceContext context)
        {
            var result = new IReduceExpr[terminals.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = terminals[i].Reduce(context);
            return result;
        }
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


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReduction Reduction(ReduceContext context) =>
            new Reduce(1, context.Id, context.ActionReduce());

        public IReduceExpr Reduce(ReduceContext context) => new ReduceArgExpr(0);
    }

    public class Terminals: ITerminals
    {
        private readonly TerminalBase[] data;

        public Terminals(TerminalBase[] data) =>
            this.data = data;

        public int Length => data.Length;

        public TerminalBase this[int index] => data[index];

        public IReduction Reduction(ReduceContext context) =>
            new Reduce(data.Length, context.Id, context.ActionReduce());

        public IReduceExpr Reduce(ReduceContext context) => 
            context.DefaultReduce();

        public IEnumerator<TerminalBase> GetEnumerator() =>
            (data as IList<TerminalBase>).GetEnumerator();

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

        public IReduction Reduction(ReduceContext context) =>
            new PushReduce(context.Id, context.ActionReduce());

        public IReduceExpr Reduce(ReduceContext context) => new ReduceNullExpr();
    }
}
