namespace ValueConversion.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ExpressionTreeToString;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class SimpleIntegrationTest
    {
        private static readonly IReadOnlyDictionary<Type, Type> _noCustomConverters = new Dictionary<Type, Type>();

        [Fact]
        public void ConvertTypeToMediator_WithNoPropertiesWorks()
        {
            Expression<Func<CustomerEntity, Address>> identity = customer => new Address();
            AssertTargetToMediator(identity, "(CustomerEntity customer) => new «Address»()", _noCustomConverters);
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
            AssertTargetToMediator(identity, result, _noCustomConverters);
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
            AssertTargetToMediator(nestedTypes, result, _noCustomConverters);
        }

        private void AssertTargetToMediator(Expression projection, string resultTargetToMediator, IReadOnlyDictionary<Type, Type> convertors)
        {
            var config = new ConversionConfiguration();

            var materializedTypesVisitor = new MaterializedTypesVisitor();
            materializedTypesVisitor.Visit(projection);

            var mediatorTypeBuilder = new MediatorTypeBuilder(convertors, config);

            var mediatorMapper = new MediatorMapper();
            foreach (var materializedType in materializedTypesVisitor.MaterializedTypes)
            {
                var mediatorType = mediatorTypeBuilder.CreateMediatorType(materializedType);
                var typeMap = new MediatorTypeMap(materializedType, mediatorType);
                mediatorMapper.AddMediatorTypeMap(typeMap);
            }

            var targetToMediatorVisitor = new TargetToMediatorVisitor(mediatorMapper);
            var result = targetToMediatorVisitor.Visit(projection);
            result.ToString("C#").Should().Be(resultTargetToMediator.Trim());
        }
    }
}
