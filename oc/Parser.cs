using Opal.Nfa;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Opal
{
    public partial class Parser
    {
        private readonly Dictionary<string, IMatch> _charClasses = new Dictionary<string, IMatch>();
        private readonly StringBuilder usings = new StringBuilder();
        private readonly ProductionList _productions = new ProductionList();
        private readonly ConflictList conflicts = new ConflictList();
        
        #region Properties

        public Language? Language => Root as Language;
        public string Usings => usings.ToString();
        public Graph Graph { get; private set; } = new Graph();

        #endregion

        private StringBuilder AddNamespace(Identifier? id)
        {
            return (id != null) ?
                usings.Append("using ").Append(id.Value).Append(';').AppendLine() :
                usings;
        }

        private Graph CreateGraph(IMatch match) => Graph.Create(match);
        private Graph CreateGraph(EscChar value) => CreateGraph(new SingleChar(value));

        private Graph SetGraph(Graph graph)
        {
            Graph = graph;
            return graph;
        }

        private Graph SetGraph() => Graph.Create();

        private IMatch FindCharClass(Token identifier)
        {
            if (!_charClasses.TryGetValue(identifier.Value, out var result))
            {
                result = EmptyMatch.Instance;
                logger.LogError(
                    $"Missing character class '{identifier.Value}'",
                    identifier);
            }

            return result;
        }

        private object? Add(NamedCharClass charClass)
        {
           
            if (_charClasses.TryGetValue(charClass.Name.Value, out var oldClass))
            {
                var id = charClass.Name;
                logger.LogError(
                    $"Duplicate character definition '{charClass.Name}' (old={oldClass})",
                    id);
            }
            else
                _charClasses.Add(charClass.Name.Value, charClass.Chars);
            return null;
        }

        /// <summary>
        /// Called when a string occurs in the production area
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        //private StringTokenProd AddStringTokenProd(StringConst str)
        //{
        //    try
        //    {
        //        if (str == null)
        //            throw new ArgumentNullException(nameof(str));
        //        var text = str.Value;

        //        if (Graph == null)
        //            throw new Exception("Graph is null");

        //        var state = Graph.FindState(text);
        //        string name;
        //        if (state == -1)
        //        {
        //            name = CreateName(text);
        //            var g = Graph.Create(text);
        //            state = g.MarkEnd(name, text);
        //            Graph.Union(g);
        //        }
        //        else
        //        {
        //            Graph.Machine.AcceptingStates.TryGetName(state, out name);
        //        }
        //        return new StringTokenProd(str, name, state);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Uncaught exception: {ex.Message}",
        //            str);
        //        throw;
        //    }
        //}

        /// <summary>
        /// Called when a character occurs in the production area
        /// </summary>
        //private StringTokenProd AddStringTokenProd(CharConst str)
        //{
        //    if (str == null)
        //        throw new ArgumentNullException(nameof(str));

        //    var text = new string(str.Value, 1);
        //    string name;
        //    var state = Graph.FindState(text);
        //    if (state == -1)
        //    {
        //        name = CreateName(text);
        //        var g = Graph.Create(text);
        //        state = g.MarkEnd(name, text);
        //        Graph.Union(g);
        //    }
        //    else
        //    {
        //        Graph.Machine.AcceptingStates.TryGetName(state, out name);
        //    }
        //    return new StringTokenProd(str, name, text, state);
        //}

        //private static string CreateName(string text)
        //{
        //    string name;
        //    if ((text.Length > 0) && char.IsLetter(text[0]))
        //        name = "@" + text;
        //    else
        //        name = text;
        //    return name;
        //}
    }
}
