using Opal.Nfa;
using System;

namespace Opal.ParseTree
{
    public class DeclareTokenContext
    {
        private readonly Logger logger;
        private readonly Graph graph;
        
        public DeclareTokenContext(Logger logger, Graph graph)
        {
            this.logger = logger;
            this.graph = graph;
        }

        public (string name, int id) AddDefinition(StringConst str)
        {
            string name;
            int state;
            try
            {
                if (str == null)
                    throw new ArgumentNullException(nameof(str));
                var text = str.Value;

                state = graph.FindState(text);
                if (state == -1)
                {
                    name = CreateName(str.Value);
                    var g = graph.Create(text);
                    state = g.MarkEnd(name, text);
                    graph.Union(g);
                }
                else
                {
                    graph.Machine.AcceptingStates.TryGetName(state, out name);
                }
                return (name, state);
            }
            catch (Exception ex)
            {
                logger.LogError($"Uncaught exception: {ex.Message}",
                    str);
                throw;
            }
        }

        private static string CreateName(string text)
        {
            string name;
            if ((text.Length > 0) && char.IsLetter(text[0]))
                name = "@" + text;
            else
                name = text;
            return name;
        }
    }
}
