namespace ValueConversion.Tests
{
    using System.Reflection;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class GraphSearcherConvertersTest
    {
        [Fact]
        public void MemberWithConverterType_IsColumnMember()
        {
            var config = new ConversionConfiguration();
            config.AddConvertor(new ValueConverter<Id<SingleProperty>, int>(model => model.Value, provider => (Id<SingleProperty>)provider));
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(typeof(SingleProperty));
            graph.Nodes.Should().ContainSingle()
                .Which.ColumnMembers.Should().ContainSingle(x => x.Name == nameof(SingleProperty.Id) && ((PropertyInfo)x).PropertyType == typeof(Id<SingleProperty>));
        }

        private class SingleProperty
        {
            public Id<SingleProperty> Id { get; set; }
        }
    }
}
