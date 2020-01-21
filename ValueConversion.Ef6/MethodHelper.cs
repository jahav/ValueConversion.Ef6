namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class MethodHelper
    {
        private static readonly Lazy<MethodInfo> _queryableSelect = new Lazy<MethodInfo>(GetGenericQueryableSelectMethod, true);
        private static readonly Lazy<MethodInfo> _enumerableSelect = new Lazy<MethodInfo>(GetGenericEnumerableSelectMethod, true);
        private static readonly Lazy<MethodInfo> _toList = new Lazy<MethodInfo>(() => typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)), true);

        internal static MethodInfo GetQueryableSelect(Type sourceType, Type targetType)
        {
            return _queryableSelect.Value.MakeGenericMethod(sourceType, targetType);
        }

        internal static MethodInfo GetEnumerableSelect(Type mediatorRootType, Type targetRootType)
        {
            return _enumerableSelect.Value.MakeGenericMethod(mediatorRootType, targetRootType);
        }

        internal static MethodInfo GetCompile(Type sourceType, Type targetType)
        {
            var mediatorToFuncType = typeof(Func<,>).MakeGenericType(sourceType, targetType);
            var mediatorToFuncTypeExpression = typeof(Expression<>).MakeGenericType(mediatorToFuncType);
            return mediatorToFuncTypeExpression.GetMethod("Compile", Type.EmptyTypes);
        }

        internal static MethodInfo GetGenericToListMethod() => _toList.Value;

        private static IEnumerable<MethodInfo> GetSelectWithTwoParameters(Type staticType)
        {
            return staticType
                   .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "Select")
                .Where(m =>
                {
                    if (!m.IsGenericMethod)
                    {
                        return false;
                    }

                    var genericArguments = m.GetGenericArguments();
                    if (genericArguments.Length != 2)
                    {
                        return false;
                    }

                    var methodParameters = m.GetParameters();
                    if (methodParameters.Length != 2)
                    {
                        return false;
                    }

                    return true;
                });
        }

        private static MethodInfo GetGenericQueryableSelectMethod()
        {
            return GetSelectWithTwoParameters(typeof(Queryable))
                .Where(m =>
                {
                    var sourceType = m.GetGenericArguments()[0];
                    var resultType = m.GetGenericArguments()[1];
                    var funcType = typeof(Func<,>).MakeGenericType(sourceType, resultType);
                    var expressionType = typeof(Expression<>).MakeGenericType(funcType);

                    if (m.GetParameters()[1].ParameterType != expressionType)
                    {
                        return false;
                    }

                    return true;
                }).Single();
        }

        private static MethodInfo GetGenericEnumerableSelectMethod()
        {
            return GetSelectWithTwoParameters(typeof(Enumerable))
                .Where(m =>
                {
                    var sourceType = m.GetGenericArguments()[0];
                    var resultType = m.GetGenericArguments()[1];
                    var funcType = typeof(Func<,>).MakeGenericType(sourceType, resultType);

                    if (m.GetParameters()[1].ParameterType != funcType)
                    {
                        return false;
                    }

                    return true;
                }).Single();
        }
    }
}
