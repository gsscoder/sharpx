using System.Text;
using RandomNumberGenerator = SharpX._RandomNumberGeneratorCompatibility;

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

    /// <summary>Returns true if the chance randomly occurred.</summary>
    public static bool ChanceOf(int thresold)
    {
        Guard.DisallowNegative(nameof(thresold), thresold);

        return thresold > 0
            ? RandomNumberGenerator.GetInt32(0, 100) <= thresold
            : false;
    }

    /// <summary>Generates a random sequence from a generator function.</summary>
    public static IEnumerable<T> GenerateSeq<T>(Func<T> generator, int? count = null)
    {
        if (count != null) Guard.DisallowNegative(nameof(count), count.Value);

        var count_ = count ?? 0;

        while (count == null || count_-- > 0) {
            yield return generator();
        }
    }

    /// <summary>Generates a random sequence of int, double or string types.</summary>
    public static IEnumerable<T> GenerateSeq<T>(int? count = null)
    {
        if (count != null) Guard.DisallowNegative(nameof(count), count.Value);

        Func<object> generator = typeof(T) switch {
            var t when t == typeof(int) => () => RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue),
            var t when t == typeof(double) => () => BitConverter.ToDouble(RandomNumberGenerator.GetBytes(8), 0),
            var t when t == typeof(string) => () => Strings.Generate(length: 16),
            _ => throw new ArgumentException($"{typeof(T).Name} is not supported"),
        };

        var count_ = count ?? 0;

        while (count == null || count_-- > 0) {
            yield return (T)generator();
        }
    }

    /// <summary>Returns true in case of a numeric type value, otherwise false.</summary>
    public static bool IsNumber<T>(T? value)
    {
        if (value == null) return false;

        return value is sbyte ||
               value is byte ||
               value is short ||
               value is ushort ||
               value is int ||
               value is uint ||
               value is long ||
               value is ulong ||
               value is float ||
               value is double ||
               value is decimal;
    }
}
