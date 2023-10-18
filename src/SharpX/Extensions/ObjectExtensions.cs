namespace SharpX.Extensions;

public static class ObjectExtensions
{
    /// <summary>Discards a value and return <c>Unit</c>.</summary>
    public static Unit ToUnit<T>(this T value) => Unit.Default;

    /// <summary>Returns true in case of a numeric type value, otherwise false.</summary>
    public static bool IsNumber<T>(this T? value) => Primitives.IsNumber(value);
}
