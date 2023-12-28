using System.Runtime.CompilerServices;

namespace SharpX;

public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowNull(string argumentName, object? value) =>
        _ = value ?? throw new ArgumentNullException(argumentName, $"{argumentName} cannot be null.");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowDefault<T>(string argumentName, T value)
        where T : struct
    {
        if (value.Equals(default(T))) throw new ArgumentNullException(argumentName, $"{argumentName} cannot be default.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowWhitespace(string argumentName, string value)
    {
        if (value.Any(c => char.IsWhiteSpace(c))) throw new ArgumentException(
            $"{argumentName} cannot be made of or contains only white spaces.", argumentName);
    }    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowEmptyWhitespace(string argumentName, string value)
    {
        if (value.Trim().Length == 0) throw new ArgumentException(
            $"{argumentName} cannot be empty or contains only white spaces.", argumentName);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowEmptyWhitespace(string argumentName, string[] value)
    {
        if (value.Any(s => s.Trim().Length == 0)) throw new ArgumentException(
            $"{argumentName} items cannot be empty or contains only white spaces.", argumentName);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowNegative(string argumentName, int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} cannot be lesser than zero.");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowNegativeZero(string argumentName, int value)
    {
        if (value < 1) throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} cannot be lesser than one.");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowOdd(string argumentName, int value)
    {
        if (value % 2 != 0) throw new ArgumentOutOfRangeException(argumentName, $"{argumentName} cannot be odd.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowArraySize<T>(string argumentName, int length, T[] value)
    {
        if (value.Length < length) throw new ArgumentException(
            $"{argumentName} cannot contain less than {length} elements.", argumentName);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowEmptyArray<T>(string argumentName, T[] value)
    {
        if (value.Length == 0) throw new ArgumentException($"{argumentName} cannot be empty.", argumentName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowEmptyEnumerable<T>(string argumentName, IEnumerable<T> value)
    {
        if (value.Count() == 0) throw new ArgumentException($"{argumentName} cannot be empty.", argumentName);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisallowMalformedGuid(string argumentName, string value)
    {
        try { Guid.Parse(value); }
        catch { throw new ArgumentException($"{argumentName} must be a correctly formatted GUID.", argumentName); }
    }
}
