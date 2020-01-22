namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Data.Spatial;

    internal static class TypeHelper
    {
        private static readonly ISet<Type> _types = new HashSet<Type>()
            {
                typeof(bool),
                typeof(byte),
                typeof(DateTime),
                typeof(decimal),
                typeof(double),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(float),
                typeof(string),
                typeof(byte[]),
                typeof(DateTimeOffset),
                typeof(Guid),
                typeof(TimeSpan),
            };

        internal static bool MemberTypeSupportedByEf(Type memberType)
        {
            Type type = memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? memberType.GetGenericArguments()[0]
                : memberType;

            if (type.IsEnum)
            {
                return true;
            }

            // EF is using TypeCode switch for better performance, but whatever
            if (_types.Contains(type))
            {
                return true;
            }
            else if (typeof(DbGeography).IsAssignableFrom(type))
            {
                return true;
            }
            else if (typeof(DbGeometry).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }
    }
}
