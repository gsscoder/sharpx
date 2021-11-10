using FsCheck;

static class ArbitraryNegativeIntegers
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(-60, -30).ToArbitrary();
}
