namespace ValueConversion.Tests
{
    public struct PhoneNumber
    {
        private PhoneNumber(string number)
        {
            Number = number;
        }

        public string Number { get; }

        public static explicit operator PhoneNumber(string number) => new PhoneNumber(number);
    }
}