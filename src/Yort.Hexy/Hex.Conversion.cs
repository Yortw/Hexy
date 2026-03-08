using System;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    public readonly partial struct Hex
    {
        // -------------------------------------------------------------------
        // Explicit conversions (byte[])
        // -------------------------------------------------------------------

        /// <summary>
        /// Explicitly converts a byte array to a <see cref="Hex"/> value.
        /// The input array is copied to preserve immutability.
        /// </summary>
        /// <param name="bytes">The byte array to convert. Must not be <see langword="null"/>.</param>
        /// <returns>A new <see cref="Hex"/> containing a copy of the bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
        /// <seealso cref="ToByteArray"/>
        public static explicit operator Hex(byte[] bytes) => new Hex(bytes);

        /// <summary>
        /// Implicitly converts a <see cref="Hex"/> to a <see cref="ReadOnlyMemory{T}"/> of bytes.
        /// This is a zero-copy operation.
        /// </summary>
        /// <param name="hex">The hex value to convert.</param>
        /// <returns>A read-only memory view over the internal byte array.</returns>
        /// <remarks>
        /// <para>This operator is zero-copy — no allocation occurs. The returned memory
        /// references the same internal array. Since <see cref="Hex"/> is immutable,
        /// the contents cannot be modified through this view.</para>
        /// </remarks>
        /// <seealso cref="AsMemory()"/>
        public static implicit operator ReadOnlyMemory<byte>(Hex hex) => hex.AsMemory();

        // -------------------------------------------------------------------
        // Explicit conversions (string)
        // -------------------------------------------------------------------

        /// <summary>
        /// Explicitly converts a string to a <see cref="Hex"/> by parsing it.
        /// </summary>
        /// <param name="hex">The hexadecimal string to parse. Must not be <see langword="null"/>.</param>
        /// <returns>A new <see cref="Hex"/> containing the parsed bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hex"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException"><paramref name="hex"/> is not a valid hexadecimal string.</exception>
        /// <seealso cref="Parse(string)"/>
        public static explicit operator Hex(string hex) => Parse(hex);

        /// <summary>
        /// Explicitly converts a <see cref="Hex"/> to its string representation using the
        /// application default format (<see cref="HexDefaults.Format"/>).
        /// </summary>
        /// <param name="hex">The hex value to convert.</param>
        /// <returns>The formatted hexadecimal string.</returns>
        /// <seealso cref="ToString()"/>
        public static explicit operator string(Hex hex) => hex.ToString();

        // -------------------------------------------------------------------
        // Span / Memory access (zero-copy)
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns a read-only span over the entire byte sequence. This is a zero-copy operation.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> over the internal byte array.</returns>
        /// <remarks>
        /// <para>No allocation occurs. For <c>default(Hex)</c>, returns an empty span.</para>
        /// </remarks>
        /// <seealso cref="AsSpan(int)"/>
        /// <seealso cref="AsSpan(int, int)"/>
        /// <seealso cref="AsMemory()"/>
        public ReadOnlySpan<byte> AsSpan() => BytesOrEmpty;

        /// <summary>
        /// Returns a read-only span starting at the specified byte offset.
        /// </summary>
        /// <param name="start">The zero-based byte index at which to begin the span.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> starting at <paramref name="start"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is outside the valid range.</exception>
        /// <seealso cref="AsSpan()"/>
        /// <seealso cref="AsSpan(int, int)"/>
        public ReadOnlySpan<byte> AsSpan(int start) => BytesOrEmpty.AsSpan(start);

        /// <summary>
        /// Returns a read-only span starting at the specified byte offset with the specified length.
        /// </summary>
        /// <param name="start">The zero-based byte index at which to begin the span.</param>
        /// <param name="length">The number of bytes in the span.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> of the specified range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> or <paramref name="length"/> is outside the valid range.
        /// </exception>
        /// <seealso cref="AsSpan()"/>
        /// <seealso cref="Slice(int, int)"/>
        public ReadOnlySpan<byte> AsSpan(int start, int length) => BytesOrEmpty.AsSpan(start, length);

        /// <summary>
        /// Returns a read-only memory region over the entire byte sequence. This is a zero-copy operation.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> over the internal byte array.</returns>
        /// <remarks>
        /// <para>Suitable for async APIs that cannot accept <see cref="ReadOnlySpan{T}"/>.</para>
        /// </remarks>
        /// <seealso cref="AsMemory(int)"/>
        /// <seealso cref="AsMemory(int, int)"/>
        /// <seealso cref="AsSpan()"/>
        public ReadOnlyMemory<byte> AsMemory() => BytesOrEmpty;

        /// <summary>
        /// Returns a read-only memory region starting at the specified byte offset.
        /// </summary>
        /// <param name="start">The zero-based byte index at which to begin.</param>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> starting at <paramref name="start"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is outside the valid range.</exception>
        /// <seealso cref="AsMemory()"/>
        public ReadOnlyMemory<byte> AsMemory(int start)
        {
            byte[] bytes = BytesOrEmpty;
            if ((uint)start > (uint)bytes.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start), "Start must be within the byte sequence.");
            }

            return new ReadOnlyMemory<byte>(bytes, start, bytes.Length - start);
        }

        /// <summary>
        /// Returns a read-only memory region starting at the specified byte offset with the specified length.
        /// </summary>
        /// <param name="start">The zero-based byte index at which to begin.</param>
        /// <param name="length">The number of bytes in the region.</param>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> of the specified range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start"/> or <paramref name="length"/> is outside the valid range.
        /// </exception>
        /// <seealso cref="AsMemory()"/>
        public ReadOnlyMemory<byte> AsMemory(int start, int length)
        {
            byte[] bytes = BytesOrEmpty;
            if ((uint)start > (uint)bytes.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start), "Start must be within the byte sequence.");
            }
            if ((uint)length > (uint)(bytes.Length - start))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length), "Length exceeds the available bytes from start.");
            }

            return new ReadOnlyMemory<byte>(bytes, start, length);
        }

        // -------------------------------------------------------------------
        // Explicit copy
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns a new byte array containing a copy of the bytes in this <see cref="Hex"/> value.
        /// </summary>
        /// <returns>
        /// A new <see cref="byte"/> array. Returns an empty array for <c>default(Hex)</c>
        /// and <see cref="Empty"/>.
        /// </returns>
        /// <remarks>
        /// <para>This method allocates a new array on every call. For zero-copy access,
        /// use <see cref="AsSpan()"/> or <see cref="AsMemory()"/> instead.</para>
        /// </remarks>
        /// <seealso cref="AsSpan()"/>
        /// <seealso cref="TryWriteBytes(Span{byte})"/>
        public byte[] ToByteArray()
        {
            byte[] bytes = BytesOrEmpty;
            if (bytes.Length == 0) return Array.Empty<byte>();
            return (byte[])bytes.Clone();
        }

        /// <summary>
        /// Tries to write the bytes of this <see cref="Hex"/> value into a caller-provided buffer.
        /// </summary>
        /// <param name="destination">The span to write the bytes into.</param>
        /// <returns>
        /// <see langword="true"/> if the bytes were successfully written;
        /// <see langword="false"/> if <paramref name="destination"/> is too small.
        /// </returns>
        /// <remarks>
        /// <para>This method performs no allocation. For hot paths, prefer this over
        /// <see cref="ToByteArray"/> which allocates a new array.</para>
        /// <para>Follows the same pattern as <c>Guid.TryWriteBytes</c>.</para>
        /// </remarks>
        /// <seealso cref="ToByteArray"/>
        /// <seealso cref="AsSpan()"/>
        public bool TryWriteBytes(Span<byte> destination)
        {
            byte[] bytes = BytesOrEmpty;
            if (bytes.Length > destination.Length)
                return false;

            bytes.AsSpan().CopyTo(destination);
            return true;
        }

        // -------------------------------------------------------------------
        // Guid interop
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="Hex"/> from the 16 bytes of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">The GUID to convert.</param>
        /// <returns>A new <see cref="Hex"/> containing the 16 bytes of the GUID.</returns>
        /// <remarks>
        /// <para>The byte order matches <see cref="Guid.ToByteArray()"/>. Note that the first
        /// three components of a GUID are stored in little-endian order on Windows.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Guid guid = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");
        /// Hex hex = Hex.FromGuid(guid);
        /// Console.WriteLine(hex.Length); // 16
        /// </code>
        /// </example>
        /// <seealso cref="ToGuid"/>
        public static Hex FromGuid(Guid value)
        {
            return new Hex(value.ToByteArray(), null);
        }

        /// <summary>
        /// Converts this <see cref="Hex"/> value to a <see cref="Guid"/>.
        /// </summary>
        /// <returns>A <see cref="Guid"/> constructed from the bytes.</returns>
        /// <exception cref="InvalidOperationException">
        /// This <see cref="Hex"/> does not contain exactly 16 bytes.
        /// </exception>
        /// <remarks>
        /// <para>The byte order matches <see cref="Guid(byte[])"/>.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex hex = Hex.Parse("0102030405060708090a0b0c0d0e0f10");
        /// Guid guid = hex.ToGuid();
        /// </code>
        /// </example>
        /// <seealso cref="FromGuid(Guid)"/>
        public Guid ToGuid()
        {
            byte[] bytes = BytesOrEmpty;
            if (bytes.Length != 16)
                ThrowHelper.ThrowInvalidOperationException(
                    $"Cannot convert Hex to Guid: expected exactly 16 bytes but found {bytes.Length}.");

#if NET9_0_OR_GREATER
            return new Guid(bytes.AsSpan());
#else
            return new Guid(bytes);
#endif
        }
    }
}
