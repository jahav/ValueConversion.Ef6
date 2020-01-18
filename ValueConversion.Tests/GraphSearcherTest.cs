namespace ValueConversion.Tests
{
    using System.Linq;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;
    using static ValueConversion.Ef6.TargetTypeGraph;

    public class GraphSearcherTest
    {
        [Fact]
        public void PrimitiveTypeAsRoot_WillBeEmptyGraph()
        {
            var primitiveType = typeof(int);
            var config = new ConversionConfiguration
            {
                IsAllowedForColumn = x => x == primitiveType,
            };
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(primitiveType);
            graph.Nodes.Should().BeEmpty();
        }

        [Fact]
        public void PropertiesMaterializedFromDbColumnAreAlwaysAllowed()
        {
            var originType = typeof(Outer);
            var primitiveType = new Outer().Primitive.GetType();
            var config = new ConversionConfiguration
            {
                IsAllowedForColumn = x => x == primitiveType,
                ShouldMediateTargetProperty = x => false,
            };
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(originType);

            graph.Nodes.Should().ContainSingle()
                .Which.Type.Should().Be(originType);
            graph.Nodes.Single().ColumnMembers
                .Should().Equal(typeof(Outer).GetProperty(nameof(Outer.Primitive)));
        }

        [Fact]
        public void CyclicGraphs_DontCauseEndlessRecursion()
        {
            var primitiveType = typeof(int);
            var config = new ConversionConfiguration
            {
                IsAllowedForColumn = x => false,
                ShouldMediateTargetProperty = x => true,
            };
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(typeof(CycleLink1));
            graph.Nodes.Select(_ => _.Type).Should().Contain(new[] { typeof(CycleLink1), typeof(CycleLink2) });
        }

        [Fact]
        public void TryingToSearchMoreThanMaxLevelDeep_ThrowsSanityException()
        {
            var primitiveType = typeof(int);
            var config = new ConversionConfiguration
            {
                IsAllowedForColumn = x => false,
                ShouldMediateTargetProperty = x => true,
                MaxRecursion = 0,
            };
            var searcher = new GraphSearcher(config);
            searcher.Invoking(x => x.SearchGraph(typeof(Outer)))
                .Should().Throw<SanityException>().WithMessage("*recursion limit*");
        }

        [Fact]
        public void GraphHasMultipleLevels_AllAreFound()
        {
            var originType = typeof(Outer);
            var config = new ConversionConfiguration
            {
                IsAllowedForColumn = x => x.IsPrimitive,
                ShouldMediateTargetProperty = x => true,
            };
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(originType);

            graph.Nodes.Select(x => x.Type).Should().BeEquivalentTo(typeof(Outer), typeof(Inner), typeof(Innermost));
            var outerNode = graph.Nodes.Single(x => x.Type == typeof(Outer));
            var innerNode = graph.Nodes.Single(x => x.Type == typeof(Inner));
            var innermostNode = graph.Nodes.Single(x => x.Type == typeof(Innermost));
            graph.Edges.Should().BeEquivalentTo(
                new Edge(outerNode, innerNode, typeof(Outer).GetProperty(nameof(Outer.Inner))),
                new Edge(innerNode, innermostNode, typeof(Inner).GetProperty(nameof(Inner.Innermost))));
        }

        private class Outer
        {
            public int Primitive { get; set; }

            public Inner Inner { get; set; }
        }

        private class Inner
        {
            public int Column { get; set; }

            public Innermost Innermost { get; set; }
        }

        private class Innermost
        {
            public int InnermostColumn { get; set; }
        }

        private class CycleLink1
        {
            public CycleLink2 Link { get; set; }
        }

        private class CycleLink2
        {
            public CycleLink1 Link { get; set; }
        }
    }
}
