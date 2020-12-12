using System.Collections.Generic;

namespace Opal.Nfa
{
    public class Machine
    {
        private readonly List<List<int>> classToNodes;

        public Machine()
        {
            Nodes = new NfaNodes();
            Matches = new Matches();
            classToNodes = new List<List<int>>();
            AcceptingStates = new AcceptingStates();
        }

        #region Properties
        public readonly NfaNodes Nodes;
        public Matches Matches { get; }
        public AcceptingStates AcceptingStates { get; }
        #endregion

        /// <summary>
        /// Searches for match:
        ///   (1) If found: creates transition to new node
        ///   (2) Not found and doesn't intersect existing match: adds to matches & create new transition
        ///   (3) Else: split existing matches & corresponding nfa-nodes: adds intersection & final match transitions
        /// </summary>
        /// <param name="end">Node from which to create transition</param>
        /// <param name="match">Match</param>
        /// <returns>Last match node</returns>
        public int SetMatch(int end, IMatch match)
        {
            int node;

            if (Matches.TryGet(match, out int clsId))
            {
                node = Nodes.CreateMatch(clsId, end);
                Register(node, clsId);
            }
            else
            {
                var matches = Matches.GetMatches();
                var prev = -1;

                foreach (var key in matches)
                {
                    var intersection = key.Intersect(match);
                    if (intersection != null)
                    {
                        var value = this.Matches[key];
                        var diff = key.Difference(intersection);
                        if (!(diff is EmptyMatch))
                        {
                            this.Matches.Replace(key, intersection, value);
                            var diffId = this.Matches.Add(diff);

                            var classToNode = classToNodes[value];
                            for (var i = 0; i < classToNode.Count; i++)
                                classToNode[i] = SplitNode(classToNode[i], diffId);
                        }

                        node = Nodes.CreateMatch(value, end, prev);
                        Register(node, value);

                        diff = match.Difference(intersection);
                        if (diff is EmptyMatch)
                        {
                            return node;
                        }
                        else
                        {
                            match = diff;
                            prev = node;
                        }
                    }
                }

                var newValue = this.Matches.Add(match);
                node = Nodes.CreateMatch(newValue, end, prev);
                Register(node, newValue);
            }
            return node;
        }

        /// <summary>
        /// Searches for character match:
        ///   (1) If found, returns class-id
        ///   (2) Not found and doesn't intersect existing match: adds to matches and returns class-id
        ///   (3) Else: split existing matches & associated nfa-nodes, return new class-id
        /// </summary>
        /// <param name="end">Node from which to create transition</param>
        /// <param name="match">Match</param>
        /// <returns>Last match node</returns>
        public int GetClassId(char ch)
        {
            var match = new SingleChar(ch);
            if (!Matches.TryGet(match, out var keyId))
            {
                var matches = this.Matches.GetMatches();
                keyId = this.Matches.Add(match);

                foreach (var key in matches)
                {
                    if (key.IsMatch(ch))
                    {
                        var value = this.Matches[key];
                        var diff = key.Difference(match);
                        this.Matches.Replace(key, diff, value);

                        var classToNode = classToNodes[value];
                        for (var i = 0; i < classToNode.Count; i++)
                            classToNode[i] = SplitNodeChar(classToNode[i], keyId);
                        break;
                    }
                }
            }
            return keyId;
        }

        /// <summary>
        /// Splits node into two matches
        ///  ⎧               ⎫  --left-->  { node 1 (match = orig-match) } -- orig left / right ->
        ///  ⎨   orig - node ⎬  
        ///  ⎩ ( match =  -1 ⎭  --right--> { node 2 (match = diff-id) }    -- orig left / right ->
        ///
        /// </summary>
        /// <param name="nodeId">Original node</param>
        /// <param name="matchId">New match type</param>
        /// <returns>First node created</returns>
        private int SplitNode(int nodeId, int matchId)
        {
            var node = Nodes[nodeId];
            var node1 = Nodes.CreateMatch(node.Match, node.Left, node.Right);
            // registration is completed in calling method
            var node2 = Nodes.CreateMatch(matchId, node.Left, node.Right);
            Register(node2, matchId);

            Nodes.Set(nodeId, -1, node1, node2);
            AcceptingStates.Transfer(nodeId, node1, node1);

            return node1;
        }

        /// <summary>
        /// Splits node into two matches, where diffId is character and doesn't need to be tracked
        ///  ⎧               ⎫  --left-->  { node 1 (match = orig-match) } -- orig left / right ->
        ///  ⎨   orig - node ⎬  
        ///  ⎩ ( match =  -1 ⎭  --right--> { node 2 (match = diff-id) }    -- orig left / right ->
        ///
        /// </summary>
        /// <param name="nodeId">Original node</param>
        /// <param name="charId">New char-match type</param>
        /// <returns>First node created</returns>
        private int SplitNodeChar(int nodeId, int charId)
        {
            var node = Nodes[nodeId];
            var node1 = Nodes.CreateMatch(node.Match, node.Left, node.Right);
            var node2 = Nodes.CreateMatch(charId, node.Left, node.Right);
            Nodes.Set(nodeId, -1, node1, node2);
            AcceptingStates.Transfer(nodeId, node.Left, node.Right);
            return node1;
        }

        /// <summary>
        /// Records nodes against their class-id
        /// </summary>
        /// <param name="nodeIndex">Node index</param>
        /// <param name="classId">Class id</param>
        private void Register(int nodeIndex, int classId)
        {
            while (classToNodes.Count <= classId)
                classToNodes.Add(new List<int>());
            var list = classToNodes[classId];
            list.Add(nodeIndex);
        }
    }
}
