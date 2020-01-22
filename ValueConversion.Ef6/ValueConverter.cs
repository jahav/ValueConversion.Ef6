namespace ValueConversion.Ef6
{
    using System;
    using System.Linq.Expressions;

    public abstract class ValueConverter
    {
        protected ValueConverter(
            LambdaExpression convertToProviderExpression,
            LambdaExpression convertFromProviderExpression)
        {
            if (convertToProviderExpression.Parameters.Count != 1)
            {
                throw new ArgumentException($"The expressin must have one parameter.", nameof(convertToProviderExpression));
            }

            if (convertFromProviderExpression.Parameters.Count != 1)
            {
                throw new ArgumentException($"The expressin must have one parameter.", nameof(convertFromProviderExpression));
            }

            ConvertToProviderExpression = convertToProviderExpression;
            ConvertFromProviderExpression = convertFromProviderExpression;
            ModelClrType = convertFromProviderExpression.Body.Type;
            ProviderClrType = convertToProviderExpression.Body.Type;
        }

        public LambdaExpression ConvertToProviderExpression { get; }

        public LambdaExpression ConvertFromProviderExpression { get; }

        public Type ModelClrType { get; }

        public Type ProviderClrType { get; }
    }
}
