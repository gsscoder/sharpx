using FsCheck;
using SharpX;

static class ArbitraryStringsWithNull
{
    public static Arbitrary<string> StringGenerator() => Gen.OneOf(
        Gen.Constant<string>(null),
        Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
