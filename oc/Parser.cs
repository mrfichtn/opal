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
        private Dictionary<string, object> _options = new Dictionary<string, object>();
        private readonly StringBuilder usings = new StringBuilder();
        private readonly ProductionList _productions = new ProductionList();
        private readonly ConflictList conflicts = new ConflictList();
        
        partial void Init()
        {
        }

        #region Properties

        public Language? Language => Root as Language;
        public string Usings => usings.ToString();
        public Graph Graph { get; private set; } = new Graph();

        #endregion

        public void SetOptions(Dictionary<string, object> options) =>
            _options = options;

        public bool TryGetOption(string key, out string? text)
        {
            var result = _options.TryGetValue(key, out var value);
            text = result ? value as string : null;
            return result;
        }

        public bool HasOption(string key)
        {
            if (!_options.TryGetValue(key, out var value))
                return false;

            if (value is bool b)
                return b;
            if (value is string s)
                return s != null &&
                    !s.Equals("false", StringComparison.InvariantCultureIgnoreCase) &&
                    !s.Equals("0");
            return true;
        }


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

                var logItem = new LogItem(LogLevel.Error,
                    $"Missing character class '{identifier.Value}'",
                    identifier,
                    _scanner.Line(identifier.Start.Ln)
                    );
                log = log.Enqueue(logItem);
                _hasErrors = true;
            }

            return result;
        }

        private object? Add(NamedCharClass charClass)
        {
           
            if (_charClasses.TryGetValue(charClass.Name.Value, out var oldClass))
            {
                var id = charClass.Name;
                var chars = charClass.Chars;
                var token = new Token(id.Start, 
                    id.End,
                    0, 
                    id.Value);

                var logItem = new LogItem(LogLevel.Error,
                    $"Duplicate character definition '{charClass.Name}' (old={oldClass})",
                    token,
                    _scanner.Line(charClass.Name.Start.Ln));
                log = log.Enqueue(logItem);
                _hasErrors = true;



                //_logger.LogError(oldClass.Name, "  previous definition of '{0}'", charClass.Name);
            }
            else
                _charClasses.Add(charClass.Name.Value, charClass.Chars);
            return null;
        }

        private object? AddOption(Identifier id, StringConst value)
        {
            _options[id.Value] = value.Value;
            return null;
        }

        private object? AddOption(Identifier id, BoolConst value)
        {
            _options[id.Value] = value.Value;
            return null;
        }

        /// <summary>
        /// Called when a string occurs in the production area
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private StringTokenProd AddStringTokenProd(StringConst str)
        {
            try
            {
                if (str == null)
                    throw new ArgumentNullException(nameof(str));
                var text = str.Value;

                if (Graph == null)
                    throw new Exception("Graph is null");

                var state = Graph.FindState(text);
                if (state == -1)
                {
                    var g = Graph.Create(text);
                    state = g.MarkEnd(CreateName(text), text);
                    Graph.Union(g);
                }
                return new StringTokenProd(str, state);
            }
            catch (Exception ex)
            {
                var token = new Token(str.Start,
                    str.End,
                    0, 
                    str.Value);
                var logItem = new LogItem(LogLevel.Error,
                    $"Uncaught exception: {ex.Message}",
                    token,
                    _scanner.Line(str.Start.Ln));
                log = log.Enqueue(logItem);
                throw;
            }
        }

        /// <summary>
        /// Called when a character occurs in the production area
        /// </summary>
        private StringTokenProd AddStringTokenProd(CharConst str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            var text = new string(str.Value, 1);
            var state = Graph.FindState(text);
            if (state == -1)
            {
                var g = Graph.Create(text);
                state = g.MarkEnd(CreateName(text), text);
                Graph.Union(g);
            }
            return new StringTokenProd(str, text, state);
        }

        private string CreateName(string text)
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
