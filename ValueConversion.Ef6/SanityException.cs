namespace ValueConversion.Ef6
{
    using System;

    internal class SanityException : Exception
    {
        public SanityException(string message)
            : base(message)
        {
        }
    }
}
