using FsCheck;
using SharpX;

namespace SharpX.FsCheck;

static class ArbitraryStringNull
{
    public static Arbitrary<string> StringGenerator() => Gen.OneOf(
        Gen.Constant<string>(null),
        Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
