namespace ValueConversion.Tests
{
    using System;
    using System.Linq;
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
        public void NewExpressionWithoutArgumentsShouldBeCaptures()
        {
            Expression<Func<int, Customer>> projection = x => new Customer();
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().BeEquivalentTo(new[] { typeof(Customer) });
        }

        [Fact]
        public void NewExpressionWithArgumentsShouldBeCaptures()
        {
            Expression<Func<int, Customer>> projection = x => new Customer(x);
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
            visitor.MaterializedTypes.Should().BeEquivalentTo(typeof(Customer), typeof(Address));
        }

        [Fact]
        public void AnonymousTypeShouldBeDetected()
        {
            Expression<Func<CustomerEntity, object>> projection = x => new { x.Id };
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().HaveCount(1)
                .And.ContainSingle(t => t.Name.StartsWith("<") && t.Name.Contains("AnonymousType"));
        }

        [Fact]
        public void SubselectsTypesShouldBeFound()
        {
            Expression<Func<CustomerEntity, Customer>> projection = x => new Customer
            {
                PhoneNumbers = x.PhoneNumbers.Select(pn => new Contact { Number = (PhoneNumber)pn.Number, Location = pn.Location }).ToList(),
            };
            var visitor = new MaterializedTypesVisitor();
            visitor.Visit(projection);
            visitor.MaterializedTypes.Should().BeEquivalentTo(typeof(Contact), typeof(Customer));
        }
    }
}
