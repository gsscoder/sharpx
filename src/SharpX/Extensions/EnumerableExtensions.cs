#pragma warning disable 8625

using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SharpX.Extensions;

public static class EnumerableExtensions
{
    /// <summary>Returns the cartesian product of two sequences by combining each element of the
    /// first set with each in the second and applying the user=define projection to the
    /// pair.</summary>
    public static IEnumerable<TResult> Cartesian<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        Guard.DisallowNull(nameof(first), first);
        Guard.DisallowNull(nameof(second), second);
        Guard.DisallowNull(nameof(resultSelector), resultSelector);

        return from element1 in first
                from element2 in second // TODO buffer to avoid multiple enumerations
                select resultSelector(element1, element2);
    }

    /// <summary>Prepends a single value to a sequence.</summary>
    public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource value)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(value), value);

        return Enumerable.Concat(Enumerable.Repeat(value, 1), source);
    }

    #region Concat
    /// <summary>Returns a sequence consisting of the head element and the given tail
    /// elements.</summary>
    public static IEnumerable<T> Concat<T>(this T head, IEnumerable<T> tail)
    {
        Guard.DisallowNull(nameof(head), head);
        Guard.DisallowNull(nameof(tail), tail);

        return tail.Prepend(head);
    }

    /// <summary>Returns a sequence consisting of the head elements and the given tail element.
    /// </summary>
    public static IEnumerable<T> Concat<T>(this IEnumerable<T> head, T tail)
    {
        Guard.DisallowNull(nameof(head), head);
        Guard.DisallowNull(nameof(tail), tail);

        return Enumerable.Concat(head, Enumerable.Repeat(tail, 1));
    }
    #endregion

    #region Exclude
    /// <summary>Excludes <paramref name="count"/> elements from a sequence starting at a given
    /// index.</summary>
    public static IEnumerable<T> Exclude<T>(this IEnumerable<T> source, int startIndex, int count)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNegative(nameof(startIndex), startIndex);
        Guard.DisallowNegative(nameof(count), count);

        var index = -1;
        var endIndex = startIndex + count;
        using var iter = source.GetEnumerator();
        // yield the first part of the sequence
        while (iter.MoveNext() && ++index < startIndex) {
            yield return iter.Current;
        }
        // skip the next part (up to count elements)
        while (++index < endIndex && iter.MoveNext()) {
            continue;
        }
        // yield the remainder of the sequence
        while (iter.MoveNext()) {
            yield return iter.Current;
        }
    }
    #endregion

    #region Index
    /// <summary>Returns a sequence of <c>KeyValuePair</c> where the key is the zero-based index
    /// of the value in the source sequence.</summary>
    public static IEnumerable<KeyValuePair<int, TSource>> Index<TSource>(
        this IEnumerable<TSource> source) => source.Index(0);

    /// <summary>Returns a sequence of <c>KeyValuePair</c> where the key is the index of the value
    /// in the source sequence. An additional parameter specifies the starting index.</summary>
    public static IEnumerable<KeyValuePair<int, TSource>> Index<TSource>(
        this IEnumerable<TSource> source, int startIndex) => source.Select((element, index) =>
            new KeyValuePair<int, TSource>(startIndex + index, element));
    #endregion

    #region Fold
    /// <summary>Returns the result of applying a function to a sequence of 1 element.</summary>
    public static TResult Fold<T, TResult>(this IEnumerable<T> source,
        Func<T, TResult> folder) => FoldImpl(source, 1, folder, null, null, null);

    /// <summary>Returns the result of applying a function to a sequence of 2 elements.</summary>
    public static TResult Fold<T, TResult>(this IEnumerable<T> source,
        Func<T, T, TResult> folder) => FoldImpl(source, 2, null, folder, null, null);

    /// <summary>Returns the result of applying a function to a sequence of 3 elements.</summary>
    public static TResult Fold<T, TResult>(this IEnumerable<T> source,
        Func<T, T, T, TResult> folder) => FoldImpl(source, 3, null, null, folder, null);

    /// <summary>Returns the result of applying a function to a sequence of 4 elements.</summary>
    public static TResult Fold<T, TResult>(this IEnumerable<T> source,
        Func<T, T, T, T, TResult> folder) => FoldImpl(source, 4, null, null, null, folder);

    static TResult FoldImpl<T, TResult>(IEnumerable<T> source, int count,
        Func<T, TResult> folder1,
        Func<T, T, TResult> folder2,
        Func<T, T, T, TResult> folder3,
        Func<T, T, T, T, TResult> folder4)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNegative(nameof(count), count);
        Guard.DisallowNull(nameof(folder1), folder1);
        Guard.DisallowNull(nameof(folder2), folder2);
        Guard.DisallowNull(nameof(folder3), folder3);
        Guard.DisallowNull(nameof(folder4), folder4);

        if (count == 1 && folder1 == null
            || count == 2 && folder2 == null
            || count == 3 && folder3 == null
            || count == 4 && folder4 == null)
        {                                                // ReSharper disable NotResolvedInText
            throw new ArgumentNullException("folder");   // ReSharper restore NotResolvedInText
        }

        var elements = new T[count];
        foreach (var e in AssertCountImpl(
            source.Index(), count, OnFolderSourceSizeErrorSelector)) {
            elements[e.Key] = e.Value;
        }

        return count switch
        {
            1 => folder1(elements[0]),
            2 => folder2(elements[0], elements[1]),
            3 => folder3(elements[0], elements[1], elements[2]),
            4 => folder4(elements[0], elements[1], elements[2], elements[3]),
            _ => throw new NotSupportedException(),
        };
    }

    static readonly Func<int, int, Exception> OnFolderSourceSizeErrorSelector = OnFolderSourceSizeError;

    static Exception OnFolderSourceSizeError(int cmp, int count)
    {
        var message = cmp < 0
                    ? "Sequence contains too few elements when exactly {0} {1} expected."
                    : "Sequence contains too many elements when exactly {0} {1} expected.";
        return new Exception(string.Format(message, count.ToString("N0"), count == 1 ? "was" : "were"));
    }
    #endregion

    #region Pairwise
    /// <summary>Returns a sequence resulting from applying a function to each element in the
    /// source sequence and its predecessor, with the exception of the first element which is 
    /// only returned as the predecessor of the second element.</summary>
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(resultSelector), resultSelector);

        using var e = source.GetEnumerator();
        if (!e.MoveNext()) {
            yield break;
        }
        var previous = e.Current;
        while (e.MoveNext()) {
            yield return resultSelector(previous, e.Current);
            previous = e.Current;
        }
    }
    #endregion

    # region ToDelimitedString
    /// <summary>Creates a delimited string from a sequence of values. The delimiter used depends
    /// on the current culture of the executing thread.</summary>
    public static string ToDelimitedString<TSource>(
        this IEnumerable<TSource> source) => ToDelimitedString(source, null);

    /// <summary>Creates a delimited string from a sequence of values and
    /// a given delimiter.</summary>
    public static string ToDelimitedString<TSource>(
        this IEnumerable<TSource> source, string delimiter) =>
        ToDelimitedStringImpl(source, delimiter, (sb, e) => sb.Append(e));

    static string ToDelimitedStringImpl<T>(IEnumerable<T> source, string delimiter,
        Func<StringBuilder, T, StringBuilder> append)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(delimiter), delimiter);
        Guard.DisallowNull(nameof(append), append);

        delimiter = delimiter ?? CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        var builder = new StringBuilder();
        var iterations = 0;
        foreach (var value in source) {
            if (iterations++ > 0) builder.Append(delimiter);
            append(builder, value);
        }
        return builder.ToString();
    }
    #endregion

    #region DistinctBy
    /// <summary>Returns all distinct elements of the given source, where "distinctness"
    /// is determined via a projection and the default equality comparer for the projected
    /// type.</summary>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(keySelector), keySelector);

        return source.DistinctBy(keySelector, null);
    }

    /// <summary>Returns all distinct elements of the given source, where "distinctness"
    /// is determined via a projection and the specified comparer for the projected type.</summary>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(keySelector), keySelector);

        return _(); IEnumerable<TSource> _()
        {
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source) {
                if (knownKeys.Add(keySelector(element)))
                    yield return element;
            }
        }
    }
    #endregion

    #region Repeat
    /// <summary>Repeats the sequence the specified number of times.</summary>
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        Guard.DisallowNull(nameof(source), source);
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count),
            "Repeat count must be greater than or equal to zero.");

        return RepeatImpl(source, count);
    }

    /// <summary>Repeats the sequence forever.</summary>
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return RepeatImpl(source, null);
    }

    static IEnumerable<T> RepeatImpl<T>(IEnumerable<T> source, int? count)
    {
        var memo = source.Materialize();
        using (memo as IDisposable) {
            while (count == null || count-- > 0) {
                foreach (var item in memo)
                    yield return item;
            }
        }
    }
    #endregion

    /// <summary>Safe function that returns Just(first element) or <c>Nothing</c>.</summary>
    public static Maybe<T> TryHead<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        using var e = source.GetEnumerator(); return e.MoveNext()
            ? Maybe.Just(e.Current)
            : Maybe.Nothing<T>();
    }

    /// <summary>Safe function that returns Just(last element) or <c>Nothing</c>.</summary>
    public static Maybe<T> TryLast<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return Maybe.Nothing<T>();
        T result = e.Current;
        while (e.MoveNext()) result = e.Current;
        return Maybe.Just(result);
    }

    /// <summary>Applies a function to each element of the source sequence and returns a new
    /// sequence of elements where the function returns Just(value).</summary>
    public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> source,
        Func<T, Maybe<TResult>> chooser)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(chooser), chooser);

        return _(); IEnumerable<TResult> _()
        {
            foreach (var item in source) {
                var result = chooser(item);
                if (result.MatchJust(out TResult value)) {
                    yield return value;
                }
            }
        }
    }

    /// <summary>Immediately executes the given action on each element in the source sequence.</summary>
    public static Unit ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(action), action);

        foreach (var element in source) {
            action(element);
        }
        return Unit.Default;
    }

    /// <summary>Immediately executes the given action on each element in the source sequence.</summary>
    public static Task<Unit> ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> func)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(func), func);

        foreach (var element in source) {
            func(element).Wait();
        }
        return Task.FromResult(Unit.Default);
    }        

    /// <summary>Return everything except first element and throws exception if empty.</summary>
    public static IEnumerable<T> Tail<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return _(); IEnumerable<T> _()
        {
            using var e = source.GetEnumerator();
            if (!e.MoveNext()) {
                throw new ArgumentException(
                    "The input sequence has an insufficient number of elements.");
            }
            while (e.MoveNext()) {
                yield return e.Current;
            }
        }
    }

    /// <summary>Return everything except first element without throwing exception if empty.</summary>
    public static IEnumerable<T> TailOrEmpty<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return _(); IEnumerable<T> _()
        {
            using (var e = source.GetEnumerator()) {
                if (e.MoveNext()) {
                    while (e.MoveNext()) {
                        yield return e.Current;
                    }
                }
            }
        }
    }

    /// <summary>Partition a sequence in to chunks of given size. Each chunk is an array of the
    /// resulting sequence.</summary>
    public static IEnumerable<T[]> ChunkBySize<T>(this IEnumerable<T> source, int chunkSize)
    {
        Guard.DisallowNull(nameof(source), source);
        if (chunkSize <= 0) throw new ArgumentException("The input must be positive.");

        return _(); IEnumerable<T[]> _()
        {
            using var e = source.GetEnumerator();
            while (e.MoveNext()) {
                var result = new T[chunkSize];
                result[0] = e.Current;
                var i = 1;
                while (i < chunkSize && e.MoveNext()) {
                    result[i] = e.Current;
                    i++;
                }
                yield return i == chunkSize ? result : SubArray(result, 0, i);
            }
        }

        T[] SubArray(T[] array, int index, int length) {
            T[] result = new T[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }
    }

    /// <summary>Splits an array into two parts at a given index. The first part ends just before
    /// the element at the given index; the second part starts with the element at the given
    /// index.</summary>
    public static (T[], T[]) SplitAt<T>(this IEnumerable<T> source, int index)
    {
        Guard.DisallowNull(nameof(source), source);
        if (index < 0) throw new ArgumentException("The input must be non-negative.");
        if (source.Count() < index) throw new ArgumentException("The input sequence has an insufficient number of elements.");

        if (index == 0) return (new T[] {}, source.ToArray());
        if (index == source.Count()) return (source.ToArray(), new T[] {});
        var left = source.Take(index).ToArray();
        var right = source.Skip(index).Take(source.Count() - index).ToArray();
        return (left, right);
    }

    /// <summary>Selects a random element.</summary>
    public static T Choice<T>(this IEnumerable<T> source)
    {
        Guard.DisallowNull(nameof(source), source);

        var index = RandomNumberGenerator.GetInt32(source.Count() - 1);
        return source.ElementAt(index);
    }

    /// <summary>Takes an element and a sequence and `intersperses' that element between its
    /// elements.</summary>
    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
    {
        Guard.DisallowNull(nameof(source), source);
        Guard.DisallowNull(nameof(element), element);

        return _(); IEnumerable<T> _()
        {
            var count = source.Count();
            var last = count - 1;
            for (var i = 0; i < count; i++) {
                yield return source.ElementAt(i);
                if (i != last) {
                    yield return element;
                }
            }
        }
    }

    #region Materialize
    class MaterializedEnumerable<T> : IEnumerable<T>
    {
        readonly ICollection<T> _inner;

        internal MaterializedEnumerable(IEnumerable<T> enumerable) =>
            _inner = enumerable as ICollection<T> ?? enumerable.ToArray();

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Captures the current state of a sequence.</summary>
    public static IEnumerable<T> Materialize<T>(this IEnumerable<T> source) =>
        source switch
        {
            null                        => throw new ArgumentNullException(nameof(source)),
            MaterializedEnumerable<T> _ => source,
            _                           => new MaterializedEnumerable<T>(source),
        };
    #endregion

    /// <summary>Flattens a sequence by one level.</summary>
    public static IEnumerable<T> FlattenOnce<T>(this IEnumerable<IEnumerable<T>> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return _(); IEnumerable<T> _()
        {
            foreach (var element in source) {
                foreach (var subelement in element) {
                    yield return subelement;
                }
            }
        }
    }

    // Based on https://stackoverflow.com/questions/273313/randomize-a-listt (accepted answer)
    /// <summary>Rendomizes a sequence.</summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        var n = list.Count;
        while (n > 1)
        {
            byte[] box;
            do box = RandomNumberGenerator.GetBytes(1);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    /// <summary>Converts a value to an enumerable.</summary>
    public static IEnumerable<T> ToEnumerable<T>(this T value) => Primitives.ToEnumerable(value);

    /// <summary>Determines if a sequence is null or empty.</summary>
    public static bool IsEmpty<T>(this IEnumerable<T>? value) => value == null || !value.Any();

    #region Internal
    static IEnumerable<TSource> AssertCountImpl<TSource>(IEnumerable<TSource> source,
        int count, Func<int, int, Exception> errorSelector)
    {
        var collection = source as ICollection<TSource>; // Optimization for collections
        if (collection != null) {
            if (collection.Count != count) {
                throw errorSelector(collection.Count.CompareTo(count), count);
            }   
            return source;
        }
        return ExpectingCountYieldingImpl(source, count, errorSelector);
    }

    static IEnumerable<TSource> ExpectingCountYieldingImpl<TSource>(IEnumerable<TSource> source,
        int count, Func<int, int, Exception> errorSelector)
    {
        var iterations = 0;
        foreach (var element in source) {
            iterations++;
            if (iterations > count) {
                throw errorSelector(1, count);
            }
            yield return element;
        }
        if (iterations != count) {
            throw errorSelector(-1, count);
        }
    }
    #endregion
}
