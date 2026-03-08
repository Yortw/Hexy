using System;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    /// <summary>
    /// A mutable builder for efficiently constructing <see cref="Hex"/> values from many parts,
    /// analogous to <see cref="System.Text.StringBuilder"/>.
    /// </summary>
    /// <remarks>
    /// <para>Backed by a single <see cref="byte"/> array buffer with geometric growth.
    /// <see cref="Append(Hex)"/> copies bytes into the buffer. <see cref="ToHex"/> allocates
    /// the final right-sized array once.</para>
    ///
    /// <para>Use <see cref="HexBuilder"/> when combining many hex values to avoid the
    /// intermediate allocations that would result from chaining <c>+</c> operators
    /// or <see cref="Hex.Append(Hex)"/>.</para>
    ///
    /// <para>This class is not thread-safe. If multiple threads need to build hex values
    /// concurrently, each thread should use its own <see cref="HexBuilder"/> instance.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = new HexBuilder();
    /// builder.Append(Hex.Parse("DEAD"));
    /// builder.Append(Hex.Parse("BEEF"));
    /// builder.Append(new byte[] { 0xCA, 0xFE });
    ///
    /// Hex result = builder.ToHex();
    /// Console.WriteLine(result); // "deadbeefcafe"
    /// </code>
    /// </example>
    /// <seealso cref="Hex.Append(Hex)"/>
    /// <seealso cref="Hex.Concat(Hex[])"/>
    public sealed class HexBuilder
    {
        private const int DefaultCapacity = 16;
        private byte[] _buffer;
        private int _length;

        /// <summary>
        /// Initializes a new <see cref="HexBuilder"/> with the default initial capacity (16 bytes).
        /// </summary>
        public HexBuilder()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="HexBuilder"/> with the specified initial byte capacity.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial buffer size in bytes. Must be non-negative.
        /// If zero, a minimal buffer is allocated on the first <see cref="Append(Hex)"/> call.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCapacity"/> is negative.</exception>
        public HexBuilder(int initialCapacity)
        {
            if (initialCapacity < 0) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be non-negative.");
            _buffer = initialCapacity == 0 ? Array.Empty<byte>() : new byte[initialCapacity];
            _length = 0;
        }

        /// <summary>
        /// Gets the total number of bytes accumulated so far.
        /// </summary>
        /// <value>The current byte count.</value>
        public int Length => _length;

        /// <summary>
        /// Gets the current buffer capacity in bytes.
        /// </summary>
        /// <value>The capacity of the internal buffer.</value>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Appends the bytes of a <see cref="Hex"/> value to this builder.
        /// </summary>
        /// <param name="value">The hex value whose bytes to append.</param>
        /// <returns>This builder instance, for fluent chaining.</returns>
        /// <example>
        /// <code>
        /// Hex result = new HexBuilder()
        ///     .Append(Hex.Parse("DEAD"))
        ///     .Append(Hex.Parse("BEEF"))
        ///     .ToHex();
        /// </code>
        /// </example>
        public HexBuilder Append(Hex value)
        {
            byte[] src = value.BytesOrEmpty;
            if (src.Length == 0) return this;
            EnsureCapacity(checked(_length + src.Length));
            Array.Copy(src, 0, _buffer, _length, src.Length);
            _length += src.Length;
            return this;
        }

        /// <summary>
        /// Appends the specified byte array to this builder.
        /// </summary>
        /// <param name="bytes">The bytes to append. Must not be <see langword="null"/>.</param>
        /// <returns>This builder instance, for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
        public HexBuilder Append(byte[] bytes)
        {
            if (bytes == null) ThrowHelper.ThrowArgumentNullException(nameof(bytes));
            if (bytes!.Length == 0) return this;
            EnsureCapacity(checked(_length + bytes.Length));
            Array.Copy(bytes, 0, _buffer, _length, bytes.Length);
            _length += bytes.Length;
            return this;
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Appends the specified span of bytes to this builder.
        /// </summary>
        /// <param name="bytes">The bytes to append.</param>
        /// <returns>This builder instance, for fluent chaining.</returns>
        public HexBuilder Append(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty) return this;
            EnsureCapacity(checked(_length + bytes.Length));
            bytes.CopyTo(_buffer.AsSpan(_length));
            _length += bytes.Length;
            return this;
        }
#endif

        /// <summary>
        /// Appends a single byte to this builder.
        /// </summary>
        /// <param name="value">The byte to append.</param>
        /// <returns>This builder instance, for fluent chaining.</returns>
        public HexBuilder Append(byte value)
        {
            EnsureCapacity(checked(_length + 1));
            _buffer[_length++] = value;
            return this;
        }

        /// <summary>
        /// Materializes the accumulated bytes into a new <see cref="Hex"/> value.
        /// </summary>
        /// <returns>
        /// A new <see cref="Hex"/> containing a copy of the accumulated bytes.
        /// Returns <see cref="Hex.Empty"/> if no bytes have been appended.
        /// </returns>
        /// <remarks>
        /// <para>This method allocates one right-sized byte array. The builder remains usable
        /// after this call — you can continue appending and call <see cref="ToHex"/> again.</para>
        /// </remarks>
        public Hex ToHex()
        {
            if (_length == 0) return Hex.Empty;

            byte[] result = new byte[_length];
            Array.Copy(_buffer, 0, result, 0, _length);
            return new Hex(result, null);
        }

        /// <summary>
        /// Resets the builder to zero length, allowing it to be reused without reallocation.
        /// The internal buffer is retained at its current capacity.
        /// </summary>
        /// <remarks>
        /// <para>The buffer contents are not zeroed. If this builder may contain sensitive data
        /// (e.g., cryptographic keys), use <see cref="Clear(bool)"/> with <c>zeroBuffer: true</c>.</para>
        /// </remarks>
        public void Clear()
        {
            _length = 0;
        }

        /// <summary>
        /// Resets the builder to zero length, optionally zeroing the internal buffer.
        /// </summary>
        /// <param name="zeroBuffer">
        /// If <see langword="true"/>, overwrites the used portion of the buffer with zeros before
        /// resetting. Use this when the builder contains sensitive data such as cryptographic keys.
        /// </param>
        public void Clear(bool zeroBuffer)
        {
            if (zeroBuffer && _length > 0)
            {
                Array.Clear(_buffer, 0, _length);
            }

            _length = 0;
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _buffer.Length) return;

            long newCapacity = _buffer.Length == 0 ? DefaultCapacity : (long)_buffer.Length * 2;
            while (newCapacity < required)
            {
                newCapacity *= 2;
            }

            if (newCapacity > int.MaxValue)
            {
                newCapacity = required;
            }

            byte[] newBuffer = new byte[(int)newCapacity];
            if (_length > 0)
            {
                Array.Copy(_buffer, 0, newBuffer, 0, _length);
            }

            _buffer = newBuffer;
        }
    }
}
