# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.3.6] - 2025-04-06

- Added agressive inlining for `Maybe` primitives.
- Set `ToMaybe` parameter as nullable.

## [8.3.4] - 2025-03-29

- Added `Primitives::SafeInvoke` overload.

## [8.3.2] - 2025-03-29

- Implemented `Primitives::SafeInvoke` method.

## [8.3.0] - 2025-02-23

- Removed `Outcome` type from class library.
- Reorganized result types source files.
- Fixed compiler warnings in Maybe implementation.
- Fixed compiler warnings in Either implementation.
- Set Unit type as readonly struct.
- Fixed compiler warnings in Result implementation.

## [8.1.0] - 2025-02-09

- Improved `LoggerExtensions` methods.
- Implemented `LoggerExtensions::WarnWith` method.
- Updated `LoggerExtensions` methods signature and parameters validation.
- Added `LoggerExtensions` methods for handling booleans.

## [8.0.0] - 2025-02-05

- Updated .NET targets and dependencies.
- Implemented `Guard::DisallowNothing` method.

## [6.4.6] - 2024-06-01

- Added `LoggerExtensions` class.
- Added `EnumerableExtensions::Batch` method from MoreLINQ.
- Implemented `UnitExtensions::And` method.

## [6.4.2] - 2024-01-14

- Implemented string utility methods to remove diacritics.

## [6.4.0] - 2024-01-14

- Renamed method `DisallowAnyEmptyWhitespace` as `DisallowEmptyWhitespace`.
- Used `ArgumentOutOfRangeException` in `Guard` when appropriate.
- Updated `Guard::DisallowMalformedGuid` exception message.
- Target set to .NET Standard 2.0.
- Fixed int genenation in `Primitives::GenerateSeq<T>` method.

## [6.3.6] - 2023-12-18

- Implemented `EnumerableExtensions::DistinctCount` method.
- Fixed missing `this` operator in `ObjectExtensions::IsNumber` method.
- Implemented `StringExtensions::ToUri` method.
- Updated `EqualsIgnoreCase` to campare null values when safe mode is on.
- Made parameter nullable in `StringExtensions::IsEmpty` method.
- Implemented `Strings::ReverseCase` method and relative extension method.
- Implemented `Strings::RandomizeCase` method and relative extension method.
- Made parameter nullable in `StringExtensions::EqualsIgnoreCase` method.

## [6.3.2] - 2023-09-28

- Implemented `NormalizeToEmpty` method and extension method.
- Implemented `IsNumber` method and extension method.
- Fix: `Arbitrary*` types made public.
- Fixed a test.

## [6.3.0] - 2023-08-20

- Improved implementation of `Primitives::ChanceOf`.
- Improved `EnumerableExtensions::Intersperse` to support randomness.
- Improved `EnumerableExtensions::Intersperse` to support nulls.
- Moved FsCheck generators to main project.
- Implemented `Primitives::GenerateSeq` method.
- Refactored FsCheck generators for version 3.0.0-alpha4.
- Updated `Strings::Generate` to function without length parameter.
- Implemented `Primitives::GenerateSeq` overload for int, double and string.
- Fixed `EnumerableExtensions::Choice` extension method.
- Refactored `ArbitraryValue` to generate default values.
- Updated `ArbitraryString` generator to include null values.
- Updated `ArbitraryStringSeq` generator to include null values.
- Updated `ArbitraryIntegerSeq` generator implementation.

## [6.2.1] - 2023-05-26

- Implemented `DisallowEmptyEnumerable` guard method.

## [6.2.0] - 2023-05-26

- Removed UnitExtensions class.
- Implemented `StringExtensions::ToGuid` extension method.
- Reimplemented `SafeSubstring` as `Substring` overload.

## [6.1.0] - 2023-04-25

- Restored target `netcoreapp3.1`.

## [6.0.3] - 2023-04-20

- Implemented `Primitives::ChanceOf` method.

## [6.0.2] - 2023-04-16

- Implemented `StringExtensions::IsEmpty` extension method.
- Implemented `EnumerableExtensions::IsEmpty` extension method.

## [6.0.1] - 2023-04-15

- Set Maybe type as readonly struct.
- Added `Maybe::DoAsync` extension overloads.
- Added `UnitExtensions::ToUnit` extension method.

- Implemented `ObjectExtensions::ToUnit`.

## [6.0.0-preview.1] - 2022-03-29

- Set target to .NET 6.0 only.
- `CryptoRandom` marked obsolete.
- Renamed `Guard::RestrictArraySize` to `DisallowArraySize`.
- Renamed `Guard::AllowGuidOnly` to `DisallowMalformedGuid`.

## [1.1.11] - 2022-03-20

- Implemented `StringExtensions::StartsWithIgnoreCase`.

## [1.1.10] - 2022-03-20

- Implemented `StringExtensions::EqualsIgnoreCase`.
- Implemented `StringExtensions::ContainsIgnoreCase`.

## [1.1.8] - 2022-01-30

- Implemented `EnumerableExtensions::Shuffle` method.
- Fixed and improved `Strings::Generate` method.

## [1.1.7] - 2022-01-30

- Added `Strings::SafeSubstring` method (and relative extension method).
- Fixed `Strings::Generate` method.

## [1.1.6] - 2022-01-22

- Added `Strings::IsEmptyWhitespace` method (and relative extension method).
- Updated `WhiteSpace` to `Whitespace` in `Guard` class.
- Updated `Strings::Mangle` to use all special characters.
- Allowed `Strings::Generate` customization options and prefix input.

## [1.1.5] - 2022-01-16

- Fixed name of `StringExtensions::IsWhitespace` to `ContainsWhitespace`.

## [1.1.4] - 2021-01-12

- Added `Primitives::ToEnumerable` method (and relative extension method).
- Added non-extension version of `ExceptionExtensions::Format` to `Primitives` class.
- Moved `ToEnumerable` method to `EnumerableExtensions`.
- Added `Unit::DoAsync` method.
- Added `EnumerableExtensions::ForEachAsync` method.

## [1.1.2] - 2021-12-19

- Updated `Strings::ContainsSpecialChar`.

## [1.1.1] - 2021-12-19

- Added `IsSpecialChar` method to `Strings` and `StringsExtensions` classes.
- Added `ContainsSpecialChar` method to `Strings` and `StringsExtensions` classes.

## [1.1.1] - 2021-12-18

- Renamed `IsWhitespace` to a more semantically correct `ContainsWhitespace`.

## [1.1.0] - 2021-12-04

- Moved `ToMaybe` `FirstOrNothing`, `LastOrNothing`, `SingleOrNothing`, `ElementAtOrNothing` to `MaybeExtensions` in root namespace.
- Updated `ToUpperFirstLetter` and `ToLowerFirstLetter` and renamed as `ToUpperFirst` and `ToLowerFirst` (`Strings` class).
- Renamed `Strings::IsWhiteSpace` as `IsWhitespace`.
- Added `MaybeExtensions::ToJust` method.
- Updated `Maybe::Equals(object)` implementation.
- Updated `Guard::DisallowDefault`.

## [1.0.5] - 2021-11-28

- Inverted oreder of members in `ResultType`.

## [1.0.4] - 2021-11-28

- Moved classes with extension methods to `SharpX.Extensions` namespace.
- Moved `StringUtil::Generate` to new `Strings` class.
- Moved `CharExtensions::Replicate` as `Strings::ReplicateChar`
- Moved `CharExtensions::Replicate` to `StringExtensions`.
- Removed `CharExtensions` class.
- All `StringExtensions` methods are implemented in `Strings` class.
- Added `Guard` class.
- Consumed `Guard` class extensively in the project.

## [1.0.3] - 2021-11-20

- Renamed `StringExtensions::StripTags` as `StripTag`.

## [1.0.2] - 2021-11-19

- Renamed `StringExtensions::StripML` as `StripTags`.
- Updated implemetation of `Maybe::GetHashCode`.
