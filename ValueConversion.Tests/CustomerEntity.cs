namespace ValueConversion.Tests
{
    using System.Collections.Generic;

    public class CustomerEntity
    {
        public int Id { get; set; }

        public ICollection<PhoneNumberEntity> PhoneNumbers { get; set; }
    }
}
