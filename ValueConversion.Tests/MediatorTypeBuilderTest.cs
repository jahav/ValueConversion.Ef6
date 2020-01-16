namespace ValueConversion.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public class MediatorTypeBuilderTest
    {
        [Fact]
        public void MediatorTypeHasParameterlessCtor()
        {
            var a = new Dictionary<Type, Type> { { typeof(string), typeof(PhoneNumber) } };
            var cfg = new ConversionConfiguration
            {
                IsAllowedForColumn = x => true,
            };
            var mediatorTypeBuilder = new MediatorTypeBuilder(a, cfg);

            var mediatorType = mediatorTypeBuilder.CreateMediatorType(typeof(EmptyType));

            mediatorType.GetConstructors().Should().HaveCount(1);
            mediatorType.Should().HaveConstructor(Type.EmptyTypes);
        }

        private class EmptyType
        {
        }
    }
}
