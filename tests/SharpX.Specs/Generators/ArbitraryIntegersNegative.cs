using FsCheck;

static class ArbitraryIntegersNegative
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(-60, -30).ToArbitrary();
}
