using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryIntegerPositive
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(1, 99).ToArbitrary();
}
