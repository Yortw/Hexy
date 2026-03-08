using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Yort.Hexy.Tests
{
    public class StreamExtensionTests
    {
        // -------------------------------------------------------------------
        // Synchronous Stream
        // -------------------------------------------------------------------

        [Fact]
        public void Stream_WriteAndRead_RoundTrips()
        {
            var original = Hex.Parse("DEADBEEF");
            using var ms = new MemoryStream();

            ms.WriteHex(original);
            ms.Position = 0;
            var result = ms.ReadHex(4);

            Assert.Equal(original, result);
        }

        [Fact]
        public void Stream_ReadHex_Zero_ReturnsEmpty()
        {
            using var ms = new MemoryStream();
            var result = ms.ReadHex(0);
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Stream_ReadHex_EndOfStream_Throws()
        {
            using var ms = new MemoryStream(new byte[] { 0xDE });
            Assert.Throws<EndOfStreamException>(() => ms.ReadHex(4));
        }

        [Fact]
        public void Stream_ReadHex_NegativeByteCount_Throws()
        {
            using var ms = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => ms.ReadHex(-1));
        }

        [Fact]
        public void Stream_WriteHex_NullStream_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((Stream)null!).WriteHex(Hex.Empty));
        }

        [Fact]
        public void Stream_WriteHex_Empty_WritesNothing()
        {
            using var ms = new MemoryStream();
            ms.WriteHex(Hex.Empty);
            Assert.Equal(0, ms.Length);
        }

        // -------------------------------------------------------------------
        // Async Stream
        // -------------------------------------------------------------------

        [Fact]
        public async Task Stream_WriteAndReadAsync_RoundTrips()
        {
            var original = Hex.Parse("CAFEBABE");
            using var ms = new MemoryStream();

            await ms.WriteHexAsync(original);
            ms.Position = 0;
            var result = await ms.ReadHexAsync(4);

            Assert.Equal(original, result);
        }

        [Fact]
        public async Task Stream_ReadHexAsync_EndOfStream_Throws()
        {
            using var ms = new MemoryStream(new byte[] { 0xDE });
            await Assert.ThrowsAsync<EndOfStreamException>(() => ms.ReadHexAsync(4));
        }

        // -------------------------------------------------------------------
        // BinaryReader / BinaryWriter
        // -------------------------------------------------------------------

        [Fact]
        public void BinaryWriter_WriteAndRead_RoundTrips()
        {
            var original = Hex.Parse("DEADBEEF");
            using var ms = new MemoryStream();

            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
                writer.Write(original);

            ms.Position = 0;

            using (var reader = new BinaryReader(ms))
            {
                var result = reader.ReadHex(4);
                Assert.Equal(original, result);
            }
        }

        [Fact]
        public void BinaryReader_ReadHex_EndOfStream_Throws()
        {
            using var ms = new MemoryStream(new byte[] { 0xDE });
            using var reader = new BinaryReader(ms);
            Assert.Throws<EndOfStreamException>(() => reader.ReadHex(4));
        }

        [Fact]
        public void BinaryReader_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((BinaryReader)null!).ReadHex(4));
        }

        [Fact]
        public void BinaryWriter_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((BinaryWriter)null!).Write(Hex.Empty));
        }

        // -------------------------------------------------------------------
        // Sequential reads/writes
        // -------------------------------------------------------------------

        [Fact]
        public void Stream_MultipleWritesAndReads_Sequential()
        {
            var a = Hex.Parse("DEAD");
            var b = Hex.Parse("BEEF");
            var c = Hex.Parse("CAFE");

            using var ms = new MemoryStream();
            ms.WriteHex(a);
            ms.WriteHex(b);
            ms.WriteHex(c);

            ms.Position = 0;
            Assert.Equal(a, ms.ReadHex(2));
            Assert.Equal(b, ms.ReadHex(2));
            Assert.Equal(c, ms.ReadHex(2));
        }

        [Fact]
        public void Stream_Position_AdvancesCorrectly()
        {
            var hex = Hex.Parse("DEADBEEFCAFE");
            using var ms = new MemoryStream();

            ms.WriteHex(hex);
            Assert.Equal(6, ms.Position);

            ms.Position = 0;
            ms.ReadHex(3);
            Assert.Equal(3, ms.Position);

            ms.ReadHex(3);
            Assert.Equal(6, ms.Position);
        }

        [Fact]
        public void Stream_WriteEmpty_DoesNotAdvancePosition()
        {
            using var ms = new MemoryStream();
            ms.WriteHex(Hex.Parse("DEAD"));
            long posAfterWrite = ms.Position;

            ms.WriteHex(Hex.Empty);
            Assert.Equal(posAfterWrite, ms.Position);
        }

        // -------------------------------------------------------------------
        // Larger payloads
        // -------------------------------------------------------------------

        [Fact]
        public void Stream_LargePayload_RoundTrips()
        {
            var original = Hex.CreateRandom(8192); // 8 KB
            using var ms = new MemoryStream();

            ms.WriteHex(original);
            ms.Position = 0;
            var result = ms.ReadHex(8192);

            Assert.Equal(original, result);
        }

        [Fact]
        public async Task Stream_LargePayload_Async_RoundTrips()
        {
            var original = Hex.CreateRandom(8192);
            using var ms = new MemoryStream();

            await ms.WriteHexAsync(original);
            ms.Position = 0;
            var result = await ms.ReadHexAsync(8192);

            Assert.Equal(original, result);
        }

        // -------------------------------------------------------------------
        // Async: sequential and edge cases
        // -------------------------------------------------------------------

        [Fact]
        public async Task Stream_MultipleWritesAndReads_Async_Sequential()
        {
            var a = Hex.Parse("DEAD");
            var b = Hex.Parse("BEEF");

            using var ms = new MemoryStream();
            await ms.WriteHexAsync(a);
            await ms.WriteHexAsync(b);

            ms.Position = 0;
            Assert.Equal(a, await ms.ReadHexAsync(2));
            Assert.Equal(b, await ms.ReadHexAsync(2));
        }

        [Fact]
        public async Task Stream_WriteHexAsync_Empty_WritesNothing()
        {
            using var ms = new MemoryStream();
            await ms.WriteHexAsync(Hex.Empty);
            Assert.Equal(0, ms.Length);
        }

        [Fact]
        public async Task Stream_ReadHexAsync_Zero_ReturnsEmpty()
        {
            using var ms = new MemoryStream();
            var result = await ms.ReadHexAsync(0);
            Assert.True(result.IsEmpty);
        }

        // -------------------------------------------------------------------
        // BinaryReader/Writer: sequential
        // -------------------------------------------------------------------

        [Fact]
        public void BinaryWriter_MultipleWrites_ReadBackSequentially()
        {
            var a = Hex.Parse("DEAD");
            var b = Hex.Parse("CAFE");

            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(a);
                writer.Write(b);
            }

            ms.Position = 0;
            using (var reader = new BinaryReader(ms))
            {
                Assert.Equal(a, reader.ReadHex(2));
                Assert.Equal(b, reader.ReadHex(2));
            }
        }

        [Fact]
        public void BinaryWriter_Empty_WritesNothing()
        {
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Hex.Empty);
            }
            Assert.Equal(0, ms.Length);
        }
    }
}
