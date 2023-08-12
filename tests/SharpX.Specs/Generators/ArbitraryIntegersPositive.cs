using FsCheck;

static class ArbitraryIntegersPositive
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(1, 60).ToArbitrary();
}
