namespace ValueConversion.Ef6
{
    using System;
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
            var graph = new TargetTypeGraph();
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
}
