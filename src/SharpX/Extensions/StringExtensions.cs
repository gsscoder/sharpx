using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SharpX.Extensions
{
    public static class StringExtensions
    {
        static readonly Random _random = new CryptoRandom();
        static string[] _mangleChars =
            {"!", "\"", "£", "$", "%", "&", "/", "(", ")", "=", "?", "^", "[", "]", "*", "@", "°",
             "#", "§", ",", ";", ".", ":", "-", "_"};
        static Regex _stripTagRegEx = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>Determines whether the beginning of this string instance matches the specified string in a case insensitive way.</summary>
        public static bool StartsWithIgnoreCase(this string source, string value) =>
            source.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        /// <summary>Determines whether two String objects have the same value in a case insensitive way.</summary>
        public static bool EqualsIgnoreCase(this string source, string value) =>
            source.Equals(value, StringComparison.OrdinalIgnoreCase);

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

        /// <summary>Determines if a string is empty or composed only by white spaces.</summary>
        public static bool IsEmptyWhitespace(this string value) => Strings.IsEmptyWhitespace(value);

        /// <summary>Determines if a string is contains any kind of white spaces.</summary>
        public static bool ContainsWhitespace(this string value) => Strings.ContainsWhitespace(value);

        /// <summary>Returns a copy of this string with first letter converted to uppercase.</summary>
        public static string ToUpperFirst(this string value) =>
            Strings.ToUpperFirst(value);

        /// <summary>Returns a copy of this string with first letter converted to lowercase.</summary>
        public static string ToLowerFirst(this string value) =>
            Strings.ToLowerFirst(value);

        /// <summary>Replicates a string for a given number of times using a seperator.</summary>
        public static string Replicate(this string value, int count, string separator = "") =>
            Strings.Replicate(value, count, separator);

        /// <summary>Applies a given function to nth-word of string.</summary>
        public static string ApplyAt(this string value, int index, Func<string, string> modifier) =>
            Strings.ApplyAt(value, index, modifier);

        /// <summary>Selects a random index of a word that optionally satisfies a function.</summary>
        public static int ChoiceOfIndex(this string value, Func<string, bool> validator = null) =>
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

        /// <summary>Removes tags from a string.</summary>
        public static string StripTag(this string value) =>
            _stripTagRegEx.Replace(value, "");

        /// <summary>Removes words of given length.</summary>
        public static string StripByLength(this string value, int length) =>
            Strings.StripByLength(value, length);

        /// <summary>Retrieves a substring from this instance. The substring starts at a specified
        /// character position and has a specified length. No exception is raised if limit is exceeded.</summary>
        public static string SafeSubstring(this string value, int startIndex, int length) =>
            Strings.SafeSubstring(value, startIndex, length);

        /// <summary>Reduces a sequence of strings to a sequence of parts, splitted by space,
        /// of each original string.</summary>
        public static IEnumerable<string> FlattenOnce(this IEnumerable<string> source) =>
            Strings.FlattenOnce(source);
    }
}
