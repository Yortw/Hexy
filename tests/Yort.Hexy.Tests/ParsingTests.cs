using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class ParsingTests
    {
        // -------------------------------------------------------------------
        // Parse — valid inputs
        // -------------------------------------------------------------------

        [Theory]
        [InlineData("deadbeef", 4)]
        [InlineData("DEADBEEF", 4)]
        [InlineData("DeAdBeEf", 4)]
        [InlineData("00", 1)]
        [InlineData("ff", 1)]
        [InlineData("FF", 1)]
        [InlineData("", 0)]
        public void Parse_PlainHex_ReturnsCorrectLength(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("0xDEADBEEF", 4)]
        [InlineData("0XDEADBEEF", 4)]
        [InlineData("0xdeadbeef", 4)]
        [InlineData("0x00", 1)]
        public void Parse_0xPrefix_StripsAndParses(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("#FF00AA", 3)]
        [InlineData("#ff00aa", 3)]
        public void Parse_HashPrefix_StripsAndParses(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("DE:AD:BE:EF", 4)]
        [InlineData("de:ad:be:ef", 4)]
        public void Parse_ColonSeparated_StripsAndParses(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("DE-AD-BE-EF", 4)]
        [InlineData("de-ad-be-ef", 4)]
        public void Parse_DashSeparated_StripsAndParses(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("DE AD BE EF", 4)]
        [InlineData("de ad be ef", 4)]
        public void Parse_SpaceSeparated_StripsAndParses(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Theory]
        [InlineData("  DEADBEEF  ", 4)]
        [InlineData("\t DEADBEEF \n", 4)]
        public void Parse_WhitespacePadding_Trims(string input, int expectedLength)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
        }

        [Fact]
        public void Parse_EmptyString_ReturnsEmpty()
        {
            var hex = Hex.Parse("");
            Assert.True(hex.IsEmpty);
            Assert.Equal(0, hex.Length);
        }

        [Fact]
        public void Parse_AllFormatsProduceSameBytes()
        {
            var plain = Hex.Parse("DEADBEEF");
            var prefixed = Hex.Parse("0xDEADBEEF");
            var hashed = Hex.Parse("#DEADBEEF");
            var colons = Hex.Parse("DE:AD:BE:EF");
            var dashes = Hex.Parse("DE-AD-BE-EF");
            var spaces = Hex.Parse("DE AD BE EF");
            var lower = Hex.Parse("deadbeef");

            Assert.Equal(plain, prefixed);
            Assert.Equal(plain, hashed);
            Assert.Equal(plain, colons);
            Assert.Equal(plain, dashes);
            Assert.Equal(plain, spaces);
            Assert.Equal(plain, lower);
        }

        [Fact]
        public void Parse_CorrectByteValues()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.Equal(0xDE, hex[0]);
            Assert.Equal(0xAD, hex[1]);
            Assert.Equal(0xBE, hex[2]);
            Assert.Equal(0xEF, hex[3]);
        }

        // -------------------------------------------------------------------
        // Parse — invalid inputs
        // -------------------------------------------------------------------

        [Fact]
        public void Parse_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Hex.Parse(null!));
        }

        [Theory]
        [InlineData("XYZ")]
        [InlineData("GHIJ")]
        [InlineData("deadbeefgg")]
        [InlineData("0xZZ")]
        public void Parse_InvalidChars_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Hex.Parse(input));
            Assert.Contains("Invalid hexadecimal character", ex.Message);
        }

        [Theory]
        [InlineData("DEA")]
        [InlineData("DEADB")]
        [InlineData("0xDEA")]
        [InlineData("F")]
        public void Parse_OddLength_ThrowsFormatException(string input)
        {
            var ex = Assert.Throws<FormatException>(() => Hex.Parse(input));
            Assert.Contains("odd number of characters", ex.Message);
        }

        // -------------------------------------------------------------------
        // TryParse
        // -------------------------------------------------------------------

        [Fact]
        public void TryParse_ValidInput_ReturnsTrue()
        {
            Assert.True(Hex.TryParse("DEADBEEF", out var result));
            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void TryParse_InvalidInput_ReturnsFalse()
        {
            Assert.False(Hex.TryParse("not hex", out var result));
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void TryParse_Null_ReturnsFalse()
        {
            Assert.False(Hex.TryParse(null, out var result));
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void TryParse_OddLength_ReturnsFalse()
        {
            Assert.False(Hex.TryParse("DEA", out _));
        }

        [Fact]
        public void TryParse_Empty_ReturnsEmptyHex()
        {
            Assert.True(Hex.TryParse("", out var result));
            Assert.True(result.IsEmpty);
        }

        // -------------------------------------------------------------------
        // String caching
        // -------------------------------------------------------------------

        [Fact]
        public void Parse_CachesNormalizedLowercaseString()
        {
            var hex = Hex.Parse("DEADBEEF");
            // The cached string should be returned for lowercase format
            string s1 = hex.ToString(HexFormat.Lowercase);
            string s2 = hex.ToString(HexFormat.Lowercase);
            Assert.Same(s1, s2); // Same string instance = cached
        }

        // -------------------------------------------------------------------
        // Explicit cast from string
        // -------------------------------------------------------------------

        [Fact]
        public void ExplicitCast_FromString_ParsesCorrectly()
        {
            Hex hex = (Hex)"0xDEADBEEF";
            Assert.Equal(4, hex.Length);
        }

        [Fact]
        public void ExplicitCast_FromInvalidString_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => (Hex)"not hex");
        }

#if NET9_0_OR_GREATER
        // -------------------------------------------------------------------
        // Span-based parsing (.NET 9+)
        // -------------------------------------------------------------------

        [Fact]
        public void Parse_Span_ValidInput()
        {
            ReadOnlySpan<char> input = "DEADBEEF".AsSpan();
            var hex = Hex.Parse(input);
            Assert.Equal(4, hex.Length);
        }

        [Fact]
        public void TryParse_Span_ValidInput()
        {
            Assert.True(Hex.TryParse("0xCAFE".AsSpan(), out var result));
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void TryParse_Span_InvalidInput()
        {
            Assert.False(Hex.TryParse("ZZZ".AsSpan(), out _));
        }
#endif
    }
}
