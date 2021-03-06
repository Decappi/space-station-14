﻿using System;
using System.Collections.Generic;
using System.IO;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using YamlDotNet.RepresentationModel;

namespace Content.Shared.Construction
{
    [Prototype("constructionGraph")]
    public class ConstructionGraphPrototype : IPrototype, IIndexedPrototype
    {
        private readonly Dictionary<string, ConstructionGraphNode> _nodes = new Dictionary<string, ConstructionGraphNode>();
        private Dictionary<ValueTuple<string, string>, ConstructionGraphNode[]> _paths = new Dictionary<ValueTuple<string, string>, ConstructionGraphNode[]>();
        private Dictionary<ConstructionGraphNode, ConstructionGraphNode> _pathfinding = new Dictionary<ConstructionGraphNode, ConstructionGraphNode>();

        [ViewVariables]
        public string ID { get; private set; }

        [ViewVariables]
        public string Start { get; private set; }

        [ViewVariables]
        public IReadOnlyDictionary<string, ConstructionGraphNode> Nodes => _nodes;

        public void LoadFrom(YamlMappingNode mapping)
        {
            var serializer = YamlObjectSerializer.NewReader(mapping);

            serializer.DataField(this, x => x.ID, "id", string.Empty);
            serializer.DataField(this, x => x.Start, "start", string.Empty);

            if (!mapping.TryGetNode("graph", out YamlSequenceNode graphMapping)) return;

            foreach (var yamlNode in graphMapping)
            {
                var childMapping = (YamlMappingNode) yamlNode;
                var node = new ConstructionGraphNode();
                node.LoadFrom(childMapping);
                _nodes[node.Name] = node;
            }

            if(string.IsNullOrEmpty(Start) || !_nodes.ContainsKey(Start))
                throw new InvalidDataException($"Starting node for construction graph {ID} is null, empty or invalid!");

            _pathfinding = Pathfind(Start);
        }

        public ConstructionGraphEdge Edge(string startNode, string nextNode)
        {
            var start = _nodes[startNode];
            return start.GetEdge(nextNode);
        }

        public ConstructionGraphNode[] Path(string startNode, string finishNode)
        {
            var tuple = new ValueTuple<string, string>(startNode, finishNode);

            if (_paths.ContainsKey(tuple))
                return _paths[tuple];

            var start = _nodes[startNode];
            var finish = _nodes[finishNode];

            var current = finish;
            var path = new List<ConstructionGraphNode>();
            while (current != start)
            {
                path.Add(current);

                // No path.
                if (current == null || !_pathfinding.ContainsKey(current))
                {
                    // We remember this for next time.
                    _paths[tuple] = null;
                    return null;
                }

                current = _pathfinding[current];
            }

            path.Reverse();
            return _paths[tuple] = path.ToArray();
        }

        /// <summary>
        ///     Uses breadth first search for pathfinding.
        /// </summary>
        /// <param name="start"></param>
        private Dictionary<ConstructionGraphNode, ConstructionGraphNode> Pathfind(string start)
        {
            // TODO: Make this use A* or something, although it's not that important.
            var startNode = _nodes[start];

            var frontier = new Queue<ConstructionGraphNode>();
            var cameFrom = new Dictionary<ConstructionGraphNode, ConstructionGraphNode>();

            frontier.Enqueue(startNode);
            cameFrom[startNode] = null;

            while (frontier.Count != 0)
            {
                var current = frontier.Dequeue();
                foreach (var edge in current.Edges)
                {
                    var edgeNode = _nodes[edge.Target];
                    if(cameFrom.ContainsKey(edgeNode)) continue;
                    frontier.Enqueue(edgeNode);
                    cameFrom[edgeNode] = current;
                }
            }

            return cameFrom;
        }
    }
}
