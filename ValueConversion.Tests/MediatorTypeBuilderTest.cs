namespace ValueConversion.Tests
{
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class MediatorTypeBuilderTest
    {
        [Fact]
        public void MediatorTypeHasParameterlessCtor()
        {
            var mediatorTypeBuilder = new MediatorTypeBuilder();
            var targetType = typeof(EmptyType);
            var graph = new TargetTypeGraph();
            graph.AddNode(targetType);
            var mediatorMapper = mediatorTypeBuilder.CreateMediatorTypes(graph);

            var mediatorType = mediatorMapper.GetMediatorType(targetType);

            mediatorType.Should().HaveDefaultConstructor();
        }

        private class EmptyType
        {
        }
    }
}
