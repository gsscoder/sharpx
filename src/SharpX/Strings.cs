using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpX
{
    public static class Strings
    {
        const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static char[] _mangleChars =
            {'!', '"', '£', '$', '%', '&', '/', '(', ')', '=', '?', '^', '[', ']', '*', '@', '°',
             '#', '§', ',', ';', '.', ':', '-', '_'};
        static readonly Random _random = new CryptoRandom();
        static Regex _stripTagRegEx = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>Replicates a character for a given number of times using a seperator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplicateChar(char value, int count, string separator = "")
        {
            Guard.DisallowNegative(nameof(count), count);
            Guard.DisallowNull(nameof(separator), separator);

            if (separator.Length == 0) return new string(value, count);

            var builder = new StringBuilder((1 + separator.Length) * count);
            for (var i = 0; i < count; i++) {
                builder.Append(value)
                       .Append(separator);
            }
            return builder.ToString(0, builder.Length - separator.Length);
        }

        /// <summary>Determines if a character is special op not.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSpecialChar(char value) =>
            !char.IsLetterOrDigit(value) && !char.IsWhiteSpace(value);

        /// <summary>Generates a random string of given length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Generate(int length)
        {
            Guard.DisallowNegative(nameof(length), length);

            if (length == 0) return string.Empty;

            return new string((from c in Enumerable.Repeat(_chars, length)
                               select c[_random.Next(c.Length)]).ToArray());
        }

        /// <summary>Determines if a string is composed only by letter characters.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            foreach (var @char in value.ToCharArray()) {
                if (!char.IsLetter(@char) || char.IsWhiteSpace(@char)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Determines if a string is composed only by alphanumeric characters.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphanumeric(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            foreach (var @char in value.ToCharArray()) {
                if (!char.IsLetterOrDigit(@char) || char.IsWhiteSpace(@char)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Determines if a string is contains any kind of white spaces.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsWhitespace(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            foreach (var @char in value.ToCharArray()) {
                if (char.IsWhiteSpace(@char)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Determines if a string contains any special character.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsSpecialChar(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            if (value.Trim().Length == 0) return false;

            foreach (var @char in value.ToCharArray()) {
                if (IsSpecialChar(@char)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Returns a copy of this string with first letter converted to uppercase.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUpperFirst(string value)
        {
            Guard.DisallowNull(nameof(value), value);
            
            if (ContainsWhitespace(value)) return value;
            if (value.Length == 1) return value.ToUpper();

            return $"{char.ToUpper(value[0])}{value.Substring(1)}";
        }

        /// <summary>Returns a copy of this string with first letter converted to lowercase.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLowerFirst(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            if (ContainsWhitespace(value)) return value;
            if (value.Length == 1) return value.ToLower();

            return $"{char.ToLower(value[0])}{value.Substring(1)}";
        }

        /// <summary>Replicates a string for a given number of times using a seperator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Replicate(string value, int count, string separator = "")
        {
            Guard.DisallowNull(nameof(value), value);
            Guard.DisallowNegative(nameof(count), count);
            Guard.DisallowNull(nameof(separator), separator);

            var builder = new StringBuilder((value.Length + separator.Length) * count);
            for (var i = 0; i < count; i++) {
                builder.Append(value);
                builder.Append(separator);
            }
            return builder.ToString(0, builder.Length - separator.Length);
        }

        /// <summary>Applies a given function to nth-word of string.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ApplyAt(string value, int index, Func<string, string> modifier)
        {
            Guard.DisallowNull(nameof(value), value);
            Guard.DisallowNegative(nameof(index), index);

            var words = value.Split().ToArray();
            words[index] = modifier(words[index]);
            return string.Join(" ", words);
        }

        /// <summary>Selects a random index of a word that optionally satisfies a function.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ChoiceOfIndex(string value, Func<string, bool> validator = null)
        {
            Guard.DisallowNull(nameof(value), value);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Mangle(string value, int times = 1, int maxLength = 1)
        {
            Guard.DisallowNull(nameof(value), value);
            Guard.DisallowNegative(nameof(times), times);
            Guard.DisallowNegative(nameof(maxLength), maxLength);
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
                    mangled.Append(ReplicateChar(
                        _mangleChars[_random.Next(_mangleChars.Length - 1)],
                        maxLength, string.Empty));
                    
                }
            }
            return mangled.ToString();
        }

        /// <summary>Takes a value and a string and `intersperses' that value between its words.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Intersperse(string value, params object[] values)
        {
            Guard.DisallowNull(nameof(value), value);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sanitize(string value, bool normalizeWhiteSpace = true)
        {
            Guard.DisallowNull(nameof(value), value);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NormalizeWhiteSpace(string value)
        {
            Guard.DisallowNull(nameof(value), value);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StripTag(string value)
        {
            Guard.DisallowNull(nameof(value), value);

            return _stripTagRegEx.Replace(value, string.Empty);
        }

        /// <summary>Removes words of given length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StripByLength(string value, int length)
        {
            Guard.DisallowNull(nameof(value), value);

            if (length < 0) throw new ArgumentException(nameof(length));
            if (length == 0) return value;

            var stripByLen = new Regex(
                string.Concat(@"\b\w{1,", length, @"}\b"),
                RegexOptions.Compiled | RegexOptions.Multiline);
            return stripByLen.Replace(value, string.Empty);
        }

        /// <summary>Reduces a sequence of strings to a sequence of parts, splitted by space,
        /// of each original string.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> FlattenOnce(IEnumerable<string> source)
        {
            Guard.DisallowNull(nameof(source), source);

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
