using FsCheck;
using FsCheck.Fluent;

namespace SharpX.FsCheck;

public static class ArbitraryIntegerSeq
{
    public static Arbitrary<int[]> Generator()
    {
        var seq = Primitives.GenerateSeq<int>(count: 100);

        return Gen.Shuffle(seq).ToArbitrary();
    }
}
