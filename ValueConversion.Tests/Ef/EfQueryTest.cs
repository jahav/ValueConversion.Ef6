namespace ValueConversion.Tests.Ef
{
    using System.Collections.Generic;
    using System.Data.SQLite;
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
                }
            }
        }
    }
}
