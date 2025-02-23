
namespace SharpX;

/// <summary>The <c>Either</c> type represents values with two possibilities: a value of type
/// <c>Either</c> T U is either <c>Left</c> T or <c>Right</c> U. The <c>Either</c> type is
/// sometimes used to represent a value which is either correct or an error; by convention, the
/// <c>Left</c> constructor is used to hold an error value and the <c>Right</c> constructor is
/// used to hold a correct value (mnemonic: "right" also means "correct").</summary>
public struct Either<TLeft, TRight>
{
    private readonly TLeft? _leftValue;
    private readonly TRight? _rightValue;

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
