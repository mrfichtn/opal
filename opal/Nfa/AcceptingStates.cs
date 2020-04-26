using Opal.Containers;
using System;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public class AcceptingStates
    {
        /// <summary>
        /// Maps symbol name to symbol index
        /// </summary>
        private readonly Dictionary<string, int> symbols;
        private readonly List<string> names;
        private readonly List<string> ignore;

        public AcceptingStates()
        {
            symbols = new Dictionary<string, int>();
            names = new List<string>();
            ignore = new List<string>();
            Nodes = new Dictionary<int, int>();

            symbols.Add("Empty", 0);
            names.Add("Empty");
        }

        #region Properties

        /// <summary>
        /// Maps node index to symbol index
        /// </summary>
        public Dictionary<int, int> Nodes { get; }

        public IEnumerable<string> Names => names;

        public IEnumerable<(string name, int index)> AllStates
        {
            get
            {
                var count = 0;
                foreach (var name in names)
                    yield return (name, count++);

                count = -2;
                foreach (var name in ignore)
                    yield return (name, count--);
            }
        }

        /// <summary>
        /// Returns state count
        /// </summary>
        public int Count => names.Count;

        #endregion

        /// <summary>
        /// Returns name of accepting state
        /// </summary>
        /// <param name="index">state index</param>
        /// <param name="name">name given to state</param>
        /// <returns>True if symbol exists</returns>
        public bool TryGetName(int index, out string name) =>
            (index >= 0) ?
                names.TryGetValue(index, out name) :
                ignore.TryGetValue(-index - 2, out name);

        /// <summary>
        /// Adds new symbol
        /// </summary>
        /// <param name="name">Name of symbol</param>
        /// <param name="ignore">True if symbol should be ignored (e.g. whitespace)</param>
        /// <param name="symbolIndex">Symbol index</param>
        /// <returns>True if symbol is added, false if already exists</returns>
        public bool TryAdd(string name, bool ignore, int node, out int symbolIndex)
        {
            var result = TryAdd(name, ignore, out symbolIndex);
            if (result)
            {
                if (Nodes.TryGetValue(node, out var prevSymbolIndex))
                    throw StateAlreadyExists(prevSymbolIndex);
                Nodes.Add(node, symbolIndex);
            }
            return result;
        }

        /// <summary>
        /// Returns accepting state at node
        /// </summary>
        /// <param name="node">NFA node index</param>
        /// <param name="state">Accepting state at node</param>
        /// <returns>True if node has an accepting state</returns>
        public bool TryFind(int node, out int state) => 
            Nodes.TryGetValue(node, out state);

        /// <summary>
        /// Moves accepting state from node to left and right nodes
        /// </summary>
        /// <param name="node">NFA node index</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void Transfer(int node, int left, int right)
        {
            if (Nodes.TryGetValue(node, out var acceptingState))
            {
                Nodes.Remove(node);
                Nodes.Add(left, acceptingState);
                Nodes.Add(right, acceptingState);
            }
        }

        /// <summary>
        /// Adds new symbol
        /// </summary>
        /// <param name="name">Name of symbol</param>
        /// <param name="ignore">True if symbol should be ignored (e.g. whitespace)</param>
        /// <param name="symbolIndex">Symbol index</param>
        /// <returns>True if symbol is added, false if already exists</returns>
        private bool TryAdd(string name, bool ignore, out int symbolIndex)
        {
            var result = !symbols.TryGetValue(name, out symbolIndex);
            if (result)
            {
                symbolIndex = ignore ? AddIgnore(name) : Add(name);
                symbols.Add(name, symbolIndex);
            }
            return result;
        }

        /// <summary>
        /// Adds new symbol
        /// </summary>
        /// <param name="name">Name of symbol</param>
        /// <param name="ignore">True if symbol should be ignored (e.g. whitespace)</param>
        /// <param name="index">Index of symbol</param>
        /// <returns>True if symbol is added, false if already exists</returns>
        private int AddIgnore(string name)
        {
            var index = -(2 + ignore.Count);
            ignore.Add(name);
            return index;
        }

        private int Add(string name)
        {
            var index = names.Count;
            names.Add(name);
            return index;
        }

        private Exception StateAlreadyExists(int symbolIndex)
        {
            if (!TryGetName(symbolIndex, out var name))
                name = $"(index = {symbolIndex})";
            return new InvalidOperationException(
                    $"Node has already been attached to accepting state '{name}'");
        }
    }
}
