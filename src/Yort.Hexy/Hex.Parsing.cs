using System;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    public readonly partial struct Hex
    {
        /// <summary>
        /// Parses a hexadecimal string into a <see cref="Hex"/> value.
        /// </summary>
        /// <param name="hex">
        /// A string containing hexadecimal characters. May include a <c>0x</c>, <c>0X</c>, or <c>#</c>
        /// prefix, and may use colon, dash, or space separators between byte pairs. Leading and trailing
        /// whitespace is trimmed. Must contain an even number of hex digits after prefix/separator removal.
        /// </param>
        /// <returns>A new <see cref="Hex"/> value containing the parsed bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hex"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">
        /// <paramref name="hex"/> contains non-hexadecimal characters after prefix and separator removal,
        /// or has an odd number of hex digits (which would represent a partial byte).
        /// The exception message includes the position of the first invalid character.
        /// </exception>
        /// <remarks>
        /// <para>Parsing is case-insensitive. The following formats are all accepted:</para>
        /// <list type="bullet">
        /// <item><description>Plain: <c>"deadbeef"</c> or <c>"DEADBEEF"</c></description></item>
        /// <item><description>Prefixed: <c>"0xDEADBEEF"</c> or <c>"#FF00AA"</c></description></item>
        /// <item><description>Separated: <c>"DE:AD:BE:EF"</c>, <c>"DE-AD-BE-EF"</c>, or <c>"DE AD BE EF"</c></description></item>
        /// </list>
        /// <para>The parsed string is cached internally in normalized lowercase form. If <see cref="ToString()"/>
        /// is subsequently called with the default <see cref="HexFormat.Lowercase"/> format, the cached string
        /// is returned with zero allocation.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex value = Hex.Parse("0xDEADBEEF");
        /// Console.WriteLine(value.Length); // 4
        /// Console.WriteLine(value);        // "deadbeef"
        ///
        /// Hex separated = Hex.Parse("DE:AD:BE:EF");
        /// Console.WriteLine(separated == value); // True
        /// </code>
        /// </example>
        /// <seealso cref="TryParse(string, out Hex)"/>
        /// <seealso cref="op_Explicit(string)"/>
        public static Hex Parse(string hex)
        {
            if (hex == null) ThrowHelper.ThrowArgumentNullException(nameof(hex));

            if (!HexDecoder.TryDecode(hex!, out byte[]? bytes, out string? normalizedLowerHex, out string? errorMessage))
                ThrowHelper.ThrowFormatException(errorMessage!);

            return new Hex(bytes!, normalizedLowerHex);
        }

        /// <summary>
        /// Attempts to parse a hexadecimal string into a <see cref="Hex"/> value without throwing exceptions.
        /// </summary>
        /// <param name="hex">
        /// A string containing hexadecimal characters. May include a <c>0x</c>, <c>0X</c>, or <c>#</c>
        /// prefix, and may use colon, dash, or space separators between byte pairs. Leading and trailing
        /// whitespace is trimmed. Can be <see langword="null"/>.
        /// </param>
        /// <param name="result">
        /// When this method returns <see langword="true"/>, contains the parsed <see cref="Hex"/> value.
        /// When this method returns <see langword="false"/>, contains <c>default(Hex)</c>.
        /// </param>
        /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method never throws. It returns <see langword="false"/> for <see langword="null"/>
        /// input, invalid characters, or odd-length hex strings.</para>
        /// <para>Like <see cref="Parse(string)"/>, the normalized lowercase hex string is cached
        /// internally on successful parse.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (Hex.TryParse("0xCAFE", out Hex result))
        ///     Console.WriteLine(result); // "cafe"
        ///
        /// bool ok = Hex.TryParse("not hex", out _); // false
        /// </code>
        /// </example>
        /// <seealso cref="Parse(string)"/>
        public static bool TryParse(string? hex, out Hex result)
        {
            if (hex == null)
            {
                result = default;
                return false;
            }

            if (!HexDecoder.TryDecode(hex, out byte[]? bytes, out string? normalizedLowerHex, out _))
            {
                result = default;
                return false;
            }

            result = new Hex(bytes!, normalizedLowerHex);
            return true;
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Parses a read-only span of characters into a <see cref="Hex"/> value.
        /// </summary>
        /// <param name="hex">A span containing hexadecimal characters. Accepts the same formats as <see cref="Parse(string)"/>.</param>
        /// <returns>A new <see cref="Hex"/> value containing the parsed bytes.</returns>
        /// <exception cref="FormatException">
        /// <paramref name="hex"/> contains non-hexadecimal characters or has an odd number of hex digits.
        /// </exception>
        /// <seealso cref="Parse(string)"/>
        /// <seealso cref="TryParse(ReadOnlySpan{char}, out Hex)"/>
        public static Hex Parse(ReadOnlySpan<char> hex)
        {
            if (!HexDecoder.TryDecodeSpan(hex, out byte[]? bytes, out string? normalizedLowerHex, out string? errorMessage))
                ThrowHelper.ThrowFormatException(errorMessage!);

            return new Hex(bytes!, normalizedLowerHex);
        }

        /// <summary>
        /// Attempts to parse a read-only span of characters into a <see cref="Hex"/> value without throwing exceptions.
        /// </summary>
        /// <param name="hex">A span containing hexadecimal characters.</param>
        /// <param name="result">
        /// When this method returns <see langword="true"/>, contains the parsed <see cref="Hex"/> value.
        /// When this method returns <see langword="false"/>, contains <c>default(Hex)</c>.
        /// </param>
        /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="TryParse(string, out Hex)"/>
        public static bool TryParse(ReadOnlySpan<char> hex, out Hex result)
        {
            if (!HexDecoder.TryDecodeSpan(hex, out byte[]? bytes, out string? normalizedLowerHex, out _))
            {
                result = default;
                return false;
            }

            result = new Hex(bytes!, normalizedLowerHex);
            return true;
        }

        // IParsable<Hex>
        static Hex IParsable<Hex>.Parse(string s, IFormatProvider? provider) => Parse(s);
        static bool IParsable<Hex>.TryParse(string? s, IFormatProvider? provider, out Hex result) => TryParse(s, out result);

        // ISpanParsable<Hex>
        static Hex ISpanParsable<Hex>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s);
        static bool ISpanParsable<Hex>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Hex result) => TryParse(s, out result);
#endif
    }
}
