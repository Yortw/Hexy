using System;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class HexTests
    {
        // -------------------------------------------------------------------
        // Default / Empty
        // -------------------------------------------------------------------

        [Fact]
        public void Default_IsEmpty()
        {
            Hex hex = default;
            Assert.True(hex.IsEmpty);
            Assert.Equal(0, hex.Length);
        }

        [Fact]
        public void Empty_IsEmpty()
        {
            Assert.True(Hex.Empty.IsEmpty);
            Assert.Equal(0, Hex.Empty.Length);
        }

        [Fact]
        public void Default_EqualsEmpty()
        {
            Assert.Equal(Hex.Empty, default(Hex));
        }

        [Fact]
        public void Default_ToString_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, default(Hex).ToString());
        }

        [Fact]
        public void Default_ToByteArray_ReturnsEmptyArray()
        {
            Assert.Empty(default(Hex).ToByteArray());
        }

        // -------------------------------------------------------------------
        // Indexer
        // -------------------------------------------------------------------

        [Fact]
        public void Indexer_ReturnsCorrectByte()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.Equal(0xDE, hex[0]);
            Assert.Equal(0xAD, hex[1]);
            Assert.Equal(0xBE, hex[2]);
            Assert.Equal(0xEF, hex[3]);
        }

        [Fact]
        public void Indexer_OutOfRange_ThrowsIndexOutOfRangeException()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<IndexOutOfRangeException>(() => hex[2]);
            Assert.Throws<IndexOutOfRangeException>(() => hex[-1]);
        }

        // -------------------------------------------------------------------
        // Length / IsEmpty
        // -------------------------------------------------------------------

        [Theory]
        [InlineData("", 0, true)]
        [InlineData("FF", 1, false)]
        [InlineData("DEADBEEF", 4, false)]
        public void Length_And_IsEmpty(string input, int expectedLength, bool expectedIsEmpty)
        {
            var hex = Hex.Parse(input);
            Assert.Equal(expectedLength, hex.Length);
            Assert.Equal(expectedIsEmpty, hex.IsEmpty);
        }

        // -------------------------------------------------------------------
        // Operators: +, Append, Concat, Slice, Reverse, StartsWith, EndsWith, Contains
        // -------------------------------------------------------------------

        [Fact]
        public void PlusOperator_CombinesBytes()
        {
            var a = Hex.Parse("DEAD");
            var b = Hex.Parse("BEEF");
            var result = a + b;
            Assert.Equal(Hex.Parse("DEADBEEF"), result);
        }

        [Fact]
        public void Append_CombinesBytes()
        {
            var a = Hex.Parse("DEAD");
            var result = a.Append(Hex.Parse("BEEF"));
            Assert.Equal(Hex.Parse("DEADBEEF"), result);
        }

        [Fact]
        public void Append_DoesNotMutateOriginal()
        {
            var original = Hex.Parse("DEAD");
            var _ = original.Append(Hex.Parse("BEEF"));
            Assert.Equal(2, original.Length); // Unchanged
        }

        [Fact]
        public void Append_Empty_ReturnsSelf()
        {
            var hex = Hex.Parse("DEAD");
            var result = hex.Append(Hex.Empty);
            Assert.Equal(hex, result);
        }

        [Fact]
        public void Concat_MultipleValues()
        {
            var result = Hex.Concat(
                Hex.Parse("DE"),
                Hex.Parse("AD"),
                Hex.Parse("BE"),
                Hex.Parse("EF")
            );
            Assert.Equal(Hex.Parse("DEADBEEF"), result);
        }

        [Fact]
        public void Concat_EmptyArray_ReturnsEmpty()
        {
            Assert.True(Hex.Concat().IsEmpty);
        }

        [Fact]
        public void Concat_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Hex.Concat(null!));
        }

        [Fact]
        public void Slice_ReturnsSubRange()
        {
            var hex = Hex.Parse("DEADBEEF");
            var slice = hex.Slice(1, 2);
            Assert.Equal(Hex.Parse("ADBE"), slice);
        }

        [Fact]
        public void Slice_FullRange_ReturnsSelf()
        {
            var hex = Hex.Parse("DEADBEEF");
            var slice = hex.Slice(0, 4);
            Assert.Equal(hex, slice);
        }

        [Fact]
        public void Slice_ZeroLength_ReturnsEmpty()
        {
            var hex = Hex.Parse("DEADBEEF");
            var slice = hex.Slice(0, 0);
            Assert.True(slice.IsEmpty);
        }

        [Fact]
        public void Slice_OutOfRange_Throws()
        {
            var hex = Hex.Parse("DEAD");
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.Slice(0, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.Slice(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.Slice(0, -1));
        }

        [Fact]
        public void StartsWith_MatchingPrefix_ReturnsTrue()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.True(hex.StartsWith(Hex.Parse("DEAD")));
        }

        [Fact]
        public void StartsWith_NonMatchingPrefix_ReturnsFalse()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.False(hex.StartsWith(Hex.Parse("BEEF")));
        }

        [Fact]
        public void StartsWith_Empty_ReturnsTrue()
        {
            Assert.True(Hex.Parse("DEAD").StartsWith(Hex.Empty));
        }

        [Fact]
        public void StartsWith_LongerPrefix_ReturnsFalse()
        {
            Assert.False(Hex.Parse("DE").StartsWith(Hex.Parse("DEAD")));
        }

        [Fact]
        public void EndsWith_MatchingSuffix_ReturnsTrue()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.True(hex.EndsWith(Hex.Parse("BEEF")));
        }

        [Fact]
        public void EndsWith_NonMatchingSuffix_ReturnsFalse()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.False(hex.EndsWith(Hex.Parse("DEAD")));
        }

        [Fact]
        public void Contains_Present_ReturnsTrue()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.True(hex.Contains(Hex.Parse("ADBE")));
        }

        [Fact]
        public void Contains_NotPresent_ReturnsFalse()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.False(hex.Contains(Hex.Parse("CAFE")));
        }

        [Fact]
        public void Contains_Empty_ReturnsTrue()
        {
            Assert.True(Hex.Parse("DEAD").Contains(Hex.Empty));
        }

        [Fact]
        public void Contains_LongerValue_ReturnsFalse()
        {
            Assert.False(Hex.Parse("DE").Contains(Hex.Parse("DEADBEEF")));
        }

        [Fact]
        public void Reverse_ReversesBytes()
        {
            var hex = Hex.Parse("DEADBEEF");
            var reversed = hex.Reverse();
            Assert.Equal(Hex.Parse("EFBEADDE"), reversed);
        }

        [Fact]
        public void Reverse_DoesNotMutateOriginal()
        {
            var original = Hex.Parse("DEADBEEF");
            var _ = original.Reverse();
            Assert.Equal(Hex.Parse("DEADBEEF"), original);
        }

        [Fact]
        public void Reverse_Empty_ReturnsEmpty()
        {
            var reversed = Hex.Empty.Reverse();
            Assert.True(reversed.IsEmpty);
        }

        [Fact]
        public void Reverse_SingleByte_ReturnsSame()
        {
            var hex = Hex.Parse("FF");
            var reversed = hex.Reverse();
            Assert.Equal(hex, reversed);
        }

        // -------------------------------------------------------------------
        // Concat: scale and edge cases
        // -------------------------------------------------------------------

        [Fact]
        public void Concat_ManyValues_ProducesCorrectResult()
        {
            int count = 500;
            var values = new Hex[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = new Hex(new byte[] { (byte)(i & 0xFF) });
            }

            var result = Hex.Concat(values);
            Assert.Equal(count, result.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.Equal((byte)(i & 0xFF), result[i]);
            }
        }

        [Fact]
        public void Concat_ThousandValues_ProducesCorrectLength()
        {
            int count = 1000;
            var values = new Hex[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = Hex.Parse("DEAD"); // 2 bytes each
            }

            var result = Hex.Concat(values);
            Assert.Equal(count * 2, result.Length);
        }

        [Fact]
        public void Concat_MixedEmptyAndNonEmpty()
        {
            var result = Hex.Concat(
                Hex.Empty,
                Hex.Parse("DE"),
                Hex.Empty,
                Hex.Parse("AD"),
                Hex.Empty
            );
            Assert.Equal(Hex.Parse("DEAD"), result);
        }

        [Fact]
        public void Concat_AllEmpty_ReturnsEmpty()
        {
            var result = Hex.Concat(Hex.Empty, Hex.Empty, Hex.Empty);
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Concat_SingleValue_ReturnsEquivalent()
        {
            var original = Hex.Parse("DEADBEEF");
            var result = Hex.Concat(original);
            Assert.Equal(original, result);
        }

#if NET9_0_OR_GREATER
        [Fact]
        public void Concat_ReadOnlySpan_ManyValues()
        {
            int count = 500;
            var values = new Hex[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = new Hex(new byte[] { (byte)(i & 0xFF) });
            }

            var result = Hex.Concat(values.AsSpan());
            Assert.Equal(count, result.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.Equal((byte)(i & 0xFF), result[i]);
            }
        }
#endif
    }
}
