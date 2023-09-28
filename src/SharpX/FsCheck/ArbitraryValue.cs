using System.Security.Cryptography;
using FsCheck;
using FsCheck.Fluent;
using SharpX.Extensions;

namespace SharpX.FsCheck;

public static class ArbitraryValue
{
    public static Arbitrary<object> Generator()
    {
        var seq = (new object[] { false, true }
          .Concat(Enumerable.Repeat<int>(default, 5).Cast<object>())
          .Concat(Primitives.GenerateSeq<int>(count: 19).Cast<object>())
          .Concat(Primitives.GenerateSeq<double>(count: 19).Cast<object>())
          .Concat(Enumerable.Range(0, 19).Select(x => (object)DateTime.Now.AddDays(x)))
          .Concat(Enumerable.Repeat<string?>(null, 5).Cast<object>())
          .Concat(Primitives.GenerateSeq<string>(count: 10).Cast<object>())
          .Concat(Enumerable.Range(0, 19).Select(x => new { Foo = x }))
          ).Shuffle();

        return Gen.OneOf(from x in seq select Gen.Constant(x)).ToArbitrary();
    }
}
