using System.Collections.Generic;

namespace SharpX
{
    public static class Primitives
    {
        /// <summary>Converts a value to an enumerable.</summary>
        public static IEnumerable<T> ToEnumerable<T>(T value)
        {
            Guard.DisallowNull(nameof(value), value);

            return _(); IEnumerable<T> _()
            {
                yield return value;
            }
        }
    }
}
