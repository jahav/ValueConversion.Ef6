namespace ValueConversion.Tests
{
    using System;
    using System.Linq.Expressions;
    using ExpressionTreeToString;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class SimpleIntegrationTest
    {
        [Fact]
        public void ConvertTypeToMediator_WithNoPropertiesWorks()
        {
            Expression<Func<CustomerEntity, Address>> identity = customer => new Address();
            AssertTargetToMediator(identity, "(CustomerEntity customer) => new «Address»()");
        }

        [Fact]
        public void ConvertTypeToMediator_WithSinglePropertyWorks()
        {
            Expression<Func<CustomerEntity, Address>> identity = customer => new Address { Street = customer.WorkStreet };
            var result =
@"
(CustomerEntity customer) => new «Address»() {
    Street = customer.WorkStreet
}";
            AssertTargetToMediator(identity, result);
        }

        [Fact]
        public void NestedTargetTypes_AreTransformed()
        {
            Expression<Func<CustomerEntity, Customer>> nestedTypes = customer => new Customer
            {
                WorkAddress = new Address
                {
                    Street = customer.WorkStreet,
                },
            };
            var result =
@"
(CustomerEntity customer) => new «Customer»() {
    WorkAddress = new «Address»() {
        Street = customer.WorkStreet
    }
}";
            AssertTargetToMediator(nestedTypes, result);
        }

        private void AssertTargetToMediator<TParameter, TResult>(
            Expression<Func<TParameter, TResult>> projection,
            string resultTargetToMediator)
        {
            var config = new ConversionConfiguration()
            {
                IsAllowedForColumn = x => x.IsValueType || x == typeof(string),
            };
            var searcher = new GraphSearcher(config);
            var graph = searcher.SearchGraph(typeof(TResult));

            var mediatorMapper = new MediatorTypeBuilder().CreateMediatorTypes(graph);

            var targetToMediatorVisitor = new TargetToMediatorVisitor(mediatorMapper);
            var result = targetToMediatorVisitor.Visit(projection);
            result.ToString("C#").Should().Be(resultTargetToMediator.Trim());
        }
    }
}
