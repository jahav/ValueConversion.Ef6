namespace ValueConversion.Ef6
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A class for holding and looking up information what type was converted into which type in the expression.
    /// </summary>
    internal class CastMap
    {
        private readonly Dictionary<Type, HashSet<Type>> fromTo = new Dictionary<Type, HashSet<Type>>();

        public void AddFromTo(Type from, Type to)
        {
            if (!fromTo.TryGetValue(from, out var key))
            {
                key = new HashSet<Type>();
                fromTo.Add(from, key);
            }

            key.Add(to);
        }
    }
}