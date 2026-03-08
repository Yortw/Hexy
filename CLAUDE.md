# CLAUDE.md — Yort.Hexy

## Project Overview

Yort.Hexy is a .NET library providing `Hex`, an immutable `readonly struct` for hexadecimal byte sequences. It targets .NET Standard 2.0, .NET 9.0, and .NET 10.0 with platform-specific optimizations via `#if` guards.

## Build & Test

```bash
dotnet build                                          # Build everything
dotnet test tests/Yort.Hexy.Tests                     # Run tests (net9.0 + net10.0)
dotnet run -c Release --project benchmarks/Yort.Hexy.Benchmarks -- --filter "*"  # Benchmarks
```

- `TreatWarningsAsErrors=true` — zero warnings required
- `AnalysisLevel=latest-recommended` — full Roslyn analyzers
- `GenerateDocumentationFile=true` — XML docs on all public members (CS1591 = warning)

## Architecture

The library is a single project (`src/Yort.Hexy/`) with partial struct files:

| File | Purpose |
|------|---------|
| `Hex.cs` | Core struct definition, fields (`_bytes`), constructor, `Empty`, `CreateRandom`, debugger display |
| `Hex.Comparison.cs` | `IEquatable<Hex>`, `IComparable<Hex>`, operators, `GetHashCode` |
| `Hex.Conversion.cs` | Explicit operators (byte[], string), `AsSpan`, `AsMemory`, `ToByteArray`, `TryWriteBytes`, Guid interop |
| `Hex.Formatting.cs` | `ToString`, `IFormattable`, `ISpanFormattable` (.NET 9+), `IUtf8SpanFormattable` (.NET 9+) |
| `Hex.Operators.cs` | `+`, `Append`, `Concat`, `Slice`, `Reverse`, `StartsWith`, `EndsWith`, `Contains` |
| `Hex.Parsing.cs` | `Parse`, `TryParse`, `IParsable<T>` (.NET 9+) |
| `Hex.Serialization.cs` | `IConvertible`, `HexJsonConverter`, `HexTypeConverter` |
| `HexFormat.cs` | Format descriptor (letter case, separator, prefix) with 10 built-in instances |
| `HexDefaults.cs` | Application-wide default format with `Lock()`/`Reset()` |
| `HexBuilder.cs` | Mutable builder analogous to `StringBuilder` |
| `HexStreamExtensions.cs` | Stream/BinaryReader/BinaryWriter extensions (sync + async) |
| `Internal/HexDecoder.cs` | Parsing implementation (permissive, handles prefixes/separators) |
| `Internal/HexEncoder.cs` | Encoding implementation (lookup tables on netstandard2.0, `Convert.ToHexString` on .NET 9+) |
| `Internal/ThrowHelper.cs` | Centralised throw helpers for JIT optimisation |

## Key Design Decisions

- **Internal `new Hex(byte[], null)` constructor** bypasses the copy that the public `new Hex(byte[])` constructor performs. Used only when the caller already owns the array (e.g., freshly allocated in `Parse`, `Concat`, `Slice`). The `null` parameter is a sentinel — it has no other meaning.
- **`BytesOrEmpty` property** returns `_bytes ?? Array.Empty<byte>()` to handle `default(Hex)` safely throughout the codebase.
- **JSON serialization always uses canonical lowercase** (no prefix, no separators) regardless of `HexDefaults.Format`, to guarantee round-trip fidelity.
- **`byte[]` conversion is explicit**, not implicit, because the constructor can throw `ArgumentNullException`.

## Conditional Compilation

Three target frameworks with `#if` guards:

- `NET9_0_OR_GREATER` — `Convert.ToHexString`, `ISpanFormattable`, `IParsable<T>`, `ReadOnlySpan<Hex>` overloads, `Guid(ReadOnlySpan<byte>)`
- `NET6_0_OR_GREATER` — `Stream.ReadAsync(Memory<byte>)`, `ArgumentNullException.ThrowIfNull`
- `NET10_0_OR_GREATER` — reserved for future optimisations (currently `NET9_0_OR_GREATER` covers .NET 10)

**Important**: `System.Memory` NuGet package provides `Span<T>` APIs (`SequenceEqual`, `SequenceCompareTo`, `IndexOf`) on netstandard2.0 — do not wrap these in `#if` guards.

## Coding Conventions

- Full braces always (no braceless `if`/`using`)
- `var` only when type is apparent; explicit types for built-in types
- Private fields: `_camelCase` prefix
- Nullable reference types enabled project-wide
- No implicit usings
- Expression-bodied members for single-line properties/methods
- `(uint)` cast trick for range validation (catches negative and too-large in one comparison)
- `checked()` arithmetic at overflow-prone call sites (e.g., `HexBuilder.Append`, `Hex.Concat`)
- `ThrowHelper` for all throws in hot paths (keeps methods small for JIT inlining)

## Testing

- xUnit 2.x, targeting net9.0 + net10.0
- 248 tests covering parsing, formatting, comparison, conversion, serialization, streaming, builder, and operators
- Test naming: `MethodName_Scenario_ExpectedBehavior`
- `[Fact]` for single scenarios, `[Theory]` with `[InlineData]` for parametrised tests
- `HexDefaults.Reset()` used in tests that modify global state

## Common Pitfalls

- Don't use `implicit` operators that can throw — use `explicit`
- Don't forget `ConfigureAwait(false)` on async calls in library code
- Don't use `HexDefaults.Format` in serialization — use `HexFormat.Lowercase` for canonical output
- When adding new `Hex` methods that return byte arrays, use `new Hex(result, null)` to avoid double-copy
