namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MediatorMapper
    {
        /// <summary>
        /// Key is target type.
        /// </summary>
        private readonly Dictionary<Type, MediatorTypeMap> _mediatorTypeMaps;

        public MediatorMapper(IEnumerable<MediatorTypeMap> mediatorTypeMaps)
        {
            _mediatorTypeMaps = mediatorTypeMaps.ToDictionary(x => x.TargetType, x => x);
        }

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