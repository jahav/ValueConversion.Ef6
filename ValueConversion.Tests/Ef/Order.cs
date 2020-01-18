namespace ValueConversion.Tests.Ef
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
