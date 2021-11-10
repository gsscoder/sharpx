using FsCheck;

static class ArbitraryIntegers
{
    public static Arbitrary<int> IntegerGenerator() => Gen.Choose(-30, 30).ToArbitrary();
}
