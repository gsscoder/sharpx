using FsCheck;
using Microsoft.FSharp.Collections;

static class ArbitraryIntegersSeq
{
    public static Arbitrary<FSharpList<int>> IntegerListGenerator() => Gen.ListOf(30,
        Gen.Choose(-30, 30)).ToArbitrary();
}
