namespace SharpX;

/// <summary>Discriminator for <c>Maybe</c>.</summary>
public enum MaybeType
{
    /// <summary>Computation case without a value.</summary>
    Nothing,
    /// <summary>Computation case with a value.</summary>
    Just
}
