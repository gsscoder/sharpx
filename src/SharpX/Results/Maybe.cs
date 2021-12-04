using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpX
{
    /// <summary>Discriminator for <c>Maybe</c>.</summary>
    public enum MaybeType
    {
        /// <summary>Computation case without a value.</summary>
        Nothing,
        /// <summary>Computation case with a value.</summary>
        Just
    }

    /// <summary>The <c>Maybe</c> type models an optional value. A value of type <c>Maybe</c> either
    /// contains a value (represented as <c>Just</c> a), or it is empty (represented as
    /// <c>Nothing</c>).</summary>
    public struct Maybe<T> : IEquatable<Maybe<T>>
    {
#if DEBUG
        internal
#endif
        readonly T _value;

        internal Maybe(T value)
        {
            _value = value;
            Tag = MaybeType.Just;
        }

        /// <summary>Type discriminator.</summary>
        public MaybeType Tag { get; private set; }

        /// <summary>Determines whether this instance and another specified <c>Maybe</c> object have the same value.</summary>
        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (other is not Maybe<T> maybe) return false;

            var casted = (Maybe<T>)other;

            return Equals(casted);
        }

        /// <summary>Determines whether this instance and another specified <c>Maybe</c> object have the same value.</summary>
        public bool Equals(Maybe<T> other) =>
            other.Tag != MaybeType.Just || _value.Equals(other._value);

        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);

        /// <summary>Serves as the default hash function.</summary>
        public override int GetHashCode()
        {
            unchecked {
                var hashCode = 2;
                hashCode = hashCode * 3 * typeof(Maybe<T>).GetHashCode();
                if (Tag == MaybeType.Just)
                    hashCode = hashCode * 3 + _value.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() =>
            Tag switch {
                MaybeType.Just => new StringBuilder("Just(")
                                      .Append(_value)
                                      .Append(")")
                                      .ToString(),
                _ => "<Nothing>"
            };

        #region Basic match methods
        /// <summary>Matches a value returning <c>true</c> and value itself via an output
        /// parameter.</summary>
        public bool MatchJust(out T value)
        {
            value = Tag == MaybeType.Just ? _value : default;
            return Tag == MaybeType.Just;
        }

        /// <summary>Matches an empty value returning <c>true</c>.</summary>
        public bool MatchNothing() => Tag == MaybeType.Nothing;
        #endregion
    }

    /// <summary>Provides static methods for manipulating <c>Maybe</c>.</summary>
    public static class Maybe
    {
        #region Value case constructors
        /// <summary>Builds the empty case of <c>Maybe</c>.</summary>
        public static Maybe<T> Nothing<T>() => new Maybe<T>();

        /// <summary>Builds the case when <c>Maybe</c> contains a value.</summary>
        public static Maybe<T> Just<T>(T value) => new Maybe<T>(value);
        #endregion

        #region Monad
        /// <summary>Injects a value into the monadic <c>Maybe</c> type.</summary>
        public static Maybe<T> Return<T>(T value) => Equals(value, default(T)) ? Nothing<T>() : Just(value);

        /// <summary>Sequentially compose two actions, passing any value produced by the first as
        /// an argument to the second.</summary>
        public static Maybe<U> Bind<T, U>(Maybe<T> maybe, Func<T, Maybe<U>> onJust)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);

            return maybe.MatchJust(out T value) ? onJust(value) : Nothing<U>();
        }
        #endregion

        #region Functor
        /// <summary>Transforms a <c>Maybe</c> value by using a specified mapping function.</summary>
        public static Maybe<U> Map<T, U>(Maybe<T> maybe, Func<T, U> onJust)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);

            return maybe.MatchJust(out T value) ? Just(onJust(value)) : Nothing<U>();
        }
        #endregion

        /// <summary>If both <c>Maybe</c> values contain a value, it merges them into a <c>Maybe</c>
        /// with a tupled value. </summary>
        public static Maybe<(T, U)> Merge<T, U>(Maybe<T> first, Maybe<U> second)
        {
            Guard.DisallowNull(nameof(first), first);
            Guard.DisallowNull(nameof(second), second);

            U value2 = default;
            return (first.MatchJust(out T value1) &&
                    second.MatchJust(out value2)) switch {
                true => Just((value1, value2)),
                _ => Nothing<(T, U)>()
            };
        }

        /// <summary>Executes the given function on a <c>Just</c> success or returns a <c>Nothing</c>.</summary>
        public static Maybe<T> Try<T>(Func<T> func)
        {
            Guard.DisallowNull(nameof(func), func);

            try {
                return Just(func());
            }
            catch {
                return Nothing<T>();
            }
        }

        /// <summary>Maps <c>Either</c> right value to <c>Just</c>, otherwise returns
        /// <c>Nothing</c>.</summary>
        public static Maybe<TRight> FromEither<TLeft, TRight>(Either<TLeft, TRight> either)
        {
            Guard.DisallowNull(nameof(either), either);

            return (either.Tag == EitherType.Right) switch
            {
                true => Just(either.FromRight()),
                _ => Nothing<TRight>()
            };
        }
    }

    /// <summary>Provides convenience extension methods for <c>Maybe</c>.</summary>
    public static class MaybeExtensions
    {
        #region Match extensions
        /// <summary>Returns <c>true</c> if it is in form of <c>Nothing</c>.</summary>
        public static bool IsNothing<T>(this Maybe<T> maybe)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return maybe.Tag == MaybeType.Nothing;
        }

        /// <summary>Returns <c>true</c> if it is in form of <c>Just</c>.</summary>
        public static bool IsJust<T>(this Maybe<T> maybe)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return maybe.Tag == MaybeType.Just;
        }

        /// <summary>Provides pattern matching using <c>System.Func</c> delegates.</summary>
        public static Unit Match<T>(this Maybe<T> maybe,
            Func<T, Unit> onJust, Func<Unit> onNothing)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);
            Guard.DisallowNull(nameof(onNothing), onNothing);

            return maybe.MatchJust(out T value) switch {
                true => onJust(value),
                _    => onNothing()
            };
        }

        /// <summary>Provides pattern matching using <c>System.Func</c> delegates over a <c>Maybe</c>
        /// with tupled wrapped value.</summary>
        public static Unit Match<T, U>(this Maybe<(T, U)> maybe,
            Func<T, U, Unit> onJust, Func<Unit> onNothing)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);
            Guard.DisallowNull(nameof(onNothing), onNothing);

            return maybe.MatchJust(out T value1, out U value2) switch {
                true => onJust(value1, value2),
                _    => onNothing()
            };
        }

        /// <summary>Matches a value returning <c>true</c> and the tupled value itself via two output
        /// parameters.</summary>
        public static bool MatchJust<T, U>(this Maybe<(T, U)> maybeTuple,
            out T value1, out U value2)
        {
            Guard.DisallowNull(nameof(maybeTuple), maybeTuple);

            if (maybeTuple.MatchJust(out (T, U) value)) {
                value1 = value.Item1;
                value2 = value.Item2;
                return true;
            }
            value1 = default;
            value2 = default;
            return false;
        }
        #endregion

        #region Monad
        /// <summary>Equivalent to monadic <c>Return</c> operation. Builds a <c>Just</c> value in case
        /// <c>value</c> is different from its default.
        /// </summary>
        public static Maybe<T> ToMaybe<T>(this T value) => Maybe.Return(value);

        /// <summary>Invokes a function on this maybe value that itself yields a maybe.</summary>
        public static Maybe<U> Bind<T, U>(this Maybe<T> maybe, Func<T, Maybe<U>> onJust) =>
            Maybe.Bind(maybe, onJust);

        /// <summary>Transforms a maybe value by using a specified mapping function.</summary>
        public static Maybe<U> Map<T, U>(this Maybe<T> maybe, Func<T, U> onJust) =>
            Maybe.Map(maybe, onJust);

        /// <summary>Unwraps a value applying a function o returns another value on fail.</summary>
        public static U Return<T, U>(this Maybe<T> maybe, Func<T, U> onJust, U @default) =>
            maybe.MatchJust(out T value) ? onJust(value) : @default;
        #endregion

        /// <summary>This is a version of map which can throw out the value. If contains a <c>Just</c>
        /// executes a mapping function over it, in case of <c>Nothing</c> returns <c>@default</c>.</summary>
        public static U Map<T, U>(this Maybe<T> maybe, Func<T, U> onJust, U @default = default)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);

            return maybe.MatchJust(out T value) ? onJust(value) : @default;
        }

        /// <summary>Lazy version of <c>Map</c>. If contains a <c>Just</c> executes a mapping function
        /// over it, in case of <c>Nothing</c> returns a value built by <c>@default</c> function.</summary>
        public static U Map<T, U>(this Maybe<T> maybe, Func<T, U> onJust, Func<U> @default)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(onJust), onJust);

            return maybe.MatchJust(out T value) ? onJust(value) : @default();
        }

        #region LINQ operators
        /// <summary>Map operation compatible with LINQ.</summary>
        public static Maybe<TResult> Select<TSource, TResult>(this Maybe<TSource> maybe,
            Func<TSource, TResult> selector) => Maybe.Map(maybe, selector);

        /// <summary>Bind operation compatible with LINQ.</summary>
        public static Maybe<TResult> SelectMany<TSource, TValue, TResult>(this Maybe<TSource> maybe,
            Func<TSource, Maybe<TValue>> valueSelector, Func<TSource, TValue, TResult> resultSelector) =>
            maybe
                .Bind(sourceValue =>
                        valueSelector(sourceValue)
                            .Map(resultValue => resultSelector(sourceValue, resultValue)));

        /// <summary>Returns the same Maybe value if the predicate is true, otherwise
        /// <c>Nothing</c>.</summary>
        public static Maybe<TSource> Where<TSource>(this Maybe<TSource> maybe,
            Func<TSource, bool> predicate) 
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(predicate), predicate);

            if (maybe.MatchJust(out TSource value)) {
                if (predicate(value)) return maybe;
            }
            return Maybe.Nothing<TSource>();
        }
        #endregion

        #region Do semantic
        /// <summary>If contains a value executes a <c>System.Func<c> delegate over it.</summary>
        public static Unit Do<T>(this Maybe<T> maybe, Func<T, Unit> func)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(func), func);

            return maybe.MatchJust(out T value) switch {
                true => func(value),
                _ => Unit.Default
            };
        }

        /// <summary>If contans a value executes a <c>System.Func<c> delegate over it.</summary>
        public static Unit Do<T, U>(this Maybe<(T, U)> maybe, Func<T, U, Unit> func)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(func), func);

            return maybe.MatchJust(out T value1, out U value2) switch {
                true => func(value1, value2),
                _ => Unit.Default
            };
        }
        #endregion

        /// <summary>Returns a <c>Just</c> of the given value.</summary>
        public static Maybe<T> ToJust<T>(this T value) => Maybe.Just(value);

        /// <summary>Extracts the element out of <c>Just</c> and returns a default value (or <c>@default</c>
        /// when given) if it is in form of <c>Nothing</c>.</summary>
        public static T FromJust<T>(this Maybe<T> maybe, T @default = default)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return maybe.MatchJust(out T value) ? value : @default;
        }

        /// <summary>Lazy version of <c>FromJust</c>. Extracts the element out of <c>Just</c> and returns
        /// a value built by <c>@default</c> function if it is in form of <c>Nothing</c>.</summary>
        public static T FromJust<T>(this Maybe<T> maybe, Func<T> @default)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return maybe.MatchJust(out T value) ? value : @default();
        }

        /// <summary>Extracts the element out of <c>Just</c> or throws an exception if it is form of
        /// <c>Nothing</c>.</summary>
        public static T FromJustOrFail<T>(this Maybe<T> maybe, Exception exceptionToThrow = null)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return maybe.MatchJust(out T value) switch
            {
                true => value,
                _ => throw exceptionToThrow ?? new Exception("The value is empty.")
            };
        }

        #region Sequences
        /// <summary>Returns an empty sequence when given <c>Nothing</c> or a singleton sequence in
        /// case of <c>Just</c>.</summary>
        public static IEnumerable<T> ToEnumerable<T>(this Maybe<T> maybe)
        {
            Guard.DisallowNull(nameof(maybe), maybe);

            return _(); IEnumerable<T> _()
            {
                if (maybe.MatchJust(out T value)) yield return value;
            }
        }

        /// <summary>Turns an empty sequence to <c>Nothing</c>, otherwise Just(sequence).</summary>
        public static Maybe<IEnumerable<T>> ToMaybe<T>(this IEnumerable<T> source)
        {
            Guard.DisallowNull(nameof(source), source);

            using var e = source.GetEnumerator();
            return e.MoveNext()
                ? Maybe.Just(source)
                : Maybe.Nothing<IEnumerable<T>>();
        }

        /// <summary>Takes a sequence of <c>Maybe</c> and counts all the <c>Nothing</c> values.</summary>
        public static int Nothings<T>(this IEnumerable<Maybe<T>> source)
        {
            Guard.DisallowNull(nameof(source), source);

            var count = 0;
            foreach (var maybe in source) {
                if (maybe.Tag == MaybeType.Just) count++;
            }
            return count;
        }

        /// <summary>Takes a sequence of <c>Maybe</c> and returns a sequence of all the <c>Just</c>
        /// values.</summary>
        public static IEnumerable<T> Justs<T>(this IEnumerable<Maybe<T>> source)
        {
            Guard.DisallowNull(nameof(source), source);

            return _(); IEnumerable<T> _()
            {
                foreach (var maybe in source) {
                    if (maybe.Tag == MaybeType.Just) yield return maybe.FromJust();
                }
            }
        }

        /// <summary>This is a version of map which can throw out elements. In particular, the functional
        /// argument returns something of type <c>Maybe&lt;U&gt;</c>. If this is Nothing, no element is
        /// added on to the result sequence. If it is <c>Just&lt;U&gt;</c>, then <c>U</c> is included
        /// in the result sequence.</summary>
        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> source, Func<T, Maybe<U>> onElement)
        {
            Guard.DisallowNull(nameof(source), source);
            Guard.DisallowNull(nameof(onElement), onElement);

            return _(); IEnumerable<U> _()
            {
                foreach (var element in source) {
                    if (onElement(element).MatchJust(out U value)) yield return value;
                }
            }
        }

        /// <summary>Returns the first element of a sequence as <c>Just</c>, or <c>Nothing</c> if the sequence
        /// contains no elements.</summary>
        public static Maybe<TSource> FirstOrNothing<TSource>(this IEnumerable<TSource> source)
        {
            Guard.DisallowNull(nameof(source), source);

            if (source is IList<TSource> list) {
                if (list.Count > 0) return Maybe.Just(list[0]);
            }
            else {
                using IEnumerator<TSource> e = source.GetEnumerator();
                if (e.MoveNext()) return Maybe.Just(e.Current);
            }
            return Maybe.Nothing<TSource>();
        }

        /// <summary>Returns the first element of the sequence as <c>Just</c> that satisfies a condition or
        /// <c>Nothing</c> if no such element is found.</summary>
        public static Maybe<TSource> FirstOrNothing<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            Guard.DisallowNull(nameof(source), source);
            Guard.DisallowNull(nameof(predicate), predicate);

            foreach (TSource element in source) {
                if (predicate(element)) return Maybe.Just(element);
            }
            return Maybe.Nothing<TSource>();
        }

        /// <summary>Returns the last element of a sequence as <c>Just</c>, or <c>Nothing</c> if the sequence
        /// contains no elements. </summary>
        public static Maybe<TSource> LastOrNothing<TSource>(this IEnumerable<TSource> source)
        {
            Guard.DisallowNull(nameof(source), source);

            if (source is IList<TSource> list) {
                int count = list.Count;
                if (count > 0) return Maybe.Just(list[count - 1]);
            }
            else {
                using IEnumerator<TSource> e = source.GetEnumerator();
                if (e.MoveNext()) {
                    TSource result;
                    do {
                        result = e.Current;
                    } while (e.MoveNext());
                    return Maybe.Just(result);
                }
            }
            return Maybe.Nothing<TSource>();
        }

        /// <summary>Returns the last element of a sequence as <c>Just</c> that satisfies a condition or <c>Nothing</c> if
        /// no such element is found.</summary>
        public static Maybe<TSource> LastOrNothing<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Guard.DisallowNull(nameof(source), source);
            Guard.DisallowNull(nameof(predicate), predicate);

            var result = Maybe.Nothing<TSource>();
            foreach (var element in source) {
                if (predicate(element)) {
                    result = Maybe.Just(element);
                }
            }
            return result;
        }

        /// <summary> Returns the only element of a sequence as <c>Just</c>, or <c>Nothing</c> if the sequence is
        /// empty.</summary>
        public static Maybe<TSource> SingleOrNothing<TSource>(this IEnumerable<TSource> source)
        {
            Guard.DisallowNull(nameof(source), source);

            if (source is IList<TSource> list) {
                switch (list.Count) {
                    case 0: return Maybe.Nothing<TSource>();
                    case 1: return Maybe.Just(list[0]);
                }
            }
            else {
                using IEnumerator<TSource> e = source.GetEnumerator();
                if (!e.MoveNext()) return Maybe.Nothing<TSource>();
                TSource result = e.Current;
                if (!e.MoveNext()) return Maybe.Just(result);
            }
            return Maybe.Nothing<TSource>();
        }

        /// <summary>Returns the only element of a sequence that satisfies a specified condition as
        /// <c>Just</c> or a <c>Nothing</c> if no such element exists; this method throws an exception if more than
        /// one element satisfies the condition.</summary>
        public static Maybe<TSource> SingleOrNothing<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Guard.DisallowNull(nameof(source), source);
            Guard.DisallowNull(nameof(predicate), predicate);

            var result = Maybe.Nothing<TSource>();
            var count = 0L;
            foreach (var element in source) {
                if (predicate(element)) {
                    result = Maybe.Just(element);
                    checked { count++; }
                }
            }
            return count switch
            {
                0 => Maybe.Nothing<TSource>(),
                1 => result,
                _ => throw new InvalidOperationException("Sequence contains more than one element"),
            };
        }

        /// <summary> Returns the element at a specified index in a sequence as <c>Just</c> or <c>Nothing</c>
        /// if the index is out of range.</summary>
        public static Maybe<TSource> ElementAtOrNothing<TSource>(this IEnumerable<TSource> source, int index)
        {
            Guard.DisallowNull(nameof(source), source);
            Guard.DisallowNegative(nameof(index), index);

            if (index >= 0) {
                if (source is IList<TSource> list) {
                    if (index < list.Count) return Maybe.Just(list[index]);
                }
                else {
                    using IEnumerator<TSource> e = source.GetEnumerator();
                    while (true) {
                        if (!e.MoveNext()) break;
                        if (index == 0) return Maybe.Just(e.Current);
                        index--;
                    }
                }
            }
            return Maybe.Nothing<TSource>();
        }
        #endregion
    }
}
