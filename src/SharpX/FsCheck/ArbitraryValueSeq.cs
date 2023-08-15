using System.Security.Cryptography;
using FsCheck;
using FsCheck.Fluent;
using SharpX.Extensions;

namespace SharpX.FsCheck;

static class ArbitraryValueSeq
{
    public static Arbitrary<object> Generator()
    {
        var seq = (new object[] { false, true }
          .Concat(Primitives.GenerateSeq<object>(() => RandomNumberGenerator.GetInt32(0, int.MaxValue), count: 19))
          .Concat(Primitives.GenerateSeq<object>(() => RandomNumberGenerator.GetInt32(1, int.MaxValue) * .5, count: 19))
          .Concat(Enumerable.Range(0, 19).Select(x => (object)DateTime.Now.AddDays(x)))
          .Concat(Primitives.GenerateSeq(() => (object)Strings.Generate(9), count: 20))
          .Concat(Enumerable.Range(0, 19).Select(x => new { Foo = x }))
          ).Shuffle();

        return Gen.OneOf(from x in seq select Gen.Constant(x)).ToArbitrary();
    }
}
