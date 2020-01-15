namespace ValueConversion.Tests
{
    public class Customer
    {
        public Id<Customer> Id { get; set; }

        internal Address DeliveryAddress { get; set; }

        internal Address WorkAddress { get; set; }
    }
}
