using System;
using System.Collections.Generic;
using System.Text;

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

        /// <summary>Formats an exception to human readable text.</summary>
        public static string FormatException(Exception exception)
        {
            Guard.DisallowNull(nameof(exception), exception);

            var builder = new StringBuilder(capacity: 256)
                .AppendLine(exception.Message);
            if (exception.StackTrace != null) {
                builder.AppendLine("--- Stack trace:")
                       .AppendLine(exception.StackTrace);
            }
            if (exception.InnerException != null) {
                builder.AppendLine("--- Inner exception:")
                       .AppendLine(exception.InnerException.Message);
                if(exception.InnerException.StackTrace != null) {
                    builder.AppendLine("--- Inner exception stack trace:")
                           .AppendLine(exception.InnerException.StackTrace);
                }
            }
            return builder.ToString();
        }        
    }
}
