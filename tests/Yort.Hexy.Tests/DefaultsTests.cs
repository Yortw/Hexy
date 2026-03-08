using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class DefaultsTests : IDisposable
    {
        public DefaultsTests()
        {
            HexDefaults.Reset(); // Ensure clean state for each test
        }

        public void Dispose()
        {
            HexDefaults.Reset(); // Clean up after each test
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Format_DefaultIsLowercase()
        {
            Assert.Same(HexFormat.Lowercase, HexDefaults.Format);
        }

        [Fact]
        public void Format_CanBeChanged()
        {
            HexDefaults.Format = HexFormat.Uppercase;
            Assert.Same(HexFormat.Uppercase, HexDefaults.Format);
        }

        [Fact]
        public void Format_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HexDefaults.Format = null!);
        }

        [Fact]
        public void Format_AffectsToString()
        {
            HexDefaults.Format = HexFormat.UppercaseColons;
            var hex = Hex.Parse("DEADBEEF");
            Assert.Equal("DE:AD:BE:EF", hex.ToString());
        }

        // -------------------------------------------------------------------
        // Locking
        // -------------------------------------------------------------------

        [Fact]
        public void IsLocked_DefaultFalse()
        {
            Assert.False(HexDefaults.IsLocked);
        }

        [Fact]
        public void Lock_SetsIsLocked()
        {
            HexDefaults.Lock();
            Assert.True(HexDefaults.IsLocked);
        }

        [Fact]
        public void Lock_IsIdempotent()
        {
            HexDefaults.Lock();
            HexDefaults.Lock(); // Should not throw
            Assert.True(HexDefaults.IsLocked);
        }

        [Fact]
        public void Lock_PreventsFormatChange()
        {
            HexDefaults.Lock();
            Assert.Throws<InvalidOperationException>(() => HexDefaults.Format = HexFormat.Uppercase);
        }

        [Fact]
        public void Lock_ExistingFormatStillReadable()
        {
            HexDefaults.Format = HexFormat.UppercaseDashes;
            HexDefaults.Lock();
            Assert.Same(HexFormat.UppercaseDashes, HexDefaults.Format);
        }
    }
}
