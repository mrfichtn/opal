using Generators;
using Opal.Containers;
using Opal.Nfa;
using Opal.Templates;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public class DfaSwitchWriter: IGeneratable, ITemplateContext
    {
        private readonly Dfa dfa;
        private readonly IEnumerable<DfaNode> states;
        public DfaSwitchWriter(Dfa dfa, bool syntaxErrorTokens)
        {
            this.dfa = dfa;
            this.states = syntaxErrorTokens ?
                dfa.States.AddSyntaxError() :
                dfa.States;
        }

        public void Write(Generator generator) =>
            TemplateProcessor2.FromAssembly(generator, this, "Opal.FrameFiles.SwitchScanner.txt");

        private void WriteStates(IGenerator generator)
        {
            generator.Indent();
            var nextStates = new HashSet<int>();
            var edges = dfa.MaxClass;

            IMatch emptyMatches = new AllMatch();
            foreach (var item in dfa.Matches)
                emptyMatches = emptyMatches.Difference(item);

            foreach (var state in states)
            {
                if (state.Index != 0)
                    generator.WriteLine().WriteLine($"State{state.Index}:");
                generator.Indent();
                if (state.IsAccepting)
                {
                    if (dfa.TryGetAccepting(state.AcceptingState, out string name))
                        generator.WriteLine($"MarkAccepting(TokenStates.{name});");
                    else if (state.AcceptingState == -1)
                        generator.WriteLine($"MarkAccepting(TokenStates.SyntaxError);");
                }

                if (state.Index != 0)
                    generator.WriteLine("NextChar();");

                if (state.Index != 0)
                    generator.WriteLine("if (ch == -1) goto EndState;");
                
                state.CopyNextStatesTo(nextStates);
                var pairs = new List<KeyValuePair<int, string>>();

                int i;
                foreach (var nextState in nextStates)
                {
                    IMatch match = new EmptyMatch();
                    for (i = 0; i < edges; i++)
                    {
                        if (state[i] == nextState)
                        {
                            var transMatch = (i == 0) ? 
                                emptyMatches :
                                dfa.Matches.Find(i);
                            if (transMatch != null)
                                match = match.Union(transMatch);
                        }
                    }
                    match = match.Reduce();
                    pairs.Add(new KeyValuePair<int, string>(nextState, match.SwitchWriter("ch")));
                }

                i = 0;
                var last = pairs.Count - 1;
                foreach (var item in pairs.OrderBy(x => x.Value.Length))
                {
                    if (i > 0)
                        generator.WriteLine();
                    if (i < last)
                    {
                        generator.Write("if (");
                        //item.Value.Write(generator, "ch");
                        generator.Write(item.Value);
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
