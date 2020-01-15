namespace ValueConversion.Tests
{
    using System.Collections.Generic;

    public class Customer
    {
        public Customer()
        {
        }

        public Customer(int id)
        {
            Id = (Id<Customer>)id;
        }

        public Id<Customer> Id { get; set; }

        public Address DeliveryAddress { get; set; }

        public Address WorkAddress { get; set; }

        public ICollection<Contact> PhoneNumbers { get; set; }
    }
}
