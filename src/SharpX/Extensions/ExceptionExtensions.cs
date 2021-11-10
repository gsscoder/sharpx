using System;
using System.Text;

namespace SharpX
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception exception)
        {
            var builder = new StringBuilder(capacity: 256);
            builder.AppendLine(exception.Message);
            if (exception.StackTrace != null) {
                builder.AppendLine("--- Stack trace:");
                builder.AppendLine(exception.StackTrace);
            }
            if (exception.InnerException != null) {
                builder.AppendLine("--- Inner exception:");
                builder.AppendLine(exception.InnerException.Message);
                if(exception.InnerException.StackTrace != null) {
                    builder.AppendLine("--- Inner exception stack trace:");
                    builder.AppendLine(exception.InnerException.StackTrace);
                }
            }
            return builder.ToString();
        }
    }
}
