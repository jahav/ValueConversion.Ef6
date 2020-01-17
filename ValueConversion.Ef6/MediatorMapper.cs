namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class MediatorMapper
    {
        /// <summary>
        /// Key is target type.
        /// </summary>
        private readonly Dictionary<Type, MediatorTypeMap> _mediatorTypeMaps = new Dictionary<Type, MediatorTypeMap>();

        /// <summary>
        /// Mutable because we add types as new expressions are translated.
        /// </summary>
        public void AddMediatorTypeMap(MediatorTypeMap mediatorTypeMap)
            => _mediatorTypeMaps.Add(mediatorTypeMap.TargetType, mediatorTypeMap);

        internal Type GetMediatorType(Type targetType)
            => _mediatorTypeMaps[targetType].MediatorType;

        internal MemberInfo ConvertToMediator(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            var targetType = propertyInfo.ReflectedType;
            if (!_mediatorTypeMaps.TryGetValue(targetType, out var mediatorMap))
            {
                throw new InvalidOperationException($"Unable to find a mediator type map for property {targetType.Name}.{propertyInfo.Name}");
            }

            return mediatorMap.ConvertToMediator(propertyInfo);
        }

        internal bool IsTargetType(Type targetType)
        {
            return _mediatorTypeMaps.ContainsKey(targetType);
        }
    }
}