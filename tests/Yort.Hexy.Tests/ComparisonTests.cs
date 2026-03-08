using System;
using System.Collections.Generic;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class ComparisonTests
    {
        // -------------------------------------------------------------------
        // Equality
        // -------------------------------------------------------------------

        [Fact]
        public void Equals_SameBytes_ReturnsTrue()
        {
            var a = Hex.Parse("DEADBEEF");
            var b = Hex.Parse("deadbeef");
            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentBytes_ReturnsFalse()
        {
            var a = Hex.Parse("DEADBEEF");
            var b = Hex.Parse("CAFEBABE");
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentLengths_ReturnsFalse()
        {
            var a = Hex.Parse("DEAD");
            var b = Hex.Parse("DEADBEEF");
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_BothEmpty_ReturnsTrue()
        {
            Assert.True(Hex.Empty.Equals(default(Hex)));
            Assert.True(Hex.Empty == default(Hex));
        }

        [Fact]
        public void Equals_EmptyAndNonEmpty_ReturnsFalse()
        {
            Assert.False(Hex.Empty.Equals(Hex.Parse("FF")));
        }

        [Fact]
        public void Equals_Object_BoxedHex_ReturnsTrue()
        {
            var a = Hex.Parse("DEADBEEF");
            object b = Hex.Parse("DEADBEEF");
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_Object_NonHex_ReturnsFalse()
        {
            var a = Hex.Parse("DEADBEEF");
            Assert.False(a.Equals("not a hex"));
            Assert.False(a.Equals(42));
            Assert.False(a.Equals((object?)null));
        }

        // -------------------------------------------------------------------
        // GetHashCode
        // -------------------------------------------------------------------

        [Fact]
        public void GetHashCode_EqualValues_SameHash()
        {
            var a = Hex.Parse("DEADBEEF");
            var b = Hex.Parse("deadbeef");
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_Empty_Deterministic()
        {
            Assert.Equal(Hex.Empty.GetHashCode(), default(Hex).GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentValues_TypicallyDiffer()
        {
            // Not guaranteed but statistically overwhelming
            var hashes = new HashSet<int>();
            for (int i = 0; i < 100; i++)
            {
                hashes.Add(Hex.CreateRandom(8).GetHashCode());
            }
            Assert.True(hashes.Count > 90, "Hash codes should be well-distributed");
        }

        // -------------------------------------------------------------------
        // Dictionary key usage
        // -------------------------------------------------------------------

        [Fact]
        public void DictionaryKey_LookupWorks()
        {
            var dict = new Dictionary<Hex, string>
            {
                [Hex.Parse("DEADBEEF")] = "found"
            };

            Assert.True(dict.ContainsKey(Hex.Parse("deadbeef")));
            Assert.Equal("found", dict[Hex.Parse("DEADBEEF")]);
        }

        // -------------------------------------------------------------------
        // CompareTo
        // -------------------------------------------------------------------

        [Fact]
        public void CompareTo_EqualValues_ReturnsZero()
        {
            var a = Hex.Parse("DEADBEEF");
            var b = Hex.Parse("DEADBEEF");
            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void CompareTo_LessThan()
        {
            var a = Hex.Parse("00FF");
            var b = Hex.Parse("FF00");
            Assert.True(a.CompareTo(b) < 0);
            Assert.True(a < b);
            Assert.True(a <= b);
        }

        [Fact]
        public void CompareTo_GreaterThan()
        {
            var a = Hex.Parse("FF00");
            var b = Hex.Parse("00FF");
            Assert.True(a.CompareTo(b) > 0);
            Assert.True(a > b);
            Assert.True(a >= b);
        }

        [Fact]
        public void CompareTo_ShorterIsLess()
        {
            var shorter = Hex.Parse("DEAD");
            var longer = Hex.Parse("DEADBEEF");
            Assert.True(shorter.CompareTo(longer) < 0);
        }

        [Fact]
        public void CompareTo_Null_ReturnsPositive()
        {
            var hex = Hex.Parse("FF");
            Assert.True(hex.CompareTo((object?)null) > 0);
        }

        [Fact]
        public void CompareTo_WrongType_ThrowsArgumentException()
        {
            var hex = Hex.Parse("FF");
            Assert.Throws<ArgumentException>(() => hex.CompareTo("not a hex"));
        }

        // -------------------------------------------------------------------
        // Sorting
        // -------------------------------------------------------------------

        [Fact]
        public void Sort_OrdersCorrectly()
        {
            var list = new List<Hex>
            {
                Hex.Parse("FF"),
                Hex.Parse("00"),
                Hex.Parse("AA"),
                Hex.Parse("55")
            };

            list.Sort();

            Assert.Equal(Hex.Parse("00"), list[0]);
            Assert.Equal(Hex.Parse("55"), list[1]);
            Assert.Equal(Hex.Parse("AA"), list[2]);
            Assert.Equal(Hex.Parse("FF"), list[3]);
        }
    }
}
