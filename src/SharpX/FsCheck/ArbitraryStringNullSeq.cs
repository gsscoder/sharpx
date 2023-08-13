using FsCheck;

namespace SharpX.FsCheck;

static class ArbitraryStringNullSeq
{
    public static Arbitrary<string[]> StringSeqGenerator() => Gen.Shuffle(new [] {
            string.Empty, "one", "1", "two", "2", "three", "3", null, "four", "4", string.Empty, "five",
            "5", "six", "6", null, "seven", "7", "eight", "8", "nine", "9", null, "ten", "10", string.Empty})
            .ToArbitrary();
}
