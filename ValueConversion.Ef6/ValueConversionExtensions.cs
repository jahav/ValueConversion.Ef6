namespace ValueConversion.Ef6
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static class ValueConversionExtensions
    {
        /// <summary>
        /// The extension method that adds a select as a last extension method and hydrates the result through proxy.
        /// </summary>
        /// <typeparam name="TSource">Type of the source object from the <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type that <paramref name="selectExpression"/> transforms the <paramref name="source"/> into.</typeparam>
        public static List<TResult> ProjectToList<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selectExpression, MediatorMapper mediatorMapper)
        {
            var selectSourceToTarget = selectExpression;

            // TODO: Caching
            var targetToMediatorVisitor = new TargetToMediatorVisitor(mediatorMapper);
            var sourceToMediatorSelect = targetToMediatorVisitor.Visit(selectSourceToTarget);

            var targetRootType = typeof(TResult);
            var mediatorRootType = mediatorMapper.GetMediatorType(targetRootType);
            var mediatorList = SelectToList(source, sourceToMediatorSelect, mediatorRootType);

            var mediatorToTargetVisitor = new MediatorToTargetVisitor(mediatorMapper);
            object mediatorToTargetSelect = mediatorToTargetVisitor.Visit(selectSourceToTarget);

            object mediatorToTargetFunc = MethodHelper.GetCompile(mediatorRootType, targetRootType)
                .Invoke(mediatorToTargetSelect, Array.Empty<object>());

            var genericEnumerableSelect = MethodHelper.GetEnumerableSelect(mediatorRootType, targetRootType);

            var targetEnumerable = (IEnumerable<TResult>)genericEnumerableSelect.Invoke(null, new object[] { mediatorList, mediatorToTargetFunc });
            var targetList = targetEnumerable.ToList();
            return targetList;
        }

        private static IList SelectToList<T>(IQueryable<T> query, Expression mediatorSelect, Type mediatorRootType)
        {
            var sourceType = typeof(T);
            var resultType = mediatorRootType;

            var genericSelectMethod = MethodHelper.GetQueryableSelect(sourceType, resultType);
            var mediatorSelectQuery = genericSelectMethod.Invoke(null, new object[] { query, mediatorSelect });

            var toListMethod = MethodHelper.GetGenericToListMethod();
            var genericToListMethod = toListMethod.MakeGenericMethod(resultType);
            var mediatorList = (IList)genericToListMethod.Invoke(null, new object[] { mediatorSelectQuery });
            return mediatorList;
        }
    }
}
