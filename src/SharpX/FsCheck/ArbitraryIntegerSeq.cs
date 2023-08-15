using FsCheck;
using FsCheck.Fluent;

namespace SharpX.FsCheck;

static class ArbitraryIntegerSeq
{
    public static Arbitrary<int[]> Generator() => Gen.ArrayOf<int>(
        Gen.Choose(-30, 30), 100).ToArbitrary();
}
