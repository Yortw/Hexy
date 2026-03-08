using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Yort.Hexy
{
    /// <summary>
    /// Provides extension methods for reading and writing <see cref="Hex"/> values from/to
    /// streams, <see cref="BinaryReader"/>, and <see cref="BinaryWriter"/>.
    /// </summary>
    public static class HexStreamExtensions
    {
        // -------------------------------------------------------------------
        // Synchronous Stream
        // -------------------------------------------------------------------

        /// <summary>
        /// Reads the specified number of bytes from the stream and returns them as a <see cref="Hex"/> value.
        /// </summary>
        /// <param name="stream">The stream to read from. Must not be <see langword="null"/>.</param>
        /// <param name="byteCount">The number of bytes to read. Must be non-negative.</param>
        /// <returns>A new <see cref="Hex"/> containing the bytes read from the stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is negative.</exception>
        /// <exception cref="EndOfStreamException">
        /// The end of the stream was reached before <paramref name="byteCount"/> bytes could be read.
        /// </exception>
        /// <remarks>
        /// <para>This method reads exactly <paramref name="byteCount"/> bytes, calling
        /// <see cref="Stream.Read(byte[], int, int)"/> in a loop if necessary.</para>
        /// </remarks>
        /// <seealso cref="ReadHexAsync(Stream, int, CancellationToken)"/>
        /// <seealso cref="WriteHex(Stream, Hex)"/>
        public static Hex ReadHex(this Stream stream, int byteCount)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(stream);
#else
            if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
            if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be non-negative.");
            if (byteCount == 0) return Hex.Empty;

            byte[] buffer = new byte[byteCount];
            int offset = 0;
            while (offset < byteCount)
            {
                int read = stream.Read(buffer, offset, byteCount - offset);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"End of stream reached after reading {offset} of {byteCount} bytes.");
                offset += read;
            }

            return new Hex(buffer, null);
        }

        /// <summary>
        /// Writes the bytes of a <see cref="Hex"/> value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to. Must not be <see langword="null"/>.</param>
        /// <param name="hex">The hex value to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <seealso cref="WriteHexAsync(Stream, Hex, CancellationToken)"/>
        /// <seealso cref="ReadHex(Stream, int)"/>
        public static void WriteHex(this Stream stream, Hex hex)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(stream);
#else
            if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
            byte[] bytes = hex.BytesOrEmpty;
            if (bytes.Length > 0)
                stream.Write(bytes, 0, bytes.Length);
        }

        // -------------------------------------------------------------------
        // Async Stream
        // -------------------------------------------------------------------

        /// <summary>
        /// Asynchronously reads the specified number of bytes from the stream and returns them as a <see cref="Hex"/> value.
        /// </summary>
        /// <param name="stream">The stream to read from. Must not be <see langword="null"/>.</param>
        /// <param name="byteCount">The number of bytes to read. Must be non-negative.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that resolves to a new <see cref="Hex"/> containing the bytes read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is negative.</exception>
        /// <exception cref="EndOfStreamException">
        /// The end of the stream was reached before <paramref name="byteCount"/> bytes could be read.
        /// </exception>
        /// <seealso cref="ReadHex(Stream, int)"/>
        public static async Task<Hex> ReadHexAsync(this Stream stream, int byteCount, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(stream);
#else
            if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
            if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be non-negative.");
            if (byteCount == 0) return Hex.Empty;

            byte[] buffer = new byte[byteCount];
            int offset = 0;
            while (offset < byteCount)
            {
#if NET6_0_OR_GREATER
                int read = await stream.ReadAsync(buffer.AsMemory(offset, byteCount - offset), cancellationToken).ConfigureAwait(false);
#else
                int read = await stream.ReadAsync(buffer, offset, byteCount - offset, cancellationToken).ConfigureAwait(false);
#endif
                if (read == 0)
                    throw new EndOfStreamException(
                        $"End of stream reached after reading {offset} of {byteCount} bytes.");
                offset += read;
            }

            return new Hex(buffer, null);
        }

        /// <summary>
        /// Asynchronously writes the bytes of a <see cref="Hex"/> value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write to. Must not be <see langword="null"/>.</param>
        /// <param name="hex">The hex value to write.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the write operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <seealso cref="WriteHex(Stream, Hex)"/>
        public static async Task WriteHexAsync(this Stream stream, Hex hex, CancellationToken cancellationToken = default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(stream);
#else
            if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
            byte[] bytes = hex.BytesOrEmpty;
            if (bytes.Length > 0)
            {
#if NET6_0_OR_GREATER
                await stream.WriteAsync(bytes.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
#endif
            }
        }

        // -------------------------------------------------------------------
        // BinaryReader / BinaryWriter
        // -------------------------------------------------------------------

        /// <summary>
        /// Reads the specified number of bytes from the <see cref="BinaryReader"/> and returns them as a <see cref="Hex"/> value.
        /// </summary>
        /// <param name="reader">The binary reader to read from. Must not be <see langword="null"/>.</param>
        /// <param name="byteCount">The number of bytes to read. Must be non-negative.</param>
        /// <returns>A new <see cref="Hex"/> containing the bytes read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is negative.</exception>
        /// <exception cref="EndOfStreamException">
        /// The end of the stream was reached before <paramref name="byteCount"/> bytes could be read.
        /// </exception>
        /// <seealso cref="Write(BinaryWriter, Hex)"/>
        public static Hex ReadHex(this BinaryReader reader, int byteCount)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(reader);
#else
            if (reader == null) throw new ArgumentNullException(nameof(reader));
#endif
            if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be non-negative.");
            if (byteCount == 0) return Hex.Empty;

            byte[] bytes = reader.ReadBytes(byteCount);
            if (bytes.Length < byteCount)
                throw new EndOfStreamException(
                    $"End of stream reached after reading {bytes.Length} of {byteCount} bytes.");

            return new Hex(bytes, null);
        }

        /// <summary>
        /// Writes the bytes of a <see cref="Hex"/> value using the <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The binary writer to write to. Must not be <see langword="null"/>.</param>
        /// <param name="hex">The hex value to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <seealso cref="ReadHex(BinaryReader, int)"/>
        public static void Write(this BinaryWriter writer, Hex hex)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(writer);
#else
            if (writer == null) throw new ArgumentNullException(nameof(writer));
#endif
            byte[] bytes = hex.BytesOrEmpty;
            if (bytes.Length > 0)
                writer.Write(bytes);
        }
    }
}
