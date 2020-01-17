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
            var config = new ConversionConfiguration();
            Expression<Func<CustomerEntity, Address>> identity = customer => new Address();

            var materializedTypesVisitor = new MaterializedTypesVisitor();

            materializedTypesVisitor.Visit(identity);

            var mediatorTypeBuilder = new MediatorTypeBuilder(_noCustomConverters, config);

            var mediatorMapper = new MediatorMapper();
            foreach (var materializedType in materializedTypesVisitor.MaterializedTypes)
            {
                var mediatorType = mediatorTypeBuilder.CreateMediatorType(materializedType);
                var typeMap = new MediatorTypeMap(materializedType, mediatorType);
                mediatorMapper.AddMediatorTypeMap(typeMap);
            }

            var targetToMediatorVisitor = new TargetToMediatorVisitor(mediatorMapper);
            var result = targetToMediatorVisitor.Visit(identity);
            result.ToString("C#").Should().Be("(CustomerEntity customer) => new AddressProxy()");
        }
    }
}
