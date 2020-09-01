# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [3.0.1] - 2020-09-01
### Fixed
- `IgnoreList.IsAnyParentDirectoryIgnored` no longer throws if you pass it a non-relative path (thanks, [@nojaf](https://github.com/nojaf))

## [3.0.0] - 2019-03-13
### Changed
- Removed C# translation of GPL `wildmatch.c` code (which was technically a license violation)
- Memory consumption and GC activity have been dramatically reduced
- Library now passes all current `.gitignore` tests ([see here](https://github.com/git/git/blob/master/t/t3070-wildmatch.sh))

## [2.0.1] - 2019-03-10
### Fixed
- Release library is no longer built in `Debug` configuration.

## [2.0.0] - 2018-10-26
### Changed
- Library is now strong-named.
### Added
- We now support `[:graph:]` & `[:print:]` classifications (thanks: [@atifaziz](https://github.com/atifaziz)).

## [1.4.0] - 2018-05-15
### Changed
- Library now targets .NET Standard 1.3 in addition to .NET Framework 3.5.

## [1.3.0] - 2017-01-30
### Added
- Adds line number logging (for rules which have been loaded using overloads which take a file path).

## [1.2.2] - 2016-10-18
### Changed
- Improve match logging.

### Fixed
- Fixed a number of obscure bugs which were picked up by testing against a reference C version of the `wildmatch` function.

## [1.2.1] - 2016-06-25
### Fixed
- Fix incorrect behaviour of parent directory ignores.
- Patterns which don't contain any slashes no longer match any path which *ends* with the pattern.

## [1.2.0] - 2016-06-23
### Changed
- You can now specify `MatchFlags` when creating an `IgnoreList`.

## [1.1.0] - 2016-06-14
### Changed
- Library now uses a C# port of the git `wildmatch` function for matching.

## [1.0.3] - 2016-06-07
### Changed
- NuGet package now includes an XML documentation file.

## [1.0.2] - 2015-06-06
### Fixed
- Fixed over-eager wildcard matching (e.g. `*.cs` also matched `*.cshtml`).

## [1.0.1] - 2016-05-31
### Changed
- Negation pattern rules now return a match in the same way as normal rules; it's up to the consuming code to treat them differently if desired, by checking the value of the Negation property. This improves match logging and works more along the lines of the Git matching engine.

## [1.0.0] - 2016-05-31
Initial release.

[Unreleased]: https://github.com/markashleybell/MAB.DotIgnore/compare/v3.0.0...HEAD
[3.0.1]: https://github.com/markashleybell/MAB.DotIgnore/compare/v3.0.0...v3.0.1
[3.0.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v2.0.1...v3.0.0
[2.0.1]: https://github.com/markashleybell/MAB.DotIgnore/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.4.0...v2.0.0
[1.4.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.2.2...v1.3.0
[1.2.2]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.0.3...v1.1.0
[1.0.3]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/markashleybell/MAB.DotIgnore/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/markashleybell/MAB.DotIgnore/compare/66fd7dd0538d68998ba97df0d1ddf4589f7b0b43...v1.0.0

