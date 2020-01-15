namespace ValueConversion.Tests
{
    using System;
    using System.Linq.Expressions;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class MaterializedTypesVisitorTest
    {
        [Fact]
        public void SelectExpressionWithNoTypes()
        {
            Expression<Func<int, int>> projection = x => x;
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().BeEmpty();
        }

        [Fact]
        public void RootEntityShouldBeCaptures()
        {
            Expression<Func<int, Customer>> projection = x => new Customer();
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().BeEquivalentTo(new[] { typeof(Customer) });
        }

        [Fact]
        public void MultipleOccuranceOfTypeWillBeIncludedOnce()
        {
            Expression<Func<CustomerEntity, Customer>> projection = x => new Customer
            {
                DeliveryAddress = new Address(),
                WorkAddress = new Address(),
            };
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().BeEquivalentTo(new[] { typeof(Customer), typeof(Address) });
        }
    }
}
