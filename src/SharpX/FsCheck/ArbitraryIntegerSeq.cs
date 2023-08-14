using FsCheck;
using Microsoft.FSharp.Collections;

namespace SharpX.FsCheck;

static class ArbitraryIntegerSeq
{
    public static Arbitrary<int[]> Generator() => Gen.ArrayOf(30,
        Gen.Choose(-30, 30)).ToArbitrary();
}
