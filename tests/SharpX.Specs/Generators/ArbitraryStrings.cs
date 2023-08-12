using FsCheck;
using SharpX;

static class ArbitraryStrings
{
    public static Arbitrary<string> StringGenerator() => Gen.OneOf(Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
