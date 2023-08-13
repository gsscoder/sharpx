using FsCheck;
using SharpX;

namespace SharpX.FsCheck;

static class ArbitraryStrings
{
    public static Arbitrary<string> StringGenerator() => Gen.OneOf(Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
