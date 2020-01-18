namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This finds the graph, all nodes and edges that must be translated.
    /// It doesn't do the translation, it only finds what must be translated.
    /// </summary>
    public class TargetTypeGraph
    {
        /// <summary>
        /// Key is node, represented by type, value are directed graphs edges (name of member and type target).
        /// Nodes should be translated. Edges can point either to a node or a <see cref="_keep"/>.
        /// </summary>
        private readonly Dictionary<Type, Dictionary<MemberInfo, Type>> _graph = new Dictionary<Type, Dictionary<MemberInfo, Type>>();

        /// <summary>
        /// Types that shouldn't be translated, e.g. <see cref="int"/>, <see cref="string"/>.
        /// <see cref="_graph"/> doesn't have nodes for these types, but edges point to them.
        /// </summary>
        private readonly HashSet<Type> _keep = new HashSet<Type>();

        // TODO: Refactor, no lazy initialization
        private IReadOnlyCollection<Node>? _nodes;
        private IReadOnlyCollection<Edge>? _edges;

        public IReadOnlyCollection<Node> Nodes =>
            _nodes ?? (_nodes = _graph.Select(x => new Node(x.Key, x.Value.Where(e => _keep.Contains(e.Value)).Select(x => x.Key))).ToList().AsReadOnly());

        // This only returns an edges between nodes, not column properties.
        public IReadOnlyCollection<Edge> Edges =>
            _edges ?? (_edges =
            _graph.SelectMany(node => node.Value.Select(to => new { From = node.Key, To = to.Value, Member = to.Key }))
                    .Where(x => !_keep.Contains(x.To))
                    .Select(x => new Edge(
                            Nodes.Single(from => from.Type == x.From),
                            Nodes.Single(to => to.Type == x.To),
                            x.Member))
                    .ToList());

        public bool AddNode(Type node)
        {
            if (_keep.Contains(node))
            {
                throw new InvalidOperationException($"Type {node} is already marked as a keep. It can't be also used as a node.");
            }

            if (_graph.ContainsKey(node))
            {
                return false;
            }

            var edges = new Dictionary<MemberInfo, Type>();
            _graph.Add(node, edges);
            return true;
        }

        public void AddEdge(Type node, PropertyInfo member)
        {
            var propertyType = member.PropertyType;
            _graph[node].Add(member, propertyType);
        }

        public void AddProperty(Type node, PropertyInfo member)
        {
            var propertyType = member.PropertyType;
            if (_graph.ContainsKey(propertyType))
            {
                throw new InvalidOperationException($"Type {propertyType} is already marked as a node. It can't be also used as a property.");
            }

            _keep.Add(member.PropertyType);
            _graph[node].Add(member, propertyType);
        }

        public class Node
        {
            internal Node(Type type, IEnumerable<MemberInfo> columnMembers)
            {
                Type = type;
                ColumnMembers = columnMembers.ToList().AsReadOnly();
            }

            public Type Type { get; }

            public IReadOnlyCollection<MemberInfo> ColumnMembers { get; }
        }

        public class Edge
        {
            internal Edge(Node from, Node to, MemberInfo member)
            {
                From = from;
                To = to;
                Member = member;
            }

            public Node From { get; }

            public Node To { get; }

            public MemberInfo Member { get; }
        }
    }
}
