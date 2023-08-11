using System.Text;
#if NETCOREAPP31
using RandomNumberGenerator = SharpX._RandomNumberGeneratorCompatibility;
#else
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;
#endif


namespace SharpX;

public static class Primitives
{
    /// <summary>Converts a value to an enumerable.</summary>
    public static IEnumerable<T> ToEnumerable<T>(T value)
    {
        Guard.DisallowNull(nameof(value), value);

        return new T[] { value };
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

    public static bool ChanceOf(int thresold)
    {
        Guard.DisallowNegative(nameof(thresold), thresold);

        return thresold > 0
            ? RandomNumberGenerator.GetInt32(0, 100) <= thresold
            : false;
    }
}
