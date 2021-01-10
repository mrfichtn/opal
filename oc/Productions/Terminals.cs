using Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Opal.Productions
{
    public interface ITerminals: IEnumerable<TerminalBase>
    {
        TerminalBase this[int index] { get; }
        int Length { get; }

        void Write(IGenerator generator, int id);
        void WriteForEmptyAction(ActionWriteContext context);
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

        public void Write(IGenerator generator, int id)
        {
            generator.WriteLine($"items = 1;");
            generator.Write($"state = Reduce({id}, ");
        }

        public void WriteForEmptyAction(ActionWriteContext context)
        {
            context.Production.Attribute.WriteEmptyAction(context, this);
        }

        public string Arg(Grammar grammar)
        {
            var arg = new StringBuilder();
            grammar.TryFindDefault(data.Name, out var type);
            arg.Append("At");
            data.WriteType(arg, type);
            arg.Append("(0)");
            return arg.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public class Terminals: ITerminals
    {
        private readonly TerminalBase[] data;

        public Terminals(TerminalBase[] data) =>
            this.data = data;

        public int Length => data.Length;

        public TerminalBase this[int index] => data[index];

        public void Write(IGenerator generator, int id)
        {
            generator.WriteLine($"items = {Length};");
            generator.Write($"state = Reduce({id}, ");
        }

        public IEnumerator<TerminalBase> GetEnumerator() =>
            (data as IList<TerminalBase>).GetEnumerator();


        public void WriteForEmptyAction(ActionWriteContext context) =>
            context.Production.Attribute.WriteEmptyAction(context, this);

        public string ArgList(Grammar grammar)
        {
            var finalArgs = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                var right = data[i];
                grammar.TryFindDefault(right.Name, out var type);
                finalArgs.Append("At");
                right.WriteType(finalArgs, type);
                finalArgs
                    .Append('(').Append(i).Append("),");
            }
            finalArgs.Length--;
            return finalArgs.ToString();
        }

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

        public void Write(IGenerator generator, int id) =>
            generator.Write($"state = Push({id}, ");

        public void WriteForEmptyAction(ActionWriteContext context) =>
            context.Production.Attribute.WriteEmptyAction(context);
    }
}
