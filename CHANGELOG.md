# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
