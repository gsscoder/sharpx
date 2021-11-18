using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpX
{
    public static class StringUtil
    {
        static readonly Random _random = new CryptoRandom();

        const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>Generates a random string of given length.</summary>
        public static string Generate(int length)
        {
            if (length < 0) throw new ArgumentException(nameof(length));
            if (length == 0) return string.Empty;

            return new string((from c in Enumerable.Repeat(_chars, length)
                               select c[_random.Next(c.Length)]).ToArray());
        }
    }

    public static class StringExtensions
    {
        static readonly Random _random = new CryptoRandom();
        static string[] _mangleChars =
            {"!", "\"", "£", "$", "%", "&", "/", "(", ")", "=", "?", "^", "[", "]", "*", "@", "°",
             "#", "§", ",", ";", ".", ":", "-", "_"};
        static Regex _stripTagsRegEx = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>Determines if a string is composed only by letter characters.</summary>
        public static bool IsAlpha(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            foreach (var @char in value.ToCharArray()) {
                if (!char.IsLetter(@char) || char.IsWhiteSpace(@char)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Determines if a string is composed only by alphanumeric characters.</summary>
        public static bool IsAlphanumeric(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            foreach (var @char in value.ToCharArray()) {
                if (!char.IsLetterOrDigit(@char) || char.IsWhiteSpace(@char)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Determines if a string is contains any kind of white spaces.</summary>
        public static bool IsWhiteSpace(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            foreach (var @char in value.ToCharArray()) {
                if (char.IsWhiteSpace(@char)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Returns a copy of this string with first letter converted to uppercase.</summary>
        public static string ToUpperFirstLetter(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Trim().Length == 0) throw new ArgumentException(nameof(value));

            return $"{char.ToUpper(value[0])}{value.Substring(1)}";
        }

        /// <summary>Returns a copy of this string with first letter converted to lowercase.</summary>
        public static string ToLowerFirstLetter(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Trim().Length == 0) throw new ArgumentException(nameof(value));

            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }

        /// <summary>Replicates a string for a given number of times using a seperator.</summary>
        public static string Replicate(this string value, int count, string separator = "")
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (count < 0) throw new ArgumentException(nameof(count));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

            var builder = new StringBuilder((value.Length + separator.Length) * count);
            for (var i = 0; i < count; i++) {
                builder.Append(value);
                builder.Append(separator);
            }
            return builder.ToString(0, builder.Length - separator.Length);
        }

        /// <summary>Applies a given function to nth-word of string.</summary>
        public static string ApplyAt(this string value, int index, Func<string, string> modifier)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (index < 0) throw new ArgumentException(nameof(index));

            var words = value.Split().ToArray();
            words[index] = modifier(words[index]);
            return string.Join(" ", words);
        }

        /// <summary>Selects a random index of a word that optionally satisfies a function.</summary>
        public static int ChoiceOfIndex(this string value, Func<string, bool> validator = null)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Func<string, bool> _nullValidator = _ => true;
            var _validator = validator ?? _nullValidator;

            var words = value.Split();
            var index = _random.Next(words.Length - 1);
            if (_validator(words[index])) {
                return index;
            }
            return ChoiceOfIndex(value, validator);
        }

        /// <summary>Mangles a string with a given number of non alphanumeric character in
        /// random positions.</summary>
        public static string Mangle(this string value, int times = 1, int maxLength = 1)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (times < 0) throw new ArgumentException(nameof(times));
            if (maxLength < 0) throw new ArgumentException(nameof(maxLength));
            if (times >= value.Length) throw new ArgumentException(nameof(times));
            if (times == 0 || maxLength == 0) return value;

            var indexes = new List<int>(times);
            int uniqueNext()
                {
                    var index = _random.Next(value.Length - 1);
                    if (indexes.Contains(index)) {
                        return uniqueNext();
                    }
                    return index;
                };
            for (var i = 0; i < times; i++) {
                indexes.Add(uniqueNext());
            }
            var mutations = indexes.OrderBy(index => index);

            var mangled = new StringBuilder(value.Length + times * maxLength);
            for (var i = 0; i < value.Length; i++) {
                mangled.Append(value[i]);
                if (mutations.Contains(i)) {
                    mangled.Append(
                        _mangleChars[_random.Next(_mangleChars.Length - 1)]
                        .Replicate(maxLength, string.Empty));
                    
                }
            }
            return mangled.ToString();
        }

        /// <summary>Takes a value and a string and `intersperses' that value between its words.</summary>
        public static string Intersperse(this string value, params object[] values)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (values.Length == 0) return value;

            var builder = new StringBuilder(value.Length + values.Length * 8);
            var words = value.Split();
            var count = words.Length;
            var last = count - 1;
            for (var i = 0; i < count; i++) {
                builder.Append(words[i]);
                builder.Append(' ');
                if (i >= values.Length) continue;
                var element = values[i];
                builder.Append(element);
                builder.Append(' ');
            }
            return builder.ToString().TrimEnd();
        }

        /// <summary>Sanitizes a string removing non alphanumeric characters and optionally normalizing
        /// white spaces.</summary>
        public static string Sanitize(this string value, bool normalizeWhiteSpace = true)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var builder = new StringBuilder(value.Length);
            foreach (var @char in value) {
                if (char.IsLetterOrDigit(@char)) {
                    builder.Append(@char);
                }
                else if (char.IsWhiteSpace(@char)) {
                    if (normalizeWhiteSpace) {
                        builder.Append(' ');
                    } else {
                        builder.Append(@char);
                    }
                }
            }
            return builder.ToString();
        }
        
        /// <summary>Normalizes any white space character to a single white space.</summary>
        public static string NormalizeWhiteSpace(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var trimmed = value.Trim();
            var builder = new StringBuilder(trimmed.Length);
            var lastIndex = trimmed.Length - 2;
            for (var i = 0; i < trimmed.Length; i++) {
                var @char = trimmed[i];
                if (char.IsWhiteSpace(@char)) {
                    if (i != lastIndex && !char.IsWhiteSpace(trimmed[i + 1])) {
                        builder.Append(' ');
                    }
                }
                else {
                    builder.Append(@char);
                }
            }
            return builder.ToString();
        }

        /// <summary>Removes tags from a string.</summary>
        public static string StripTags(this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return _stripTagsRegEx.Replace(value, string.Empty);
        }

        /// <summary>Removes words of given length.</summary>
        public static string StripByLength(this string value, int length)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (length < 0) throw new ArgumentException(nameof(length));
            if (length == 0) return value;

            var stripByLen = new Regex(
                string.Concat(@"\b\w{1,", length, @"}\b"),
                RegexOptions.Compiled | RegexOptions.Multiline);
            return stripByLen.Replace(value, string.Empty);
        }

        /// <summary>Reduces a sequence of strings to a sequence of parts, splitted by space,
        /// of each original string.</summary>
        public static IEnumerable<string> FlattenOnce(this IEnumerable<string> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return _(); IEnumerable<string> _()
            {
                foreach (var element in source) {
                    var parts = element.Split();
                    foreach (var part in parts) {
                        yield return part;
                    }
                }
            }
        }
    }
}
