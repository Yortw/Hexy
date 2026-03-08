using System;
using System.ComponentModel;
using System.Text.Json;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class SerializationTests
    {
        // -------------------------------------------------------------------
        // System.Text.Json
        // -------------------------------------------------------------------

        [Fact]
        public void Json_Serialize_WritesHexString()
        {
            var hex = Hex.Parse("DEADBEEF");
            string json = JsonSerializer.Serialize(hex);
            Assert.Equal("\"deadbeef\"", json);
        }

        [Fact]
        public void Json_Deserialize_ParsesHexString()
        {
            var hex = JsonSerializer.Deserialize<Hex>("\"DEADBEEF\"");
            Assert.Equal(Hex.Parse("DEADBEEF"), hex);
        }

        [Fact]
        public void Json_Deserialize_PermissiveParsing()
        {
            var hex = JsonSerializer.Deserialize<Hex>("\"0xDE:AD\"");
            Assert.Equal(Hex.Parse("DEAD"), hex);
        }

        [Fact]
        public void Json_Deserialize_Null_ReturnsEmpty()
        {
            var hex = JsonSerializer.Deserialize<Hex>("null");
            Assert.True(hex.IsEmpty);
        }

        [Fact]
        public void Json_Deserialize_InvalidHex_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Hex>("\"not hex\""));
        }

        [Fact]
        public void Json_RoundTrip()
        {
            var original = Hex.CreateRandom(16);
            string json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<Hex>(json);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void Json_InObject_RoundTrip()
        {
            var obj = new TestObject { Id = Hex.Parse("DEADBEEF"), Name = "test" };
            string json = JsonSerializer.Serialize(obj);
            var deserialized = JsonSerializer.Deserialize<TestObject>(json);
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Id, deserialized!.Id);
            Assert.Equal(obj.Name, deserialized.Name);
        }

        private sealed class TestObject
        {
            public Hex Id { get; set; }
            public string Name { get; set; } = "";
        }

        // -------------------------------------------------------------------
        // TypeConverter
        // -------------------------------------------------------------------

        [Fact]
        public void TypeConverter_CanConvertFromString()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Hex));
            Assert.True(converter.CanConvertFrom(typeof(string)));
        }

        [Fact]
        public void TypeConverter_ConvertFromString()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Hex));
            var result = converter.ConvertFrom("DEADBEEF");
            Assert.IsType<Hex>(result);
            Assert.Equal(Hex.Parse("DEADBEEF"), (Hex)result!);
        }

        [Fact]
        public void TypeConverter_ConvertFromByteArray()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Hex));
            var result = converter.ConvertFrom(new byte[] { 0xCA, 0xFE });
            Assert.IsType<Hex>(result);
            Assert.Equal(Hex.Parse("CAFE"), (Hex)result!);
        }

        [Fact]
        public void TypeConverter_ConvertToString()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Hex));
            var result = converter.ConvertTo(Hex.Parse("DEADBEEF"), typeof(string));
            Assert.IsType<string>(result);
        }

        [Fact]
        public void TypeConverter_ConvertToByteArray()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Hex));
            var result = converter.ConvertTo(Hex.Parse("CAFE"), typeof(byte[]));
            Assert.IsType<byte[]>(result);
            Assert.Equal(new byte[] { 0xCA, 0xFE }, (byte[])result!);
        }

        // -------------------------------------------------------------------
        // IConvertible
        // -------------------------------------------------------------------

        [Fact]
        public void IConvertible_ChangeType_ToString()
        {
            var hex = Hex.Parse("DEADBEEF");
            var result = Convert.ChangeType(hex, typeof(string), System.Globalization.CultureInfo.InvariantCulture);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void IConvertible_ChangeType_ToByteArray()
        {
            var hex = Hex.Parse("CAFE");
            var result = Convert.ChangeType(hex, typeof(byte[]), System.Globalization.CultureInfo.InvariantCulture);
            Assert.IsType<byte[]>(result);
            Assert.Equal(new byte[] { 0xCA, 0xFE }, (byte[])result!);
        }

        [Fact]
        public void IConvertible_ChangeType_ToInt32_Throws()
        {
            var hex = Hex.Parse("DEADBEEF");
            Assert.Throws<InvalidCastException>(() => Convert.ChangeType(hex, typeof(int), System.Globalization.CultureInfo.InvariantCulture));
        }

        [Fact]
        public void IConvertible_ToBoolean_Throws()
        {
            IConvertible conv = Hex.Parse("FF");
            Assert.Throws<InvalidCastException>(() => conv.ToBoolean(null));
        }

        [Fact]
        public void IConvertible_ChangeType_ToHex_ReturnsSelf()
        {
            var hex = Hex.Parse("DEADBEEF");
            var result = Convert.ChangeType(hex, typeof(Hex), System.Globalization.CultureInfo.InvariantCulture);
            Assert.IsType<Hex>(result);
            Assert.Equal(hex, (Hex)result!);
        }

        // -------------------------------------------------------------------
        // JSON: empty, large, canonical format, non-string tokens
        // -------------------------------------------------------------------

        [Fact]
        public void Json_Serialize_Empty_WritesEmptyString()
        {
            string json = JsonSerializer.Serialize(Hex.Empty);
            Assert.Equal("\"\"", json);
        }

        [Fact]
        public void Json_Deserialize_EmptyString_ReturnsEmpty()
        {
            var hex = JsonSerializer.Deserialize<Hex>("\"\"");
            Assert.True(hex.IsEmpty);
        }

        [Fact]
        public void Json_RoundTrip_LargeValue()
        {
            var original = Hex.CreateRandom(4096); // 4 KB
            string json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<Hex>(json);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void Json_Serialize_AlwaysCanonicalLowercase()
        {
            // Even if HexDefaults.Format is something else, JSON should use canonical lowercase
            var hex = Hex.Parse("DEADBEEF");
            string json = JsonSerializer.Serialize(hex);
            Assert.Equal("\"deadbeef\"", json);
            Assert.DoesNotContain("DEAD", json);
        }

        [Fact]
        public void Json_RoundTrip_WithCustomDefaults()
        {
            // Temporarily set a custom format, verify JSON still round-trips
            HexDefaults.Format = HexFormat.Uppercase;
            try
            {
                var original = Hex.Parse("CAFEBABE");
                string json = JsonSerializer.Serialize(original);
                var deserialized = JsonSerializer.Deserialize<Hex>(json);
                Assert.Equal(original, deserialized);

                // JSON should be canonical lowercase regardless
                Assert.Equal("\"cafebabe\"", json);
            }
            finally
            {
                HexDefaults.Reset();
            }
        }

        [Fact]
        public void Json_Deserialize_NumberToken_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Hex>("42"));
        }

        [Fact]
        public void Json_Deserialize_BoolToken_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Hex>("true"));
        }

        [Fact]
        public void Json_Deserialize_ArrayToken_ThrowsJsonException()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Hex>("[1,2,3]"));
        }

        [Fact]
        public void Json_RoundTrip_SingleByte()
        {
            var original = Hex.Parse("FF");
            string json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<Hex>(json);
            Assert.Equal(original, deserialized);
        }
    }
}
