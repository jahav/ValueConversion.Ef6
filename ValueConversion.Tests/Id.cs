namespace ValueConversion.Tests
{
    public struct Id<T>
        where T : class
    {
        private Id(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static explicit operator Id<T>(int value) => new Id<T>(value);
    }
}
