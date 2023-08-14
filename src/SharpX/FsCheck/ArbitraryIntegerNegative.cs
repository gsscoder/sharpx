using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryIntegerNegative
{
    public static Arbitrary<int> Generator() => Gen.Choose(-60, -30).ToArbitrary();
}
