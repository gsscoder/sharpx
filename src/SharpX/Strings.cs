using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using SharpX.Extensions;
using RandomNumberGenerator = SharpX._RandomNumberGeneratorCompatibility;

namespace SharpX;

public sealed class GenerateOptions
{
    public bool AllowSpecialChars { get; set; }

    public bool AllowQuoteChars { get; set; }
}

public static class Strings
{
    private const string _alpahNumChars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _specialChars = "!#$%&()*+,-./:;<=>?@[\\]^_{|}~¡¢£¤¥¦§¨©«¬®¯°±²³¶·¹»¼½¾¿÷";
    private const string _quotesChars = "\"'`";
    private static Regex _matchTagRegEx = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);
    #region Diatritics map
    private static Dictionary<string, string> _diatritics = new()
    {
        { "äæǽ", "ae" },
        { "öœ", "oe" },
        { "ü", "ue" },
        { "Ä", "Ae" },
        { "Ü", "Ue" },
        { "Ö", "Oe" },
        { "ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A" },
        { "àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a" },
        { "Б", "B" },
        { "б", "b" },
        { "ÇĆĈĊČ", "C" },
        { "çćĉċč", "c" },
        { "Д", "D" },
        { "д", "d" },
        { "ÐĎĐΔ", "Dj" },
        { "ðďđδ", "dj" },
        { "ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E" },
        { "èéêëēĕėęěέεẽẻẹềếễểệеэ", "e" },
        { "Ф", "F" },
        { "ф", "f" },
        { "ĜĞĠĢΓГҐ", "G" },
        { "ĝğġģγгґ", "g" },
        { "ĤĦ", "H" },
        { "ĥħ", "h" },
        { "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I" },
        { "ìíîïĩīĭǐįıηήίιϊỉịиыї", "i" },
        { "Ĵ", "J" },
        { "ĵ", "j" },
        { "ĶΚК", "K" },
        { "ķκк", "k" },
        { "ĹĻĽĿŁΛЛ", "L" },
        { "ĺļľŀłλл", "l" },
        { "М", "M" },
        { "м", "m" },
        { "ÑŃŅŇΝН", "N" },
        { "ñńņňŉνн", "n" },
        { "ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O" },
        { "òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o" },
        { "П", "P" },
        { "п", "p" },
        { "ŔŖŘΡР", "R" },
        { "ŕŗřρр", "r" },
        { "ŚŜŞȘŠΣС", "S" },
        { "śŝşșšſσςс", "s" },
        { "ȚŢŤŦτТ", "T" },
        { "țţťŧт", "t" },
        { "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУ", "U" },
        { "ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u" },
        { "ÝŸŶΥΎΫỲỸỶỴЙ", "Y" },
        { "ýÿŷỳỹỷỵй", "y" },
        { "В", "V" },
        { "в", "v" },
        { "Ŵ", "W" },
        { "ŵ", "w" },
        { "ŹŻŽΖЗ", "Z" },
        { "źżžζз", "z" },
        { "ÆǼ", "AE" },
        { "ß", "ss" },
        { "Ĳ", "IJ" },
        { "ĳ", "ij" },
        { "Œ", "OE" },
        { "ƒ", "f" },
        { "ξ", "ks" },
        { "π", "p" },
        { "β", "v" },
        { "μ", "m" },
        { "ψ", "ps" },
        { "Ё", "Yo" },
        { "ё", "yo" },
        { "Є", "Ye" },
        { "є", "ye" },
        { "Ї", "Yi" },
        { "Ж", "Zh" },
        { "ж", "zh" },
        { "Х", "Kh" },
        { "х", "kh" },
        { "Ц", "Ts" },
        { "ц", "ts" },
        { "Ч", "Ch" },
        { "ч", "ch" },
        { "Ш", "Sh" },
        { "ш", "sh" },
        { "Щ", "Shch" },
        { "щ", "shch" },
        { "ЪъЬь", "" },
        { "Ю", "Yu" },
        { "ю", "yu" },
        { "Я", "Ya" },
        { "я", "ya" },
    };
    #endregion

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

    /// <summary>Generates a random string of 8-32 chars or of given length.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Generate(int? length = null, GenerateOptions? options = null, string prefix = "")
    {
        if (length != null) Guard.DisallowNegative(nameof(length), length.Value);
        Guard.DisallowNull(nameof(prefix), prefix);
        Guard.DisallowWhitespace(nameof(prefix), prefix);
        if (options != null && !options.AllowSpecialChars && options.AllowQuoteChars)
            throw new ArgumentException("Cannot allow quote chars when special chars are disallowed.", nameof(options));

        var length_ = length ?? RandomNumberGenerator.GetInt32(8, 33);
        if (length_ == 0) return string.Empty;

        var prefs = options ?? new GenerateOptions();
        var chars = _alpahNumChars;
        if (prefs.AllowSpecialChars) chars += _specialChars;
        if (prefs.AllowQuoteChars) chars += _quotesChars;
        chars = new string(chars.Shuffle().ToArray());

        return string.Concat(prefix, 
            new string((from c in Enumerable.Repeat(chars, length_)
                            select c[RandomNumberGenerator.GetInt32(c.Length)]).ToArray()));
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

    /// <summary>Determines if a string is empty or composed only by white spaces.</summary>
    public static bool IsEmptyWhitespace(string value)
    {
        Guard.DisallowNull(nameof(value), value);

        if (value.Length == 0) return true;

        return value.All(char.IsWhiteSpace);
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
    public static bool ContainsSpecialChar(string value, params char[] excluded)
    {
        Guard.DisallowNull(nameof(value), value);

        if (value.Trim().Length == 0) return false;

        foreach (var @char in value.ToCharArray()) {
            if (IsSpecialChar(@char) && !excluded.Contains(@char)) {
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
    public static int ChoiceOfIndex(string value, Func<string, bool>? validator = null)
    {
        Guard.DisallowNull(nameof(value), value);

        Func<string, bool> _nullValidator = _ => true;
        var _validator = validator ?? _nullValidator;

        var words = value.Split();
        var index = RandomNumberGenerator.GetInt32(words.Length - 1);
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
                var index = RandomNumberGenerator.GetInt32(value.Length - 1);
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
                    _specialChars[RandomNumberGenerator.GetInt32(_specialChars.Length - 1)],
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

    /// <summary>Normalizes a null or white space string to empty.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NormalizeToEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value;

    /// <summary>Removes tags from a string.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StripTag(string value)
    {
        Guard.DisallowNull(nameof(value), value);

        return _matchTagRegEx.Replace(value, string.Empty);
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

    /// <summary>Retrieves a substring from this instance. The substring starts at a specified
    /// character position and has a specified length. No exception is raised if limit is exceeded.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Substring(string value, int startIndex, int length, bool safe = false)
    {
        Guard.DisallowNull(nameof(value), value);

        if (safe == false) return value.Substring(startIndex, length);

        Guard.DisallowNegative(nameof(startIndex), startIndex);
        Guard.DisallowNegative(nameof(length), length);
        
        if (length == 0) return string.Empty;
        if (startIndex > value.Length) return string.Empty;

        var result = new StringBuilder(capacity: value.Length);
        for (int i = startIndex, l = 0; i < value.Length; i++, l++) {
            if (l == length) break;
            result.Append(value[i]);
        }
        return result.ToString();
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

    /// <summary>Reverses tha case of a character.</summary>
    public static char ReverseCase(char value)
    {
        if (!Char.IsLetter(value)) return value;

        return Char.IsLower(value) ? Char.ToUpper(value) : Char.ToLower(value);
    }

    /// <summary>Randomizes the case of a string characters.</summary>
    public static string RandomizeCase(string value)
    {
        Guard.DisallowNull(nameof(value), value);

        if (value.Length == 0) return value;
        if (!value.Any(Char.IsLetter)) return value;

        var randomized = _randomize();
        return randomized != value ? randomized : _reverse();

        string _randomize()
        {
            var result = value.ToCharArray();
            for (var i = 0; i < value.Length; i++) {
                result[i] = Primitives.ChanceOf(50)
                    ? ReverseCase(value[i]) : value[i];
            }
            return new string(result);
        }

        string _reverse()
        {
            var result = value.ToCharArray();
            for (var i = 0; i < value.Length; i++) {
                result[i] = ReverseCase(value[i]);
            }
            return new string(result);
        }
    }

    /// <summary>Normalize a diacritics with an ordinary character.</summary>
    public static char RemoveDiacritics(char value)
    {
        foreach (var entry in _diatritics) {
            if (entry.Key.IndexOf(value) != -1) {
                return entry.Value[0];
            }
        }

        return value;
    }

    /// <summary>Normalize a string with diacritics with ordinary characters.</summary>
    public static string RemoveDiacritics(string value)
    {
        Guard.DisallowNull(nameof(value), value);

        var builder = new StringBuilder(capacity: value.Length);

        foreach (char c in value) {
            int len = builder.Length;

            foreach (var entry in _diatritics) {
                if (entry.Key.IndexOf(c) != -1) {
                    builder.Append(entry.Value);
                    break;
                }
            }

            if (len == builder.Length) {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
