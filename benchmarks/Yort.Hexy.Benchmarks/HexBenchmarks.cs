using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Yort.Hexy;

BenchmarkSwitcher.FromAssembly(typeof(Yort.Hexy.Benchmarks.HexBenchmarks).Assembly).Run(args);

namespace Yort.Hexy.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class HexBenchmarks
    {
        private byte[] _bytes16 = null!;
        private byte[] _bytes64 = null!;
        private string _hexString16 = null!;
        private string _hexString64 = null!;
        private string _hexPrefixed = null!;
        private string _hexSeparated = null!;
        private Hex _hex16;
        private Hex _hex64;

        [GlobalSetup]
        public void Setup()
        {
            _bytes16 = new byte[16];
            _bytes64 = new byte[64];
            new Random(42).NextBytes(_bytes16);
            new Random(42).NextBytes(_bytes64);

            _hex16 = new Hex(_bytes16);
            _hex64 = new Hex(_bytes64);
            _hexString16 = _hex16.ToString(HexFormat.Lowercase);
            _hexString64 = _hex64.ToString(HexFormat.Lowercase);
            _hexPrefixed = "0x" + _hexString16;
            _hexSeparated = _hex16.ToString(HexFormat.UppercaseColons);
        }

        // -------------------------------------------------------------------
        // Parsing
        // -------------------------------------------------------------------

        [Benchmark]
        public Hex Parse_Plain_16() => Hex.Parse(_hexString16);

        [Benchmark]
        public Hex Parse_Plain_64() => Hex.Parse(_hexString64);

        [Benchmark]
        public Hex Parse_Prefixed() => Hex.Parse(_hexPrefixed);

        [Benchmark]
        public Hex Parse_Separated() => Hex.Parse(_hexSeparated);

        [Benchmark]
        public bool TryParse_16() => Hex.TryParse(_hexString16, out _);

        // -------------------------------------------------------------------
        // Formatting
        // -------------------------------------------------------------------

        [Benchmark]
        public string ToString_Lowercase_16() => _hex16.ToString(HexFormat.Lowercase);

        [Benchmark]
        public string ToString_Lowercase_64() => _hex64.ToString(HexFormat.Lowercase);

        [Benchmark]
        public string ToString_Uppercase_16() => _hex16.ToString(HexFormat.Uppercase);

        [Benchmark]
        public string ToString_Colons_16() => _hex16.ToString(HexFormat.UppercaseColons);

        [Benchmark]
        public string ToString_Cached_16() => Hex.Parse(_hexString16).ToString(HexFormat.Lowercase);

        // -------------------------------------------------------------------
        // Comparison
        // -------------------------------------------------------------------

        [Benchmark]
        public bool Equals_16() => _hex16.Equals(_hex16);

        [Benchmark]
        public int GetHashCode_16() => _hex16.GetHashCode();

        [Benchmark]
        public int CompareTo_16() => _hex16.CompareTo(_hex64);

        // -------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------

        [Benchmark]
        public Hex Constructor_ByteArray_16() => new Hex(_bytes16);

        [Benchmark]
        public Hex CreateRandom_16() => Hex.CreateRandom(16);

        // -------------------------------------------------------------------
        // Operations
        // -------------------------------------------------------------------

        [Benchmark]
        public Hex Append() => _hex16.Append(_hex16);

        [Benchmark]
        public Hex Concat_Four() => Hex.Concat(_hex16, _hex16, _hex16, _hex16);

        [Benchmark]
        public byte[] ToByteArray_16() => _hex16.ToByteArray();

        [Benchmark]
        public Hex Reverse_16() => _hex16.Reverse();

        [Benchmark]
        public bool StartsWith_16() => _hex64.StartsWith(_hex16);
    }
}
