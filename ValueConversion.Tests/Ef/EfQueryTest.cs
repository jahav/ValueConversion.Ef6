namespace ValueConversion.Tests.Ef
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SQLite;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public partial class EfQueryTest
    {
        [Fact]
        public void Select()
        {
            using (var connection = new SQLiteConnection("data source=:memory:"))
            {
                connection.Open();
                using (var ctx = new OrderContext(connection))
                {
                    var order = new Order
                    {
                        Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Product = new Product { Name = "Diapers", Cost = 10.0m },
                            Quantity = 5,
                        },
                        new OrderItem
                        {
                            Product = new Product { Name = "Baby formula", Cost = 50m },
                            Quantity = 3,
                        },
                    },
                    };
                    ctx.Orders.Add(order);
                    ctx.SaveChanges();

                    Expression<Func<OrderItem, object>> select = x => new A
                    {
                        Quantity = x.Quantity,
                    };

                    var config = new ConversionConfiguration()
                    {
                        IsAllowedForColumn = x => x.IsValueType || x == typeof(string),
                    };
                    var searcher = new GraphSearcher(config);
                    var graph = searcher.SearchGraph(typeof(A));

                    var mediatorMapper = new MediatorTypeBuilder().CreateMediatorTypes(graph);

                    var targetToMediatorVisitor = new TargetToMediatorVisitor(mediatorMapper);
                    var mediatorSelect = targetToMediatorVisitor.Visit(select);

                    var mediatorList = SelectToList(ctx.OrderItems, mediatorSelect, mediatorMapper.GetMediatorType(typeof(A)));
                    mediatorList.Should().HaveCount(2);
                }
            }
        }

        private static MethodInfo GetGenericToListMethod() => typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));

        private static MethodInfo GetGenericSelectMethod()
        {
            return typeof(Queryable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == nameof(Queryable.Select))
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

                    var sourceType = m.GetGenericArguments()[0];
                    var resultType = m.GetGenericArguments()[1];
                    var funcType = typeof(Func<,>).MakeGenericType(sourceType, resultType);
                    var expressionType = typeof(Expression<>).MakeGenericType(funcType);

                    if (methodParameters[1].ParameterType != expressionType)
                    {
                        return false;
                    }

                    return true;
                }).Single();
        }

        private IList SelectToList<T>(DbSet<T> query, Expression mediatorSelect, Type mediatorRootType)
            where T : class
        {
            var sourceType = typeof(T);
            var resultType = mediatorRootType;

            var genericSelectMethod = GetGenericSelectMethod().MakeGenericMethod(sourceType, resultType);
            var mediatorSelectQuery = genericSelectMethod.Invoke(null, new object[] { query, mediatorSelect });

            var toListMethod = GetGenericToListMethod();
            var genericToListMethod = toListMethod.MakeGenericMethod(resultType);
            var mediatorList = (IList)genericToListMethod.Invoke(null, new object[] { mediatorSelectQuery });
            return mediatorList;
        }

        public class A
        {
            public int Quantity { get; set; }
        }
    }
}
