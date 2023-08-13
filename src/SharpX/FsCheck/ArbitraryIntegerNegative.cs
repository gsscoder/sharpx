using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryIntegerNegative
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(-60, -30).ToArbitrary();
}
