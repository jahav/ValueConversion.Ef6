namespace ValueConversion.Ef6
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A representation of map target - mediator.
    /// Inheritance not counted, table per concrete type.
    /// </summary>
    public class MediatorTypeMap
    {
        public MediatorTypeMap(Type targetType, Type mediatorType)
        {
            TargetType = targetType;
            MediatorType = mediatorType;
        }

        internal Type MediatorType { get; }

        internal Type TargetType { get; }

        /// <summary>
        /// Convert a property of the target type into a property of the mediator type.
        /// </summary>
        /// <param name="propertyInfo">A property of a target type.</param>
        /// <exception cref="InvalidOperationException">Property is not present on the mediator.</exception>
        internal MemberInfo ConvertToMediator(PropertyInfo propertyInfo)
        {
            if (propertyInfo.ReflectedType != TargetType)
            {
                throw new ArgumentException($"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} belongs to type {propertyInfo.ReflectedType}, but this map is for {TargetType}.");
            }

            var property = MediatorType.GetProperty(propertyInfo.Name);
            if (property == null)
            {
                throw new InvalidOperationException($"Property {propertyInfo.ReflectedType}.{propertyInfo.Name} is not found in the mediator type. Change your configuration to include it ({nameof(ConversionConfiguration)}.{nameof(ConversionConfiguration.IsAllowedForColumn)} or {nameof(ConversionConfiguration)}.{nameof(ConversionConfiguration.ShouldMediateTargetProperty)}).");
            }

            return property;
        }
    }
}