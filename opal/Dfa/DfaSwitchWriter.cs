using Generators;
using Opal.Containers;
using Opal.Nfa;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public class DfaSwitchWriter: IGeneratable, ITemplateContext
    {
        private readonly Dfa dfa;
        public DfaSwitchWriter(Dfa dfa) => 
            this.dfa = dfa;

        public void Write(Generator generator) =>
            TemplateProcessor.FromAssembly(generator, this, "Opal.FrameFiles.SwitchScanner.txt");

        private void WriteStates(IGenerator generator)
        {
            generator.Indent();
            var nextStates = new HashSet<int>();
            var edges = dfa.MaxClass;

            foreach (var state in dfa.States)
            {
                if (state.Index != 0)
                    generator.WriteLine().WriteLine($"State{state.Index}:");
                generator.Indent();

                if (state.IsAccepting && dfa.TryGetAccepting(state.AcceptingState, out string name))
                    generator.WriteLine($"MarkAccepting(TokenStates.{name});");

                if (state.Index != 0)
                    generator.WriteLine("NextChar();");

                state.CopyNextStatesTo(nextStates);
                var remaining = new EmptyMatch().Invert();
                var pairs = new List<KeyValuePair<int, IMatch>>
                {
                    new KeyValuePair<int, IMatch>(0, remaining)
                };

                int i;
                foreach (var nextState in nextStates.Where(x => (x != 0)))
                {
                    IMatch match = new EmptyMatch();
                    for (i = 0; i < edges; i++)
                    {
                        if (state[i] == nextState)
                        {
                            var transMatch = dfa.Matches.Find(i);
                            match = match.Union(transMatch);
                        }
                    }
                    match = match.Reduce();
                    pairs.Add(new KeyValuePair<int, IMatch>(nextState, match));
                }

                i = 0;
                var last = pairs.Count - 1;
                foreach (var item in pairs.OrderBy(x => x.Value.Count))
                {
                    if (i > 0)
                        generator.WriteLine();
                    if (i < last)
                    {
                        generator.Write("if (");
                        item.Value.Write(generator, "ch");
                        generator.Write(") ");
                    }

                    generator.Write("goto ");
                    if (item.Key == 0)
                    {
                        if (i == last && state.Index == 0)
                            generator.Write("EndState2;");
                        else
                            generator.Write("EndState;");
                    }
                    else
                        generator.Write("State")
                            .Write(item.Key.ToString())
                            .Write(";");
                    i++;
                }

                generator.UnIndent();
            }

            generator.UnIndent();
        }

        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            var found = true;
            switch (varName)
            {
                case "dfa.states":
                    WriteStates(generator);
                    break;
                default:
                    found = false;
                    break;
            }
            return found;
        }

        bool ITemplateContext.Condition(string varName)
        {
            return false;
        }

        string? ITemplateContext.Include(string name)
        {
            return null;
        }
    }
}
