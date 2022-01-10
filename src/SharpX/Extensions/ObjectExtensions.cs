using System.Collections.Generic;

namespace SharpX.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>Converts a value to an enumerable.</summary>
        public static IEnumerable<T> ToEnumerable<T>(this T value) => Primitives.ToEnumerable(value);
    }
}
