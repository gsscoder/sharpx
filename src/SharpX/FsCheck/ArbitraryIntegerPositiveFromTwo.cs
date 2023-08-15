using FsCheck;
using FsCheck.Fluent;

namespace SharpX.FsCheck;

static class ArbitraryIntegerPositiveFromTwo
{
    public static Arbitrary<int> Generator() => Gen.Choose(2, 102).ToArbitrary();
}
