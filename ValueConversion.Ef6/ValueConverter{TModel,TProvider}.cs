namespace ValueConversion.Ef6
{
    using System;
    using System.Linq.Expressions;

    public class ValueConverter<TModel, TProvider> : ValueConverter
    {
        public ValueConverter(
            Expression<Func<TModel, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TModel>> convertFromProviderExpression)
            : base(convertToProviderExpression, convertFromProviderExpression)
        {
        }
    }
}
