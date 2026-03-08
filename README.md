# Yort.Hexy

[![NuGet](https://img.shields.io/nuget/v/Yort.Hexy.svg)](https://www.nuget.org/packages/Yort.Hexy/)
[![Build](https://github.com/Yortw/Yort.Hexy/actions/workflows/ci.yml/badge.svg)](https://github.com/Yortw/Yort.Hexy/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

An immutable .NET value type for hexadecimal byte sequences — a proper domain type for hex data instead of raw `byte[]` or `string`. Parse any common hex format, format with full control, compare with value equality, and use as dictionary keys without writing utility code.

## Why Yort.Hexy?

The .NET BCL has `Convert.ToHexString` for one-off conversions, but if hex values are part of your domain — identifiers, protocol fields, device addresses, cryptographic hashes — you quickly run into friction:

| Pain point | How `Hex` solves it |
|---|---|
| **`byte[]` has reference equality** — breaks dictionary keys, requires `SequenceEqual` everywhere | `Hex` has value equality, implements `IEquatable<Hex>`, and works correctly as a dictionary or `HashSet` key |
| **No standard type for "a hex value"** — you pass `string` or `byte[]` and hope callers know the contract | `Hex` is a self-describing, immutable value type with explicit conversions in both directions |
| **Parsing is fragile** — different APIs for `0x` prefixes, separators, mixed case; easy to get wrong | `Hex.Parse` and `TryParse` accept all common formats permissively: `"DEADBEEF"`, `"0xdeadbeef"`, `"DE:AD:BE:EF"`, `"#FF00AA"`, mixed case, whitespace |
| **Formatting requires ceremony** — `BitConverter.ToString` gives you `"DE-AD-BE-EF"` and nothing else without manual work | 10 built-in `HexFormat` presets plus custom separator/prefix/casing combos; `ToString()` just works |
| **You keep writing `HexUtils`** — every project ends up with the same conversion/comparison helpers | This *is* that utility class, tested (248 tests) and optimised, as a NuGet package |

## When to Use This

**Use Yort.Hexy when** hex values are part of your domain model and you want a proper type instead of stringly-typed code. Common in RFID/NFC, network protocols, cryptography, IoT, smart cards, and binary format parsing.

**Just use `Convert.ToHexString`** if you need a one-off byte-array-to-string conversion and don't need equality, comparison, or rich formatting.

## Features

- **Immutable `Hex` value type** — `readonly struct` backed by `byte[]`, safe to pass around and use as dictionary keys
- **Permissive parsing** — handles `"DEADBEEF"`, `"0xDEADBEEF"`, `"#FF00AA"`, `"DE:AD:BE:EF"`, `"DE-AD-BE-EF"`, `"DE AD BE EF"`, mixed case, whitespace
- **Rich formatting** — 10+ built-in formats, custom separator/prefix/casing combos, configurable defaults
- **Fast comparison & hashing** — SIMD-accelerated on .NET 9+, suitable for high-throughput dictionary lookups
- **Zero-copy access** — `AsSpan()`, `AsMemory()`, implicit `ReadOnlyMemory<byte>` conversion
- **Full .NET integration** — `IEquatable<T>`, `IComparable<T>`, `IFormattable`, `IConvertible`, `ISpanFormattable` (.NET 9+), `IParsable<T>` (.NET 9+)
- **JSON ready** — built-in `System.Text.Json` converter with permissive deserialization
- **Multi-target** — .NET Standard 2.0, .NET 9.0, .NET 10.0 with platform-specific optimizations

## Quick Start

```csharp
using Yort.Hexy;

// Parse from any common hex format
Hex value = Hex.Parse("0xDEADBEEF");
Hex fromColons = Hex.Parse("DE:AD:BE:EF");
Hex fromBytes = new Hex(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

// All three are equal
Debug.Assert(value == fromColons);
Debug.Assert(value == fromBytes);

// Format in different styles
Console.WriteLine(value.ToString(HexFormat.Uppercase));       // "DEADBEEF"
Console.WriteLine(value.ToString(HexFormat.LowercaseColons)); // "de:ad:be:ef"
Console.WriteLine(value.ToString(HexFormat.LowercasePrefixed)); // "0xdeadbeef"

// Use as dictionary key
var lookup = new Dictionary<Hex, string>
{
    [Hex.Parse("CAFEBABE")] = "Java class file"
};

// Zero-copy byte access
ReadOnlySpan<byte> span = value.AsSpan();
ReadOnlyMemory<byte> memory = value; // implicit conversion

// Generate random hex values
Hex nonce = Hex.CreateRandom(16);

// Safe parsing
if (Hex.TryParse(userInput, out Hex parsed))
    Console.WriteLine(parsed);
```

## Formatting

### Built-in Formats

| Format | Example Output |
|--------|---------------|
| `HexFormat.Lowercase` | `deadbeef` |
| `HexFormat.Uppercase` | `DEADBEEF` |
| `HexFormat.LowercasePrefixed` | `0xdeadbeef` |
| `HexFormat.UppercasePrefixed` | `0XDEADBEEF` |
| `HexFormat.LowercaseColons` | `de:ad:be:ef` |
| `HexFormat.UppercaseColons` | `DE:AD:BE:EF` |
| `HexFormat.LowercaseDashes` | `de-ad-be-ef` |
| `HexFormat.UppercaseDashes` | `DE-AD-BE-EF` |
| `HexFormat.LowercaseSpaces` | `de ad be ef` |
| `HexFormat.UppercaseSpaces` | `DE AD BE EF` |

### Custom Formats

```csharp
var dotted = new HexFormat(HexLetterCase.Lower, separator: ".", prefix: "0x");
Console.WriteLine(value.ToString(dotted)); // "0xde.ad.be.ef"
```

### Application Defaults

```csharp
// Set once at startup
HexDefaults.Format = HexFormat.UppercaseColons;
HexDefaults.Lock(); // Prevent further changes

// All ToString() calls now use the configured default
Console.WriteLine(value.ToString()); // "DE:AD:BE:EF"
```

## Operations

```csharp
// Concatenation
Hex combined = a + b;
Hex appended = a.Append(b);
Hex multi = Hex.Concat(a, b, c, d);

// Slicing
Hex slice = value.Slice(1, 2);

// Pattern matching
bool starts = value.StartsWith(Hex.Parse("DEAD"));
bool ends = value.EndsWith(Hex.Parse("BEEF"));
bool has = value.Contains(Hex.Parse("ADBE"));

// Reverse (endianness flip)
Hex reversed = value.Reverse();

// Efficient building
var builder = new HexBuilder();
builder.Append(chunk1);
builder.Append(chunk2);
Hex result = builder.ToHex(); // Single allocation
```

## Conversions

```csharp
// Explicit from byte[]
Hex hex = (Hex)new byte[] { 0xCA, 0xFE };

// Explicit copy to byte[]
byte[] bytes = hex.ToByteArray();

// Zero-allocation write to buffer
Span<byte> buffer = stackalloc byte[16];
hex.TryWriteBytes(buffer);

// Guid interop
Hex fromGuid = Hex.FromGuid(myGuid);
Guid backToGuid = fromGuid.ToGuid();

// String casts
Hex parsed = (Hex)"0xDEADBEEF";
string formatted = (string)hex;
```

## Stream Extensions

```csharp
// Synchronous
stream.WriteHex(myHex);
Hex read = stream.ReadHex(byteCount: 16);

// Async
await stream.WriteHexAsync(myHex, cancellationToken);
Hex readAsync = await stream.ReadHexAsync(16, cancellationToken);

// BinaryReader/Writer
writer.Write(myHex);
Hex fromReader = reader.ReadHex(16);
```

## JSON Serialization

`Hex` works automatically with `System.Text.Json`:

```csharp
var obj = new MyModel { Id = Hex.Parse("DEADBEEF") };
string json = JsonSerializer.Serialize(obj);   // {"Id":"deadbeef"}
var back = JsonSerializer.Deserialize<MyModel>(json);
```

Deserialization is permissive — it accepts all the same formats as `Hex.Parse`.

## Performance

Representative benchmarks on .NET 10 (ShortRun, Intel i7-11850H):

| Operation | Mean | Allocated |
|-----------|-----:|----------:|
| `Equals` (16 B) | 0.5 ns | 0 B |
| `StartsWith` (64 B vs 16 B) | 1.7 ns | 0 B |
| `CompareTo` (16 B vs 64 B) | 1.7 ns | 0 B |
| `GetHashCode` (16 B) | 5.7 ns | 0 B |
| `ToString` lowercase (16 B) | 16 ns | 88 B |
| `Append` (16 B + 16 B) | 21 ns | 56 B |
| `Reverse` (16 B) | 30 ns | 40 B |
| `Concat` (4 × 16 B) | 61 ns | 176 B |
| `Constructor` from byte[] (16 B) | 71 ns | 40 B |
| `CreateRandom` (16 B) | 86 ns | 40 B |
| `Parse` plain (16 B) | 200 ns | 128 B |
| `Parse` prefixed (16 B) | 207 ns | 128 B |
| `Parse` separated (16 B) | 217 ns | 128 B |

Comparison, pattern matching, and hashing are zero-allocation. Run benchmarks yourself for your hardware:

```bash
cd benchmarks/Yort.Hexy.Benchmarks
dotnet run -c Release --framework net10.0 -- --filter "*"
```

## Target Frameworks

| Framework | Notes |
|-----------|-------|
| .NET Standard 2.0 | Broad compatibility. Uses `System.Memory` and manual lookup tables. |
| .NET 9.0 | `Convert.ToHexString`, `Span.SequenceCompareTo`, `HashCode.AddBytes`, `ISpanFormattable`, `IParsable<T>` |
| .NET 10.0 | Latest runtime optimizations |

Platform-specific optimizations are selected automatically at compile time.

## Installation

```
dotnet add package Yort.Hexy
```

## Building from Source

```bash
git clone https://github.com/Yortw/Yort.Hexy.git
cd Yort.Hexy
dotnet build
dotnet test
```

## Running Benchmarks

```bash
cd benchmarks/Yort.Hexy.Benchmarks
dotnet run -c Release --framework net10.0 -- --filter "*"
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT — see [LICENSE](LICENSE) for details.
