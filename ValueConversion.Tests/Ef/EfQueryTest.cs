namespace ValueConversion.Tests.Ef
{
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using FluentAssertions;
    using ValueConversion.Ef6;
    using Xunit;

    public partial class EfQueryTest
    {
        [Fact]
        public void SelectWithNamedType_IsProperlyMaterialized()
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

                    var config = new ConversionConfiguration()
                    {
                        IsAllowedForColumn = x => x.IsValueType || x == typeof(string),
                    };
                    var searcher = new GraphSearcher(config);
                    var graph = searcher.SearchGraph(typeof(NamedTarget));
                    var mediatorMapper = new MediatorTypeBuilder().CreateMediatorTypes(graph);
                    var result = ctx.OrderItems.ProjectToList(
                        x => new NamedTarget
                        {
                            Quantity = x.Quantity,
                        },
                        mediatorMapper);
                    result.Select(x => x.Quantity).Should().BeEquivalentTo(3, 5);
                }
            }
        }

        [Fact]
        public void SelectWithMultiplePropertyChain_IsProperlyMaterialized()
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

                    var config = new ConversionConfiguration()
                    {
                        IsAllowedForColumn = x => x.IsValueType || x == typeof(string),
                    };
                    var searcher = new GraphSearcher(config);
                    var graph = searcher.SearchGraph(typeof(B));
                    var mediatorMapper = new MediatorTypeBuilder().CreateMediatorTypes(graph);
                    var result = ctx.OrderItems.ProjectToList(
                        x => new B
                        {
                            ProductCost = x.Product.Cost,
                        },
                        mediatorMapper);
                    result.Select(x => x.ProductCost).Should().BeEquivalentTo(10m, 50m);
                }
            }
        }

        private class NamedTarget
        {
            public int Quantity { get; set; }
        }

        private class B
        {
            public decimal ProductCost { get; set; }
        }
    }
}
