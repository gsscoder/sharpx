using System;
using System.Text;

namespace SharpX
{
    public static class CharExtensions
    {
        /// <summary>Replicates a character for a given number of times using a seperator.</summary>
        public static string Replicate(this char value, int count, string separator = "")
        {
            if (count < 0) throw new ArgumentException(nameof(count));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

            if (separator.Length == 0) return new string(value, count);

            var builder = new StringBuilder((1 + separator.Length) * count);
            for (var i = 0; i < count; i++) {
                builder.Append(value);
                builder.Append(separator);
            }
            return builder.ToString(0, builder.Length - separator.Length);
        }
    }
}
