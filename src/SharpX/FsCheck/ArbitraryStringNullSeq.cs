using FsCheck;
using SharpX.Extensions;

namespace SharpX.FsCheck;

static class ArbitraryStringNullSeq
{
    public static Arbitrary<string?[]> StringSeqGenerator()
    {
        var seq = Primitives.GenerateSeq(() => Strings.Generate(9), count: 20)
                  .Concat(Enumerable.Range(0, 9).Select(x => x.ToString()))
                  .Shuffle()
                  .Intersperse(null)
                  .Intersperse(string.Empty);

        return Gen.Shuffle(seq).ToArbitrary();
    }
}
