using Generators;
using Opal.Containers;
using Opal.Nfa;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public class DfaSwitchWriter: IGeneratable, IVarProvider
    {
        private readonly Dfa _dfa;
        public DfaSwitchWriter(Dfa dfa)
        {
            _dfa = dfa;
        }

        public void Write(Generator generator)
        {
            TemplateProcessor.FromAssembly(generator, this, "Opal.FrameFiles.SwitchScanner.txt");
        }

        bool IVarProvider.AddVarValue(Generator generator, string varName)
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

        private void WriteStates(IGenerator generator)
        {
            generator.Indent();
            var nextStates = new HashSet<int>();
            var edges = _dfa.MaxClass;

            foreach (var state in _dfa.States)
            {
                if (state.Index != 0)
                    generator.WriteLine().WriteLine($"State{state.Index}:");
                generator.Indent();

                if (state.IsAccepting && _dfa.TryGetAccepting(state.AcceptingState, out string name))
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
                            var transMatch = _dfa.Matches.Find(i);
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
                        item.Value.Write(generator, "_ch");
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
    }
}
