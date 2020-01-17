namespace ValueConversion.Ef6
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)
        {
            return source.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
