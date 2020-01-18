namespace ValueConversion.Tests.Ef
{
    using System.Data.Common;
    using System.Data.Entity;
    using SQLite.CodeFirst;

    public partial class EfQueryTest
    {
        public class OrderContext : DbContext
        {
            public OrderContext(DbConnection connection)
                : base(connection, false)
            {
            }

            public DbSet<Order> Orders { get; set; }

            public DbSet<OrderItem> OrderItems { get; set; }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                Database.SetInitializer(new Initializer(modelBuilder));
            }

            private class Initializer : SqliteDropCreateDatabaseWhenModelChanges<OrderContext>
            {
                public Initializer(DbModelBuilder modelBuilder)
                    : base(modelBuilder)
                {
                }

                public override void InitializeDatabase(OrderContext context)
                {
                    base.InitializeDatabase(context);
                }
            }
        }
    }
}
