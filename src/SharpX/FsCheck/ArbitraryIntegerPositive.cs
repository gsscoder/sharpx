using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryIntegerPositive
{
    public static Arbitrary<int> Generator() => Gen.Choose(1, 99).ToArbitrary();
}
