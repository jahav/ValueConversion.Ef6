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
        {
            if (!_mediatorTypeMaps.TryGetValue(targetType, out var value))
            {
                throw new KeyNotFoundException($"Unable to find a mediator type for {targetType}. Make sure it was found by the searcher and a mediator type was created.");
            }

            return value.MediatorType;
        }

        internal MemberInfo ConvertToMediator(PropertyInfo targetProperty)
        {
            var targetType = targetProperty.ReflectedType;
            if (!_mediatorTypeMaps.TryGetValue(targetType, out var mediatorMap))
            {
                throw new InvalidOperationException($"Unable to find a mediator type map for property {targetType.Name}.{targetProperty.Name}");
            }

            return mediatorMap.ConvertToMediator(targetProperty);
        }

        internal bool IsTargetType(Type targetType)
        {
            return _mediatorTypeMaps.ContainsKey(targetType);
        }
    }
}