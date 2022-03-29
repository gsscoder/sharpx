
#pragma warning disable 8600, 8601, 8603, 8604, 8616, 8618

namespace SharpX
{
    /// <summary>Discriminator for <c>Either</c>.</summary>
    public enum EitherType
    {
        /// <summary>Failed computation case.</summary>
        Left,
        /// <summary>Sccessful computation case.</summary>
        Right
    }
    
    /// <summary>The <c>Either</c> type represents values with two possibilities: a value of type
    /// <c>Either</c> T U is either <c>Left</c> T or <c>Right</c> U. The <c>Either</c> type is
    /// sometimes used to represent a value which is either correct or an error; by convention, the
    /// <c>Left</c> constructor is used to hold an error value and the <c>Right</c> constructor is
    /// used to hold a correct value (mnemonic: "right" also means "correct").</summary>
    public struct Either<TLeft, TRight>
    {
        readonly TLeft _leftValue;
        readonly TRight _rightValue;

        internal Either(TLeft value)
        {
            _leftValue = value;
            _rightValue = default;
            Tag = EitherType.Left;
        }

        internal Either(TRight value)
        {
            _leftValue = default;
            _rightValue = value;
            Tag = EitherType.Right;
        }

        public EitherType Tag { get; private set; }

        #region Basic match methods
        /// <summary>Matches a <c>Left</c> value returning <c>true</c> and value itself via an output
        /// parameter.</summary>
        public bool MatchLeft(out TLeft? value)
        {
            value = Tag == EitherType.Left ? _leftValue : default;
            return Tag == EitherType.Left;
        }

        /// <summary>Matches a <c>Right</c> value returning <c>true</c> and value itself via an output
        /// parameter.</summary>
        public bool MatchRight(out TRight? value)
        {
            value = Tag == EitherType.Right ? _rightValue : default;
            return Tag == EitherType.Right;
        }
        #endregion
    }

    public static class Either
    {
        #region Value case constructors
        /// <summary>Builds the <c>Left</c> case of an <c>Either</c> value.</summary>
        public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value) =>
            new Either<TLeft, TRight>(value);

        /// <summary>Builds the <c>Right</c> case of an <c>Either</c> value.</summary>
        public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value) =>
            new Either<TLeft, TRight>(value);
        #endregion

        #region Monad
        /// <summary>Inject a value into the <c>Either</c> type, returning Right case.</summary>
        public static Either<string, TRight> Return<TRight>(TRight value) =>
            Either.Right<string, TRight>(value);

        /// <summary>Monadic bind.</summary>
        public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(
            Either<TLeft, TRight> either, Func<TRight, Either<TLeft, TResult>> func)
        {
            Guard.DisallowNull(nameof(either), either);
            Guard.DisallowNull(nameof(func), func);

            if (either.MatchRight(out TRight right)) {
                return func(right);
            }
            return Either.Left<TLeft, TResult>(either.GetLeft());
        }
        #endregion

        #region Functor
        /// <summary>Transforms a <c>Either</c> right value by using a specified mapping function.</summary>
        public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(Either<TLeft, TRight> either,
            Func<TRight, TResult> func)
        {
            Guard.DisallowNull(nameof(either), either);
            Guard.DisallowNull(nameof(func), func);

            if (either.MatchRight(out TRight right)) {
                return Either.Right<TLeft, TResult>(func(right));
            }
            return Either.Left<TLeft, TResult>(either.GetLeft());
        }
        #endregion

        #region Bifunctor
        /// <summary>Maps both parts of a Either type. Applies the first function if <c>Either</c>
        /// is <c>Left</c>. Otherwise applies the second function.</summary>
        public static Either<TLeft1, TRight1> Bimap<TLeft, TRight, TLeft1, TRight1>(Either<TLeft, TRight> either,
            Func<TLeft, TLeft1> mapLeft, Func<TRight, TRight1> mapRight)
        {
            Guard.DisallowNull(nameof(either), either);
            Guard.DisallowNull(nameof(mapLeft), mapLeft);
            Guard.DisallowNull(nameof(mapRight), mapRight);

            if (either.MatchRight(out TRight right)) {
                return Either.Right<TLeft1, TRight1>(mapRight(right));
            }
            return Either.Left<TLeft1, TRight1>(mapLeft(either.GetLeft()));
        }
        #endregion

        /// <summary>Fail with a message. Not part of mathematical definition of a monad.</summary>
        public static Either<string, TRight> Fail<TRight>(string message) => throw new Exception(message);

        /// <summary>Wraps a function, encapsulates any exception thrown within to a <c>Either</c>.</summary>
        public static Either<Exception, TRight> Try<TRight>(Func<TRight> func)
        {
            Guard.DisallowNull(nameof(func), func);

            try {
                return new Either<Exception, TRight>(func());
            }
            catch (Exception ex) {
                return new Either<Exception, TRight>(ex);
            }
        }

        /// <summary>Attempts to cast an object. Stores the cast value in <c>Right</c> if successful, otherwise
        /// stores the exception in <c>Left</c>.</summary>
        public static Either<Exception, TRight> Cast<TRight>(object obj) => Either.Try(() => (TRight)obj);

        /// <summary>Converts a <c>Just</c> value to a <c>Right</c> and a <c>Nothing</c> value to a
        /// <c>Left</c>.</summary>
        public static Either<TLeft, TRight> FromMaybe<TLeft, TRight>(Maybe<TRight> maybe, TLeft left)
        {
            Guard.DisallowNull(nameof(maybe), maybe);
            Guard.DisallowNull(nameof(left), left);

            if (maybe.Tag == MaybeType.Just) {
                return Either.Right<TLeft, TRight>(maybe.FromJust());
            }
            return Either.Left<TLeft, TRight>(left);
        }

        static TLeft? GetLeft<TLeft, TRight>(this Either<TLeft, TRight> either) => either.FromLeft();
    }

    public static class EitherExtensions
    {
        #region LINQ operators
        /// <summary>Map operation compatible with LINQ.</summary>
        public static Either<TLeft, TResult> Select<TLeft, TRight, TResult>(
            this Either<TLeft, TRight> either,
            Func<TRight, TResult> selector) => Either.Map(either, selector);

        /// <summary>Map operation compatible with LINQ.</summary>
        public static Either<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(this Either<TLeft, TRight> result,
            Func<TRight, Either<TLeft, TResult>> func) => Either.Bind(result, func);
        #endregion

        #region Alternative match extensions
        public static Unit Match<TLeft, TRight>(this Either<TLeft, TRight> either,
            Func<TLeft, Unit> onLeft, Func<TRight, Unit> onRight)
        {
            Guard.DisallowNull(nameof(either), either);
            Guard.DisallowNull(nameof(onLeft), onLeft);
            Guard.DisallowNull(nameof(onRight), onRight);

            return either.MatchRight(out TRight right) switch {
                true => onRight(right),
                _ =>    onLeft(either.FromLeft())
            };
        }
        #endregion

        /// <summary>Equivalent to monadic <c>Return</c> operation. Builds a <c>Right</c> value
        /// by default.</summary>
        public static Either<string, TRight> ToEither<TRight>(this TRight value) => Either.Return<TRight>(value);

        /// <summary>Equivalent to monadic Bind.</summary>
        public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(
            this Either<TLeft, TRight> either,
            Func<TRight, Either<TLeft, TResult>> func) => Either.Bind(either, func);

        /// <summary>Equivalent to monadic Map.</summary>
        public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(
            this Either<TLeft, TRight> either,
            Func<TRight, TResult> func) => Either.Map(either, func);

        /// <summary>Eviqualent to monadic Bimap.</summary>
        public static Either<TLeft1, TRight1> Bimap<TLeft, TRight, TLeft1, TRight1>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TLeft1> mapLeft,
            Func<TRight, TRight1> mapRight) => Either.Bimap(either, mapLeft, mapRight);

        /// <summary>Returns <c>true</c> if it is in form of <c>Left</c>.</summary>
        public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> either) =>
            either.Tag == EitherType.Left;

        /// <summary>Returns <c>true</c> if it is in form of <c>Right</c>.</summary>
        public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> either) =>
            either.Tag == EitherType.Right;

        /// <summary>Extracts the element out of <c>Left</c> and returns a default value (or <c>noneValue</c>
        /// when given) if it is in form of <c>Right</c>.</summary>
        public static TLeft FromLeft<TLeft, TRight>(this Either<TLeft, TRight> either,
            TLeft? noneValue = default) => either.MatchLeft(out TLeft value) ? value : noneValue;

        /// <summary>Extracts the element out of <c>Left</c> and throws an exception if it is form of
        /// <c>Right</c>.</summary>
        public static TLeft FromLeftOrFail<TLeft, TRight>(this Either<TLeft, TRight> either,
            Exception? exceptionToThrow = null)
        {
            Guard.DisallowNull(nameof(either), either);

            if (either.MatchLeft(out TLeft value)) {
                return value;
            }
            throw exceptionToThrow ?? new Exception("The value is empty.");
        }

        /// <summary>Extracts the element out of <c>Left</c> and returns a default (or <c>noneValue</c>
        /// when given) value if it is in form of<c>Right</c>.</summary>
        public static TRight? FromRight<TLeft, TRight>(this Either<TLeft, TRight> either,
            TRight? noneValue = default) => either.MatchRight(out TRight value) ? value : noneValue;

        /// <summary>Extracts the element out of <c>Left</c> and throws an exception if it is form of
        /// <c>Right</c>.</summary>
        public static TRight? FromRightOrFail<TLeft, TRight>(this Either<TLeft, TRight> either,
            Exception? exceptionToThrow = null)
        {
            Guard.DisallowNull(nameof(either), either);

            if (either.MatchRight(out TRight value)) {
                return value;
            }
            throw exceptionToThrow ?? new Exception("The value is empty.");
        }

        #region Sequences
        /// <summary>Extracts from a sequence of <c>Either</c> all the <c>Left</c> elements. All the
        /// <c>Left</c> elements are extracted in order.</summary>
        public static IEnumerable<TLeft> Lefts<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source)
        {
            Guard.DisallowNull(nameof(source), source);

            return _(); IEnumerable<TLeft> _()
            {
                foreach (var either in source) {
                    if (either.Tag == EitherType.Left) yield return either.FromLeft();
                }
            }
        }

        /// <summary>Extracts from a sequence of <c>Either</c> all the <c>Right</c> elements. All the
        /// <c>Rights</c> elements are extracted in order.</summary>
        public static IEnumerable<TRight> Rights<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source)
        {
            Guard.DisallowNull(nameof(source), source);

            return _(); IEnumerable<TRight> _()
            {
                foreach (var either in source) {
                    if (either.Tag == EitherType.Right) yield return either.FromRight();
                }
            }
        }       

        /// <summary>Partitions a sequence of <c>Either</c> into two sequences. All the <c>Left</c>
        /// elements are extracted, in order, to the first component of the pair. Similarly the <c>Right</c>
        /// elements are extracted to the second component of the pair.</summary>
        public static (IEnumerable<TLeft>, IEnumerable<TRight>) Partition<TLeft, TRight>(
            this IEnumerable<Either<TLeft, TRight>> source)
        {
            Guard.DisallowNull(nameof(source), source);

            var lefts = new List<TLeft>();
            var rights = new List<TRight>();

            foreach (var either in source) {
                if (either.Tag == EitherType.Left) lefts.Add(either.FromLeft());
                else rights.Add(either.FromRight());
            }
            return (lefts, rights);
        }
        #endregion
    }
}
