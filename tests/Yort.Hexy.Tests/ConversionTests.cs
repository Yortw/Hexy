using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class ConversionTests
    {
        // -------------------------------------------------------------------
        // Constructor from byte[]
        // -------------------------------------------------------------------

        [Fact]
        public void Constructor_ByteArray_CopiesInput()
        {
            byte[] input = { 0xDE, 0xAD };
            var hex = new Hex(input);
            input[0] = 0xFF; // Mutate original
            Assert.Equal(0xDE, hex[0]); // Hex unaffected
        }

        [Fact]
        public void Constructor_EmptyByteArray()
        {
            var hex = new Hex(Array.Empty<byte>());
            Assert.True(hex.IsEmpty);
        }

        [Fact]
        public void Constructor_NullByteArray_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Hex((byte[])null!));
        }

        // -------------------------------------------------------------------
        // Explicit from byte[]
        // -------------------------------------------------------------------

        [Fact]
        public void ExplicitConversion_FromByteArray()
        {
            Hex hex = (Hex)new byte[] { 0xCA, 0xFE };
            Assert.Equal(2, hex.Length);
            Assert.Equal(0xCA, hex[0]);
        }

        // -------------------------------------------------------------------
        // ToByteArray
        // -------------------------------------------------------------------

        [Fact]
        public void ToByteArray_ReturnsCopy()
        {
            var hex = Hex.Parse("DEADBEEF");
            byte[] a = hex.ToByteArray();
            byte[] b = hex.ToByteArray();

            Assert.Equal(a, b);
            Assert.NotSame(a, b); // Different instances
        }

        [Fact]
        public void ToByteArray_Empty_ReturnsEmptyArray()
        {
            byte[] result = Hex.Empty.ToByteArray();
            Assert.Empty(result);
        }

        [Fact]
        public void ToByteArray_MutationDoesNotAffectHex()
        {
            var hex = Hex.Parse("CAFE");
            byte[] bytes = hex.ToByteArray();
            bytes[0] = 0xFF;
            Assert.Equal(0xCA, hex[0]);
        }

        // -------------------------------------------------------------------
        // TryWriteBytes
        // -------------------------------------------------------------------

        [Fact]
        public void TryWriteBytes_SufficientSpace_Succeeds()
        {
            var hex = Hex.Parse("DEADBEEF");
            Span<byte> buffer = stackalloc byte[4];
            Assert.True(hex.TryWriteBytes(buffer));
            Assert.Equal(0xDE, buffer[0]);
            Assert.Equal(0xEF, buffer[3]);
        }

        [Fact]
        public void TryWriteBytes_InsufficientSpace_ReturnsFalse()
        {
            var hex = Hex.Parse("DEADBEEF");
            Span<byte> buffer = stackalloc byte[2];
            Assert.False(hex.TryWriteBytes(buffer));
        }

        [Fact]
        public void TryWriteBytes_Empty_SucceedsWithEmptyBuffer()
        {
            Assert.True(Hex.Empty.TryWriteBytes(Span<byte>.Empty));
        }

        // -------------------------------------------------------------------
        // AsSpan
        // -------------------------------------------------------------------

        [Fact]
        public void AsSpan_ReturnsCorrectBytes()
        {
            var hex = Hex.Parse("CAFE");
            var span = hex.AsSpan();
            Assert.Equal(2, span.Length);
            Assert.Equal(0xCA, span[0]);
            Assert.Equal(0xFE, span[1]);
        }

        [Fact]
        public void AsSpan_WithOffsetAndLength()
        {
            var hex = Hex.Parse("DEADBEEF");
            var span = hex.AsSpan(1, 2);
            Assert.Equal(2, span.Length);
            Assert.Equal(0xAD, span[0]);
            Assert.Equal(0xBE, span[1]);
        }

        [Fact]
        public void AsSpan_Empty_ReturnsEmptySpan()
        {
            Assert.True(Hex.Empty.AsSpan().IsEmpty);
            Assert.True(default(Hex).AsSpan().IsEmpty);
        }

        // -------------------------------------------------------------------
        // AsMemory
        // -------------------------------------------------------------------

        [Fact]
        public void AsMemory_ReturnsCorrectBytes()
        {
            var hex = Hex.Parse("CAFE");
            ReadOnlyMemory<byte> mem = hex.AsMemory();
            Assert.Equal(2, mem.Length);
            Assert.Equal(0xCA, mem.Span[0]);
        }

        [Fact]
        public void ImplicitConversion_ToReadOnlyMemory()
        {
            var hex = Hex.Parse("CAFE");
            ReadOnlyMemory<byte> mem = hex; // implicit conversion
            Assert.Equal(2, mem.Length);
        }

        // -------------------------------------------------------------------
        // Guid interop
        // -------------------------------------------------------------------

        [Fact]
        public void FromGuid_RoundTrips()
        {
            Guid original = Guid.NewGuid();
            Hex hex = Hex.FromGuid(original);
            Guid back = hex.ToGuid();
            Assert.Equal(original, back);
        }

        [Fact]
        public void FromGuid_Returns16Bytes()
        {
            Hex hex = Hex.FromGuid(Guid.NewGuid());
            Assert.Equal(16, hex.Length);
        }

        [Fact]
        public void ToGuid_WrongLength_ThrowsInvalidOperationException()
        {
            var hex = Hex.Parse("DEADBEEF"); // 4 bytes, not 16
            Assert.Throws<InvalidOperationException>(() => hex.ToGuid());
        }

        [Fact]
        public void ToGuid_Empty_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Hex.Empty.ToGuid());
        }

        // -------------------------------------------------------------------
        // CreateRandom
        // -------------------------------------------------------------------

        [Fact]
        public void CreateRandom_ReturnsCorrectLength()
        {
            var hex = Hex.CreateRandom(16);
            Assert.Equal(16, hex.Length);
        }

        [Fact]
        public void CreateRandom_Zero_ReturnsEmpty()
        {
            var hex = Hex.CreateRandom(0);
            Assert.True(hex.IsEmpty);
        }

        [Fact]
        public void CreateRandom_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Hex.CreateRandom(-1));
        }

        [Fact]
        public void CreateRandom_ProducesUniqueValues()
        {
            var a = Hex.CreateRandom(16);
            var b = Hex.CreateRandom(16);
            Assert.NotEqual(a, b); // Statistically guaranteed for 16 bytes
        }

        // -------------------------------------------------------------------
        // AsMemory(int start) — validation
        // -------------------------------------------------------------------

        [Fact]
        public void AsMemory_WithStart_ReturnsRemainder()
        {
            var hex = Hex.Parse("DEADBEEF");
            var mem = hex.AsMemory(1);
            Assert.Equal(3, mem.Length);
            Assert.Equal(0xAD, mem.Span[0]);
            Assert.Equal(0xEF, mem.Span[2]);
        }

        [Fact]
        public void AsMemory_WithStart_AtEnd_ReturnsEmpty()
        {
            var hex = Hex.Parse("DEAD");
            var mem = hex.AsMemory(2);
            Assert.Equal(0, mem.Length);
        }

        [Fact]
        public void AsMemory_WithStart_Zero_ReturnsFull()
        {
            var hex = Hex.Parse("CAFE");
            var mem = hex.AsMemory(0);
            Assert.Equal(2, mem.Length);
        }

        [Fact]
        public void AsMemory_WithStart_Negative_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(-1));
        }

        [Fact]
        public void AsMemory_WithStart_PastEnd_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(3));
        }

        [Fact]
        public void AsMemory_WithStart_Empty_OnlyZeroIsValid()
        {
            var mem = Hex.Empty.AsMemory(0);
            Assert.Equal(0, mem.Length);
            Assert.Throws<ArgumentOutOfRangeException>(() => Hex.Empty.AsMemory(1));
        }

        // -------------------------------------------------------------------
        // AsMemory(int start, int length) — validation
        // -------------------------------------------------------------------

        [Fact]
        public void AsMemory_WithStartAndLength_ReturnsSlice()
        {
            var hex = Hex.Parse("DEADBEEF");
            var mem = hex.AsMemory(1, 2);
            Assert.Equal(2, mem.Length);
            Assert.Equal(0xAD, mem.Span[0]);
            Assert.Equal(0xBE, mem.Span[1]);
        }

        [Fact]
        public void AsMemory_WithStartAndLength_ZeroLength_ReturnsEmpty()
        {
            var hex = Hex.Parse("DEAD");
            var mem = hex.AsMemory(1, 0);
            Assert.Equal(0, mem.Length);
        }

        [Fact]
        public void AsMemory_WithStartAndLength_FullRange()
        {
            var hex = Hex.Parse("CAFE");
            var mem = hex.AsMemory(0, 2);
            Assert.Equal(2, mem.Length);
            Assert.Equal(0xCA, mem.Span[0]);
        }

        [Fact]
        public void AsMemory_WithStartAndLength_NegativeStart_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(-1, 1));
        }

        [Fact]
        public void AsMemory_WithStartAndLength_NegativeLength_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(0, -1));
        }

        [Fact]
        public void AsMemory_WithStartAndLength_ExceedsBounds_Throws()
        {
            var hex = Hex.Parse("DEAD"); // 2 bytes
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(1, 2));
        }

        [Fact]
        public void AsMemory_WithStartAndLength_StartPastEnd_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.AsMemory(3, 0));
        }

        [Fact]
        public void AsMemory_WithStartAndLength_Empty_OnlyZeroZeroIsValid()
        {
            var mem = Hex.Empty.AsMemory(0, 0);
            Assert.Equal(0, mem.Length);
            Assert.Throws<ArgumentOutOfRangeException>(() => Hex.Empty.AsMemory(0, 1));
        }

#if NET9_0_OR_GREATER
        // -------------------------------------------------------------------
        // Span constructor (.NET 9+)
        // -------------------------------------------------------------------

        [Fact]
        public void Constructor_ReadOnlySpan()
        {
            ReadOnlySpan<byte> data = stackalloc byte[] { 0xCA, 0xFE };
            var hex = new Hex(data);
            Assert.Equal(2, hex.Length);
            Assert.Equal(0xCA, hex[0]);
        }
#endif
    }
}
