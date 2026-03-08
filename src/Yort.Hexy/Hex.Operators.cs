using System;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    public readonly partial struct Hex
    {
        // -------------------------------------------------------------------
        // Concatenation
        // -------------------------------------------------------------------

        /// <summary>
        /// Concatenates two <see cref="Hex"/> values into a new <see cref="Hex"/> containing
        /// the bytes of <paramref name="left"/> followed by the bytes of <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first hex value.</param>
        /// <param name="right">The second hex value.</param>
        /// <returns>A new <see cref="Hex"/> containing the combined byte sequences.</returns>
        /// <remarks>
        /// <para>This operator allocates a new byte array sized to hold both values. For
        /// combining many values efficiently, use <see cref="HexBuilder"/> instead.</para>
        /// </remarks>
        /// <seealso cref="Append(Hex)"/>
        /// <seealso cref="Concat(Hex[])"/>
        /// <seealso cref="HexBuilder"/>
        public static Hex operator +(Hex left, Hex right) => left.Append(right);

        /// <summary>
        /// Returns a new <see cref="Hex"/> containing this instance's bytes followed by
        /// <paramref name="other"/>'s bytes.
        /// </summary>
        /// <param name="other">The hex value to append.</param>
        /// <returns>A new <see cref="Hex"/> containing the combined byte sequences. This instance is not modified.</returns>
        /// <remarks>
        /// <para>This method allocates a new byte array. For combining many values, use
        /// <see cref="HexBuilder"/> instead.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex a = Hex.Parse("DEAD");
        /// Hex b = Hex.Parse("BEEF");
        /// Hex combined = a.Append(b);
        /// Console.WriteLine(combined); // "deadbeef"
        /// </code>
        /// </example>
        /// <seealso cref="op_Addition(Hex, Hex)"/>
        /// <seealso cref="Concat(Hex[])"/>
        /// <seealso cref="HexBuilder"/>
        public Hex Append(Hex other)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = other.BytesOrEmpty;

            if (a.Length == 0) return other;
            if (b.Length == 0) return this;

            byte[] result = new byte[a.Length + b.Length];
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return new Hex(result, null);
        }

        /// <summary>
        /// Concatenates multiple <see cref="Hex"/> values into a single <see cref="Hex"/>.
        /// Performs a single allocation sized to the total byte count.
        /// </summary>
        /// <param name="values">The values to concatenate in order.</param>
        /// <returns>A new <see cref="Hex"/> containing all bytes in sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>More efficient than chaining <c>+</c> operators for 3+ values because it
        /// computes the total length first and performs a single allocation.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex result = Hex.Concat(
        ///     Hex.Parse("DE"),
        ///     Hex.Parse("AD"),
        ///     Hex.Parse("BEEF")
        /// );
        /// Console.WriteLine(result); // "deadbeef"
        /// </code>
        /// </example>
        /// <seealso cref="Append(Hex)"/>
        /// <seealso cref="HexBuilder"/>
        public static Hex Concat(params Hex[] values)
        {
            if (values == null) ThrowHelper.ThrowArgumentNullException(nameof(values));

            long totalLen = 0;
            for (int i = 0; i < values!.Length; i++)
                totalLen += values[i].Length;

            if (totalLen == 0) return Empty;
            if (totalLen > int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(values), "Combined length exceeds maximum array size.");

            byte[] result = new byte[(int)totalLen];
            int offset = 0;
            for (int i = 0; i < values.Length; i++)
            {
                byte[] src = values[i].BytesOrEmpty;
                Array.Copy(src, 0, result, offset, src.Length);
                offset += src.Length;
            }

            return new Hex(result, null);
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Concatenates a span of <see cref="Hex"/> values into a single <see cref="Hex"/>.
        /// </summary>
        /// <param name="values">The values to concatenate in order.</param>
        /// <returns>A new <see cref="Hex"/> containing all bytes in sequence.</returns>
        /// <seealso cref="Concat(Hex[])"/>
        public static Hex Concat(ReadOnlySpan<Hex> values)
        {
            long totalLen = 0;
            for (int i = 0; i < values.Length; i++)
                totalLen += values[i].Length;

            if (totalLen == 0) return Empty;
            if (totalLen > int.MaxValue) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(values), "Combined length exceeds maximum array size.");

            byte[] result = new byte[(int)totalLen];
            int offset = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var src = values[i].BytesOrEmpty;
                src.AsSpan().CopyTo(result.AsSpan(offset));
                offset += src.Length;
            }

            return new Hex(result, null);
        }
#endif

        // -------------------------------------------------------------------
        // Slicing
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns a new <see cref="Hex"/> containing a sub-range of bytes from this instance.
        /// </summary>
        /// <param name="offset">The zero-based byte index at which the slice begins.</param>
        /// <param name="length">The number of bytes in the slice.</param>
        /// <returns>A new <see cref="Hex"/> containing the specified byte range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="length"/> is negative, or their sum
        /// exceeds <see cref="Length"/>.
        /// </exception>
        /// <remarks>
        /// <para>Allocates a new byte array for the slice. For zero-copy access to a sub-range,
        /// use <see cref="AsSpan(int, int)"/> or <see cref="AsMemory(int, int)"/> instead.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex value = Hex.Parse("DEADBEEF");
        /// Hex slice = value.Slice(1, 2);
        /// Console.WriteLine(slice); // "adbe"
        /// </code>
        /// </example>
        /// <seealso cref="AsSpan(int, int)"/>
        public Hex Slice(int offset, int length)
        {
            byte[] bytes = BytesOrEmpty;

            if (offset < 0) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");
            if (length < 0) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
            if ((long)offset + length > bytes.Length) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length), "Offset and length exceed the hex value's length.");

            if (length == 0) return Empty;
            if (offset == 0 && length == bytes.Length) return this;

            byte[] result = new byte[length];
            Array.Copy(bytes, offset, result, 0, length);
            return new Hex(result, null);
        }

        // -------------------------------------------------------------------
        // Pattern matching
        // -------------------------------------------------------------------

        /// <summary>
        /// Determines whether this <see cref="Hex"/> starts with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the byte sequence of this instance begins with the byte sequence
        /// of <paramref name="prefix"/>; otherwise, <see langword="false"/>. An empty prefix always
        /// returns <see langword="true"/>.
        /// </returns>
        /// <seealso cref="EndsWith(Hex)"/>
        /// <seealso cref="Contains(Hex)"/>
        public bool StartsWith(Hex prefix)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = prefix.BytesOrEmpty;

            if (b.Length > a.Length) return false;
            if (b.Length == 0) return true;

            return a.AsSpan(0, b.Length).SequenceEqual(b.AsSpan());
        }

        /// <summary>
        /// Determines whether this <see cref="Hex"/> ends with the specified suffix.
        /// </summary>
        /// <param name="suffix">The suffix to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the byte sequence of this instance ends with the byte sequence
        /// of <paramref name="suffix"/>; otherwise, <see langword="false"/>. An empty suffix always
        /// returns <see langword="true"/>.
        /// </returns>
        /// <seealso cref="StartsWith(Hex)"/>
        /// <seealso cref="Contains(Hex)"/>
        public bool EndsWith(Hex suffix)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = suffix.BytesOrEmpty;

            if (b.Length > a.Length) return false;
            if (b.Length == 0) return true;

            int offset = a.Length - b.Length;

            return a.AsSpan(offset, b.Length).SequenceEqual(b.AsSpan());
        }

        /// <summary>
        /// Determines whether this <see cref="Hex"/> contains the specified value as a
        /// contiguous byte subsequence.
        /// </summary>
        /// <param name="value">The byte sequence to search for.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value"/>'s bytes appear as a contiguous
        /// subsequence; otherwise, <see langword="false"/>. An empty value always returns
        /// <see langword="true"/>.
        /// </returns>
        /// <seealso cref="StartsWith(Hex)"/>
        /// <seealso cref="EndsWith(Hex)"/>
        public bool Contains(Hex value)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = value.BytesOrEmpty;

            if (b.Length == 0) return true;
            if (b.Length > a.Length) return false;

            return a.AsSpan().IndexOf(b.AsSpan()) >= 0;
        }

        // -------------------------------------------------------------------
        // Reverse
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns a new <see cref="Hex"/> with the byte order reversed.
        /// </summary>
        /// <returns>
        /// A new <see cref="Hex"/> with bytes in reverse order. This instance is not modified.
        /// Returns <see cref="Empty"/> if this instance is empty.
        /// </returns>
        /// <remarks>
        /// <para>Useful for converting between big-endian and little-endian byte orders.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex value = Hex.Parse("DEADBEEF");
        /// Hex reversed = value.Reverse();
        /// Console.WriteLine(reversed); // "efbeadde"
        /// </code>
        /// </example>
        public Hex Reverse()
        {
            byte[] bytes = BytesOrEmpty;
            if (bytes.Length <= 1) return this;

            byte[] result = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = bytes[bytes.Length - 1 - i];
            }
            return new Hex(result, null);
        }
    }
}
