using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryString
{
    public static Arbitrary<string> Generator() => Gen.OneOf(Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
