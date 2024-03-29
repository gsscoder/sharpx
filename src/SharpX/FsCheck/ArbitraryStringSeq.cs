using FsCheck;
using FsCheck.Fluent;
using SharpX.Extensions;

namespace SharpX.FsCheck;

public static class ArbitraryStringSeq
{
    public static Arbitrary<string?[]> Generator()
    {
        var seq = Primitives.GenerateSeq(() => Strings.Generate(9), count: 20)
                  .Concat(Enumerable.Range(0, 9).Select(x => x.ToString()))
                  .Intersperse(null)
                  .Intersperse(string.Empty);

        return Gen.Shuffle(seq).ToArbitrary();
    }
}
