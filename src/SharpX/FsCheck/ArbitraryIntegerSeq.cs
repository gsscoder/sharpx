using FsCheck;
using Microsoft.FSharp.Collections;

namespace SharpX.FsCheck;

static class ArbitraryIntegerSeq
{
    public static Arbitrary<FSharpList<int>> Generator() => Gen.ListOf(30,
        Gen.Choose(-30, 30)).ToArbitrary();
}
