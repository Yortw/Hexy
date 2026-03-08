# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - TBD

### Added

- `Hex` readonly struct — immutable hexadecimal byte sequence type
- Permissive parsing: plain, `0x`/`0X`/`#` prefixed, colon/dash/space separated, mixed case
- 10 built-in `HexFormat` instances plus custom format support
- `HexDefaults` for configurable application-wide format defaults with `Lock()` support
- `HexBuilder` for efficient incremental construction
- Equality, comparison, and hashing (`IEquatable<Hex>`, `IComparable<Hex>`)
- Comparison operators (`==`, `!=`, `<`, `>`, `<=`, `>=`)
- Concatenation (`+` operator, `Append`, `Concat`)
- `Slice`, `Reverse`, `StartsWith`, `EndsWith`, `Contains`
- Zero-copy access: `AsSpan()`, `AsMemory()`, implicit `ReadOnlyMemory<byte>`
- `ToByteArray()`, `TryWriteBytes(Span<byte>)`
- `CreateRandom(int)` via `RandomNumberGenerator`
- `FromGuid` / `ToGuid` interop
- `System.Text.Json` converter (built-in)
- `TypeConverter` for model binding support
- `IConvertible` (outbound: string, byte[])
- `IFormattable` with format specifier strings
- Stream extensions: `ReadHex`, `WriteHex`, async variants, `BinaryReader`/`BinaryWriter`
- .NET 9+ enhancements: `ISpanFormattable`, `IUtf8SpanFormattable`, `IParsable<Hex>`, `ISpanParsable<Hex>`
- Platform-specific optimizations via conditional compilation
- `DebuggerDisplay` attribute for IDE experience
- Multi-targeting: .NET Standard 2.0, .NET 9.0, .NET 10.0
- SourceLink and symbol package (snupkg) support
- Comprehensive XML documentation on all public members
- BenchmarkDotNet benchmark project
