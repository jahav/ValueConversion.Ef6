namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class GraphSearcher
    {
        private readonly ConversionConfiguration _configuration;

        public GraphSearcher(ConversionConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Depth first search.
        /// </summary>
        public TargetTypeGraph SearchGraph(Type origin)
        {
            var graph = new TargetTypeGraph(origin);
            if (_configuration.IsAllowedForColumn(origin))
            {
                return graph;
            }

            SearchNode(graph, origin, 0);
            return graph;
        }

        private void SearchNode(TargetTypeGraph graph, Type node, int level)
        {
            if (level > _configuration.MaxRecursion)
            {
                throw new SanityException($"While looking for a connected graph we went over the recursion limit {level}.");
            }

            if (!graph.AddNode(node))
            {
                return;
            }

            foreach (var property in node.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var propertyType = property.PropertyType;
                if (_configuration.IsAllowedForColumn(propertyType))
                {
                    graph.AddProperty(node, property);
                }
                else
                if (_configuration.ShouldMediateTargetProperty(property))
                {
                    graph.AddEdge(node, property);
                    SearchNode(graph, propertyType, level + 1);
                }
                else
                {
                    // ignore
                }
            }
        }
    }

    /// <summary>
    /// This finds the graph, all nodes and edges that must be translated.
    /// It doesn't do the translation, it only finds what must be translated.
    /// </summary>
    public class TargetTypeGraph
    {
        private readonly Type _origin;

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

        public TargetTypeGraph(Type origin)
        {
            _origin = origin;
        }

        public IReadOnlyCollection<Node> Nodes =>
            _nodes ?? (_nodes = _graph.Select(x => new Node(x.Key, x.Value.Values.Where(e => _keep.Contains(e)))).ToList().AsReadOnly());

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
            internal Node(Type type, IEnumerable<Type> columnPropertie)
            {
                Type = type;
                ColumnProperties = columnPropertie.ToList().AsReadOnly();
            }

            public Type Type { get; }

            public IReadOnlyCollection<Type> ColumnProperties { get; }
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
