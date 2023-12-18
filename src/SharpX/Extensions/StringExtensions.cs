using System.Text.RegularExpressions;

namespace SharpX.Extensions;

public static class StringExtensions
{
    static Regex _stripTagRegEx = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>Determines whether the beginning of this string instance matches the specified string in a case insensitive way.</summary>
    public static bool StartsWithIgnoreCase(this string source, string value) =>
        source.StartsWith(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>Determines whether two String objects have the same value in a case insensitive way.</summary>
    public static bool EqualsIgnoreCase(this string? source, string value, bool safe = false)
    {
        if (!safe) Guard.DisallowNull(nameof(source), source);
        else if (safe && source == null && value != null) return false;
        else if (safe && source == null && value == null) return true;

        return source.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Determines whether a specified substring occurs within this string in a case insensitive way.</summary>
    public static bool ContainsIgnoreCase(this string source, string value) =>
        source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>Replicates a character for a given number of times using a seperator.</summary>
    public static string Replicate(this char value, int count, string separator = "") =>
        Strings.ReplicateChar(value, count, separator);

    /// <summary>Determines if a character is special op not.</summary>
    public static bool IsSpecialChar(this char value) => Strings.IsSpecialChar(value);

    /// <summary>Determines if a string is composed only by letter characters.</summary>
    public static bool IsAlpha(this string value) => Strings.IsAlpha(value);

    /// <summary>Determines if a string is composed only by alphanumeric characters.</summary>
    public static bool IsAlphanumeric(this string value) => Strings.IsAlphanumeric(value);

    /// <summary>Determines if a string is null, empty or composed only by white space.</summary>
    public static bool IsEmpty(this string? value) => String.IsNullOrWhiteSpace(value);

    /// <summary>Determines if a string is empty or composed only by white spaces.</summary>
    public static bool IsEmptyWhitespace(this string value) => Strings.IsEmptyWhitespace(value);

    /// <summary>Determines if a string is contains any kind of white spaces.</summary>
    public static bool ContainsWhitespace(this string value) => Strings.ContainsWhitespace(value);

    /// <summary>Returns a copy of this string with first letter converted to uppercase.</summary>
    public static string ToUpperFirst(this string value) =>
        Strings.ToUpperFirst(value);

    /// <summary>Returns a copy of this string with first letter converted to lowercase.</summary>
    public static string ToLowerFirst(this string value) =>
        Strings.ToLowerFirst(value);

    /// <summary>Creates a Guid from a given string or default if safe is set.</summary>
    public static Guid ToGuid(this string value, bool safe = false)
    {
        if (!safe) return new(value);
        return Guid.TryParse(value, out var result)
            ? result
            : default;
    }

    /// <summary>Replicates a string for a given number of times using a seperator.</summary>
    public static string Replicate(this string value, int count, string separator = "") =>
        Strings.Replicate(value, count, separator);

    /// <summary>Applies a given function to nth-word of string.</summary>
    public static string ApplyAt(this string value, int index, Func<string, string> modifier) =>
        Strings.ApplyAt(value, index, modifier);

    /// <summary>Selects a random index of a word that optionally satisfies a function.</summary>
    public static int ChoiceOfIndex(this string value, Func<string, bool>? validator = null) =>
        Strings.ChoiceOfIndex(value, validator);

    /// <summary>Mangles a string with a given number of non alphanumeric character in
    /// random positions.</summary>
    public static string Mangle(this string value, int times = 1, int maxLength = 1) =>
        Strings.Mangle(value, times, maxLength);

    /// <summary>Takes a value and a string and `intersperses' that value between its words.</summary>
    public static string Intersperse(this string value, params object[] values) =>
        Strings.Intersperse(value, values);

    /// <summary>Sanitizes a string removing non alphanumeric characters and optionally normalizing
    /// white spaces.</summary>
    public static string Sanitize(this string value, bool normalizeWhiteSpace = true) =>
        Strings.Sanitize(value, normalizeWhiteSpace);

    /// <summary>Normalizes any white space character to a single white space.</summary>
    public static string NormalizeWhiteSpace(this string value) =>
        Strings.NormalizeWhiteSpace(value);

    // <summary>Normalizes a null or white space string to empty.</summary>
    public static string NormalizeToEmpty(string value) =>
        Strings.NormalizeToEmpty(value);

    /// <summary>Removes tags from a string.</summary>
    public static string StripTag(this string value) =>
        _stripTagRegEx.Replace(value, "");

    /// <summary>Removes words of given length.</summary>
    public static string StripByLength(this string value, int length) =>
        Strings.StripByLength(value, length);

    /// <summary>Retrieves a substring from this instance. The substring starts at a specified
    /// character position and has a specified length. No exception is raised if limit is exceeded.</summary>
    public static string Substring(this string value, int startIndex, int length, bool safe = false) =>
        Strings.Substring(value, startIndex, length, safe);

    /// <summary>Reduces a sequence of strings to a sequence of parts, splitted by space,
    /// of each original string.</summary>
    public static IEnumerable<string> FlattenOnce(this IEnumerable<string> source) =>
        Strings.FlattenOnce(source);

    /// <summary>Convenience extension method to create a new Uri from a string.</summary>
    public static Uri? ToUri(this string value, bool safe = false)
    {
        if (!safe) return new(value);
        try {
            return new(value);
        }
        catch {
            return default;
        }
    }

    /// <summary>Reverses tha case of a character.</summary>
    public static char ReverseCase(this char value) => Strings.ReverseCase(value);

    /// <summary>Randomizes the case of a string characters.</summary>
    public static string RandomizeCase(this string value) => Strings.RandomizeCase(value);
}
