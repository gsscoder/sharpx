using FsCheck;

static class ArbitraryPositiveIntegers
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(1, 60).ToArbitrary();
}
