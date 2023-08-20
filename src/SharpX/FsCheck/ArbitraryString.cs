using FsCheck;
using FsCheck.Fluent;

namespace SharpX.FsCheck;

static class ArbitraryString
{
    public static Arbitrary<string> Generator() => Gen.OneOf(Gen.Constant<string>(null),
        Gen.Constant(Strings.Generate(9))).ToArbitrary();
}
