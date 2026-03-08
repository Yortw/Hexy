using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class HexBuilderTests
    {
        [Fact]
        public void Build_FromMultipleHexValues()
        {
            var result = new HexBuilder()
                .Append(Hex.Parse("DE"))
                .Append(Hex.Parse("AD"))
                .Append(Hex.Parse("BEEF"))
                .ToHex();

            Assert.Equal(Hex.Parse("DEADBEEF"), result);
        }

        [Fact]
        public void Build_FromByteArrays()
        {
            var result = new HexBuilder()
                .Append(new byte[] { 0xCA })
                .Append(new byte[] { 0xFE })
                .ToHex();

            Assert.Equal(Hex.Parse("CAFE"), result);
        }

        [Fact]
        public void Build_FromSingleBytes()
        {
            var result = new HexBuilder()
                .Append((byte)0xDE)
                .Append((byte)0xAD)
                .ToHex();

            Assert.Equal(Hex.Parse("DEAD"), result);
        }

        [Fact]
        public void Build_Empty_ReturnsEmpty()
        {
            var result = new HexBuilder().ToHex();
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Length_TracksAccumulation()
        {
            var builder = new HexBuilder();
            Assert.Equal(0, builder.Length);

            builder.Append(Hex.Parse("DEAD"));
            Assert.Equal(2, builder.Length);

            builder.Append(Hex.Parse("BEEF"));
            Assert.Equal(4, builder.Length);
        }

        [Fact]
        public void Clear_ResetsLength()
        {
            var builder = new HexBuilder();
            builder.Append(Hex.Parse("DEADBEEF"));
            Assert.Equal(4, builder.Length);

            builder.Clear();
            Assert.Equal(0, builder.Length);

            var result = builder.ToHex();
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Reuse_AfterToHex()
        {
            var builder = new HexBuilder();
            builder.Append(Hex.Parse("DEAD"));
            var first = builder.ToHex();

            builder.Append(Hex.Parse("BEEF"));
            var second = builder.ToHex();

            Assert.Equal(Hex.Parse("DEAD"), first);
            Assert.Equal(Hex.Parse("DEADBEEF"), second);
        }

        [Fact]
        public void InitialCapacity_Zero_StillWorks()
        {
            var builder = new HexBuilder(0);
            builder.Append(Hex.Parse("CAFE"));
            Assert.Equal(Hex.Parse("CAFE"), builder.ToHex());
        }

        [Fact]
        public void InitialCapacity_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HexBuilder(-1));
        }

        [Fact]
        public void Append_NullByteArray_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HexBuilder().Append((byte[])null!));
        }

        [Fact]
        public void Append_EmptyHex_NoOp()
        {
            var builder = new HexBuilder();
            builder.Append(Hex.Empty);
            Assert.Equal(0, builder.Length);
        }

        [Fact]
        public void GeometricGrowth_HandlesLargeAppends()
        {
            var builder = new HexBuilder(4);
            for (int i = 0; i < 100; i++)
            {
                builder.Append(Hex.CreateRandom(8));
            }

            var result = builder.ToHex();
            Assert.Equal(800, result.Length);
        }

#if NET9_0_OR_GREATER
        [Fact]
        public void Append_ReadOnlySpan()
        {
            ReadOnlySpan<byte> data = stackalloc byte[] { 0xCA, 0xFE };
            var result = new HexBuilder()
                .Append(data)
                .ToHex();

            Assert.Equal(Hex.Parse("CAFE"), result);
        }
#endif

        // -------------------------------------------------------------------
        // Clear(bool zeroBuffer)
        // -------------------------------------------------------------------

        [Fact]
        public void Clear_ZeroBuffer_ZeroesUsedPortion()
        {
            var builder = new HexBuilder(8);
            builder.Append(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

            // After clear with zeroing, ToHex should be empty
            builder.Clear(zeroBuffer: true);
            Assert.Equal(0, builder.Length);
            Assert.True(builder.ToHex().IsEmpty);
        }

        [Fact]
        public void Clear_WithoutZero_ResetsLength()
        {
            var builder = new HexBuilder();
            builder.Append(Hex.Parse("DEADBEEF"));
            builder.Clear(zeroBuffer: false);
            Assert.Equal(0, builder.Length);
            Assert.True(builder.ToHex().IsEmpty);
        }

        // -------------------------------------------------------------------
        // Capacity growth and scale
        // -------------------------------------------------------------------

        [Fact]
        public void Capacity_GrowsGeometrically()
        {
            var builder = new HexBuilder(4);
            Assert.Equal(4, builder.Capacity);

            // Appending 5 bytes should trigger growth beyond 4
            builder.Append(new byte[] { 1, 2, 3, 4, 5 });
            Assert.True(builder.Capacity >= 5);
            Assert.True(builder.Capacity >= 8); // geometric doubling from 4
        }

        [Fact]
        public void LargeAccumulation_PreservesAllData()
        {
            var builder = new HexBuilder(1); // Start tiny
            int totalBytes = 10_000;
            for (int i = 0; i < totalBytes; i++)
            {
                builder.Append((byte)(i & 0xFF));
            }

            var result = builder.ToHex();
            Assert.Equal(totalBytes, result.Length);

            // Verify a sampling of values
            Assert.Equal(0, result[0]);
            Assert.Equal(1, result[1]);
            Assert.Equal(0xFF, result[255]);
            Assert.Equal(0, result[256]); // wraps around
        }

        [Fact]
        public void RepeatedClearAndAppend_WorksCorrectly()
        {
            var builder = new HexBuilder();
            for (int cycle = 0; cycle < 50; cycle++)
            {
                builder.Clear();
                builder.Append(Hex.Parse("CAFE"));
                builder.Append((byte)0xBA);
                builder.Append(new byte[] { 0xBE });

                var result = builder.ToHex();
                Assert.Equal(Hex.Parse("CAFEBABE"), result);
            }
        }

        [Fact]
        public void MixedAppendTypes_ProducesCorrectResult()
        {
            var result = new HexBuilder()
                .Append(Hex.Parse("DE"))
                .Append((byte)0xAD)
                .Append(new byte[] { 0xBE, 0xEF })
                .Append(Hex.Parse("CAFE"))
                .ToHex();

            Assert.Equal(Hex.Parse("DEADBEEFCAFE"), result);
        }

        [Fact]
        public void ZeroCapacity_GrowsOnFirstAppend()
        {
            var builder = new HexBuilder(0);
            Assert.Equal(0, builder.Capacity);

            builder.Append((byte)0xFF);
            Assert.True(builder.Capacity > 0);
            Assert.Equal(1, builder.Length);
            Assert.Equal(Hex.Parse("FF"), builder.ToHex());
        }
    }
}
