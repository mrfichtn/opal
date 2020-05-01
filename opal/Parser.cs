using Opal.Nfa;
using Opal.ParseTree;
using System.Collections.Generic;
using System.Text;

namespace Opal
{
    public partial class Parser
    {
        private Dictionary<string, IMatch> _charClasses;
        private Dictionary<string, string> _options;
        private StringBuilder _usings;
        private ProductionList _productions;
        private Machine _machine;

        partial void Init()
        {
            _charClasses = new Dictionary<string, IMatch>();
            _usings = new StringBuilder();
            _productions = new ProductionList();
            _machine = new Machine();
        }

        #region Properties

        public Language Language => Root as Language;
        public string Usings => _usings.ToString();
        public Graph Graph { get; private set; }

        #endregion

        public void SetOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public bool TryGetOption(string key, out string value)
        {
            return _options.TryGetValue(key, out value);
        }

        private StringBuilder AddNamespace(Identifier id)
        {
            return _usings.Append("using ")
                .Append(id.Value)
                .Append(';')
                .AppendLine();
        }

        private Graph CreateGraph(IMatch match) => new Graph(_machine, match);
        private Graph CreateGraph(EscChar value) => CreateGraph(new SingleChar(value));

        private Graph SetGraph(Graph graph)
        {
            Graph = graph;
            return graph;
        }

        private Graph SetGraph() => new Graph(_machine);

        private IMatch FindCharClass(Token identifier)
        {
            if (!_charClasses.TryGetValue(identifier.Value, out var result))
            {
                result = EmptyMatch.Instance;
                _logger.LogError(identifier, "Missing character class '{0}'", identifier.Value);
                _hasErrors = true;
            }

            return result;
        }

        private object Add(NamedCharClass charClass)
        {
            
            if (_charClasses.TryGetValue(charClass.Name.Value, out var oldClass))
            {
                _logger.LogError(charClass.Name,
                    "Duplicate character definition '{0}' (old={1})", 
                    charClass.Name,
                    oldClass);
                //_logger.LogError(oldClass.Name, "  previous definition of '{0}'", charClass.Name);
            }
            else
                _charClasses.Add(charClass.Name.Value, charClass.Chars);
            return null;
        }

        private object AddOption(Token token, StringConst value)
        {
            _options[token.Value] = value.Value;
            return null;
        }

        private StringTokenProd AddStringTokenProd(StringConst str)
        {
            var text = str.Value;
            var state = Graph.FindState(text);
            if (state == -1)
            {
                var g = Graph.Create(text);
                state = g.MarkEnd(CreateName(text));
                Graph.Union(g);
            }
            return new StringTokenProd(str, state);
        }

        private StringTokenProd AddStringTokenProd(CharConst str)
        {
            var text = new string(str.Value, 1);
            var state = Graph.FindState(text);
            if (state == -1)
            {
                var g = Graph.Create(text);
                state = g.MarkEnd(CreateName(text));
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
