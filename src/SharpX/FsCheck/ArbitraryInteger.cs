using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryInteger
{
    public static Arbitrary<int> Generator() => Gen.Choose(-30, 30).ToArbitrary();
}
