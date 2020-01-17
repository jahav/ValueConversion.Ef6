namespace ValueConversion.Ef6
{
    using System;

    public class ValueConverter
    {
        private readonly Type _fromType;
        private readonly Type _toType;

        public ValueConverter(Type fromType, Type toType)
        {
            _fromType = fromType;
            _toType = toType;
        }
    }
}