using System;
using System.Text;

namespace SharpX.Extensions
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception exception)
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
