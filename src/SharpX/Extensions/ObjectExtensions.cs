namespace SharpX.Extensions;

public static class ObjectExtensions
{
    /// <summary>Discards a value and return <c>Unit</c>.</summary>
    public static Unit ToUnit<T>(this T value) => Unit.Default;
}
