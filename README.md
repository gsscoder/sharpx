
# SharpX

![alt text](/assets/icon.png "SharpX Logo")

SharpX is derived from [CSharpx](https://github.com/gsscoder/csharpx) 2.8.0-rc.2 (_which was practically a stable_) and [RailwaySharp](https://github.com/gsscoder/railwaysharp) 1.2.2. While both projects were meant mainly for source inclusion, SharpX is designed to be pulled from [NuGet](https://www.nuget.org/).

The library contains **functional types** and other utilities, including **test oriented tools**. It follows the _don't reinvent the wheel_ philosophy. This project was originally inspired by [Real-World Functional Programming](https://www.amazon.com/Real-World-Functional-Programming-Tomas-Petricek/dp/1933988924/ref=sr_1_1?keywords=Real-World+Functional+Programming&qid=1580118924&s=books&sr=1-1) and includes code from [MoreLINQ](https://github.com/morelinq/MoreLINQ).

## Target

- .NET Standard 2.0

## Install via NuGet

You can install it via NuGet:

```sh
$ dotnet add package SharpX --version 6.4.0
  Determining projects to restore...
  ...
```

## Overview

All types are available in the root namespace. Extension methods (except the very specific ones) are in `SharpX.Extensions`.
`SharpX.FsCheck` contains [FsCheck](https://github.com/fscheck/FsCheck) generators for property-based testing.

## [Maybe]

- Encapsulates an optional value that can contain a value or being empty.
- Similar to F# `'T option` / Haskell `data Maybe a = Just a | Nothing` type.

```csharp
var greet = true;
var value = greet ? "world".ToMaybe() : Maybe.Nothing<string>();
value.Match(
    who => Console.WriteLine($"hello {who}!"),
    () => Environment.Exit(1));
```

- Supports LINQ syntax:

```csharp
var result1 = (30).ToJust();
var result2 = (10).ToJust();
var result3 = (2).ToJust();

var sum = from r1 in result1
          from r2 in result2
          where r1 > 0
          select r1 - r2 into temp
          from r3 in result3
          select temp * r3;

var value = sum.FromJust(); // outcome: 40
```

- Features sequence extensions:

```csharp
var maybeFirst = new int[] {0, 1, 2}.FirstOrNothing(x => x == 1)
// outcome: Just(1)
```

## Either

- Represents a value that can contain either a value or an error.
- Similar to Haskell `data Either a b = Left a | Right b` type.
- Similar also to F# `Choice<'T, 'U>`.
- Like in Haskell the convention is to let `Right` case hold the value and `Left` keep track of error or similar data.
- If you want a more complete implementation of this kind of types, consider using `Result`.

## Result

This type was originally present in RailwaySharp. Check the test project to see a more complete usage example.

``` csharp
public static Result<Request, string> ValidateInput(Request input)
{
    if (input.Name == string.Empty) {
        return Result<Request, string>.FailWith("Name must not be blank");
    }
    if (input.EMail == string.Empty) {
        return Result<Request, string>.FailWith("Email must not be blank");
    }
    return Result<Request, string>.Succeed(input);
}

var request = new Request { Name = "Giacomo", EMail = "gsscoder@gmail.com" };
var result = Validation.ValidateInput(request);
result.Match(
    (x, msgs) => { Logic.SendMail(x.EMail); },
    msgs => { Logic.HandleFailure(msgs) });
```

## Outcome

- Represents a value that can be a success or a failure in form of a type that can contains a custom error message and optionally an exception.

```csharp
Outcome ValidateArtifact(Artifact artifact)
{
    try {
        artifact = ArtifactManager.Load(artifact.Path);
    }
    catch (IOException e) {
        return Result.Failure($"Unable to load artifcat {path}:\n{e.Message}", exception: e);
    }
    return artifact.CheckIntegrity() switch {
        Integrity.Healthy => Outcome.Success(),
        _                 => Outcome.Failure("Artifact integrity is compromised")
    };
}

if (ValidateArtifact(artifact).MatchFailure(out Error error)) {
    //Error::ToString creates a string with message and exception details
    _logger.LogError(error.Exception.FromJust(), error.ToString());
    Environment.Exit(1);
}
// do something useful with artifact
```

## Unit

- `Unit` is similar to `void` but, since it's a **real** type. `void` is not, in fact you can't declare a variable of that type. `Unit` allows the use functions without a result in a computation (**functional style**). It's essentially **F#** `unit` and **Haskell** `Unit`.

```csharp
// prints each word and returns 0 to the shell
static int Main(string[] args)
{
    var sentence = "this is a sentence";
    return (from _ in
            from word in sentence.Split()
            select Unit.Do(() => Console.WriteLine(word))
            select 0).Distinct().Single();
}
```

## FSharpResultExtensions

- Convenient extension methods to consume `FSharpResult<T, TError>` in simple and functional for other **.NET** languages.

```csharp
// pattern match like
var result = Query.GetStockQuote("ORCL");
result.Match(
    quote => Console.WriteLine($"Price: {quote.Price}"),
    error => Console.WriteLine($"Trouble: {error}"));
// mapping
var result = Query.GetIndex(".DJI");
result.Map(
    quote => CurrencyConverter.Change(quote.Price, "$", "€"));
```

- Blog [post](https://gsscoder.github.io/consuming-fsharp-results-in-c/) about it.

## StringExtensions

- General purpose and randomness string manipulation extensions. Few more methods and non-extension version can be found in `Strings` class.

```csharp
Console.WriteLine(
    "\t[hello\world@\t".Sanitize(normalizeWhiteSpace: true));
// outcome: ' hello world '

Console.WriteLine(
    "I want to change a word".ApplyAt(4, word => word.Mangle()));
// outcome like: 'I want to change &a word'
```

## EnumerableExtensions

- Most useful extension methods from [MoreLINQ](https://github.com/morelinq/MoreLINQ).
- Some of these reimplemnted (e.g. `Choose` using `Maybe`):

```csharp
var numbers = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
var evens = numbers.Choose(x => x % 2 == 0
                                ? x.ToJust()
                                : Maybe.Nothing<int>());
// outcome: {0, 2, 4, 6, 8}
```

- With other useful methods too:

```CSharp
var sequence = new int[] {0, 1, 2, 3, 4}.Intersperse(5);
// outcome: {0, 5, 1, 5, 2, 5, 3, 5, 4}
var element = sequence.Choice();
// will choose a random element
var sequence = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ChunkBySize(3);
// outcome: { [0, 1, 2], [3, 4, 5], [6, 7, 8], [9, 10] }
```

## Icon

[Tool](https://thenounproject.com/search/?q=tool&i=3902696) icon designed by Cattaleeya Thongsriphong from [The Noun Project](https://thenounproject.com/)
