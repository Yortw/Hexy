using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class FormattingTests
    {
        private static readonly Hex Sample = Hex.Parse("DEADBEEF");

        [Fact]
        public void ToString_Default_ReturnsLowercase()
        {
            // Ensure defaults are reset for this test
            HexDefaults.Reset();
            Assert.Equal("deadbeef", Sample.ToString());
        }

        [Fact]
        public void ToString_Lowercase_ReturnsLowercase()
        {
            Assert.Equal("deadbeef", Sample.ToString(HexFormat.Lowercase));
        }

        [Fact]
        public void ToString_Uppercase_ReturnsUppercase()
        {
            Assert.Equal("DEADBEEF", Sample.ToString(HexFormat.Uppercase));
        }

        [Fact]
        public void ToString_LowercasePrefixed()
        {
            Assert.Equal("0xdeadbeef", Sample.ToString(HexFormat.LowercasePrefixed));
        }

        [Fact]
        public void ToString_UppercasePrefixed()
        {
            Assert.Equal("0XDEADBEEF", Sample.ToString(HexFormat.UppercasePrefixed));
        }

        [Fact]
        public void ToString_LowercaseColons()
        {
            Assert.Equal("de:ad:be:ef", Sample.ToString(HexFormat.LowercaseColons));
        }

        [Fact]
        public void ToString_UppercaseColons()
        {
            Assert.Equal("DE:AD:BE:EF", Sample.ToString(HexFormat.UppercaseColons));
        }

        [Fact]
        public void ToString_LowercaseDashes()
        {
            Assert.Equal("de-ad-be-ef", Sample.ToString(HexFormat.LowercaseDashes));
        }

        [Fact]
        public void ToString_UppercaseDashes()
        {
            Assert.Equal("DE-AD-BE-EF", Sample.ToString(HexFormat.UppercaseDashes));
        }

        [Fact]
        public void ToString_LowercaseSpaces()
        {
            Assert.Equal("de ad be ef", Sample.ToString(HexFormat.LowercaseSpaces));
        }

        [Fact]
        public void ToString_UppercaseSpaces()
        {
            Assert.Equal("DE AD BE EF", Sample.ToString(HexFormat.UppercaseSpaces));
        }

        // -------------------------------------------------------------------
        // Format specifier strings
        // -------------------------------------------------------------------

        [Theory]
        [InlineData("l", "deadbeef")]
        [InlineData("U", "DEADBEEF")]
        [InlineData("0x", "0xdeadbeef")]
        [InlineData("0X", "0XDEADBEEF")]
        [InlineData("l:", "de:ad:be:ef")]
        [InlineData("U:", "DE:AD:BE:EF")]
        [InlineData("l-", "de-ad-be-ef")]
        [InlineData("U-", "DE-AD-BE-EF")]
        public void ToString_FormatSpecifier_FormatsCorrectly(string specifier, string expected)
        {
            Assert.Equal(expected, Sample.ToString(specifier, null));
        }

        [Fact]
        public void ToString_SpaceFormatSpecifier()
        {
            Assert.Equal("de ad be ef", Sample.ToString("l ", null));
            Assert.Equal("DE AD BE EF", Sample.ToString("U ", null));
        }

        [Fact]
        public void ToString_UnknownSpecifier_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => Sample.ToString("Q", null));
        }

        // -------------------------------------------------------------------
        // Empty value formatting
        // -------------------------------------------------------------------

        [Fact]
        public void ToString_Empty_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, Hex.Empty.ToString());
        }

        [Fact]
        public void ToString_Default_Empty_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, default(Hex).ToString());
        }

        [Fact]
        public void ToString_EmptyWithPrefix_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, Hex.Empty.ToString(HexFormat.LowercasePrefixed));
        }

        // -------------------------------------------------------------------
        // Custom format
        // -------------------------------------------------------------------

        [Fact]
        public void ToString_CustomFormat()
        {
            var format = new HexFormat(HexLetterCase.Lower, separator: ".", prefix: "0x");
            Assert.Equal("0xde.ad.be.ef", Sample.ToString(format));
        }

        // -------------------------------------------------------------------
        // Single byte
        // -------------------------------------------------------------------

        [Fact]
        public void ToString_SingleByte_NoSeparator()
        {
            var hex = Hex.Parse("FF");
            Assert.Equal("FF", hex.ToString(HexFormat.UppercaseColons));
        }

        // -------------------------------------------------------------------
        // Null format
        // -------------------------------------------------------------------

        [Fact]
        public void ToString_NullHexFormat_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => Sample.ToString((HexFormat)null!));
        }

        // -------------------------------------------------------------------
        // Explicit cast to string
        // -------------------------------------------------------------------

        [Fact]
        public void ExplicitCast_ToString_UsesDefault()
        {
            HexDefaults.Reset();
            string result = (string)Sample;
            Assert.Equal("deadbeef", result);
        }

#if NET9_0_OR_GREATER
        // -------------------------------------------------------------------
        // ISpanFormattable
        // -------------------------------------------------------------------

        [Fact]
        public void TryFormat_CharSpan_SucceedsWithSufficientSpace()
        {
            Span<char> buffer = stackalloc char[16];
            Assert.True(Sample.TryFormat(buffer, out int written, default, null));
            Assert.Equal("deadbeef", buffer.Slice(0, written).ToString());
        }

        [Fact]
        public void TryFormat_CharSpan_FailsWithInsufficientSpace()
        {
            Span<char> buffer = stackalloc char[4];
            Assert.False(Sample.TryFormat(buffer, out int written, default, null));
            Assert.Equal(0, written);
        }
#endif
    }
}
