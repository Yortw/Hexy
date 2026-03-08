using System;
using System.Text;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    public readonly partial struct Hex
    {
        /// <summary>
        /// Returns the hexadecimal string representation using the application default format
        /// configured via <see cref="HexDefaults.Format"/>.
        /// </summary>
        /// <returns>
        /// A string containing the hexadecimal representation of the bytes. Returns <see cref="string.Empty"/>
        /// for <c>default(Hex)</c> or <see cref="Empty"/>.
        /// </returns>
        /// <remarks>
        /// <para>If the default format is <see cref="HexFormat.Lowercase"/> and this instance was
        /// created via <see cref="Parse(string)"/>, the internally cached string is returned
        /// with zero allocation.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex value = Hex.Parse("DEADBEEF");
        /// Console.WriteLine(value.ToString()); // "deadbeef" (default is lowercase)
        ///
        /// HexDefaults.Format = HexFormat.UppercaseColons;
        /// Console.WriteLine(value.ToString()); // "DE:AD:BE:EF"
        /// </code>
        /// </example>
        /// <seealso cref="ToString(HexFormat)"/>
        /// <seealso cref="HexDefaults"/>
        public override string ToString()
        {
            return ToString(HexDefaults.Format);
        }

        /// <summary>
        /// Returns the hexadecimal string representation using the specified <see cref="HexFormat"/>.
        /// </summary>
        /// <param name="format">
        /// The format to use. Must not be <see langword="null"/>.
        /// Use one of the pre-built formats (e.g., <see cref="HexFormat.Uppercase"/>,
        /// <see cref="HexFormat.LowercaseColons"/>) or construct a custom <see cref="HexFormat"/>.
        /// </param>
        /// <returns>
        /// A string containing the formatted hexadecimal representation. Returns <see cref="string.Empty"/>
        /// for empty values.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>When <paramref name="format"/> is the canonical <see cref="HexFormat.Lowercase"/>
        /// (no separator, no prefix) and the instance was created via a parse method, the cached
        /// string is returned with zero allocation.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex value = Hex.Parse("DEADBEEF");
        /// Console.WriteLine(value.ToString(HexFormat.Uppercase));       // "DEADBEEF"
        /// Console.WriteLine(value.ToString(HexFormat.LowercaseColons)); // "de:ad:be:ef"
        /// Console.WriteLine(value.ToString(HexFormat.LowercasePrefixed)); // "0xdeadbeef"
        /// </code>
        /// </example>
        /// <seealso cref="ToString()"/>
        /// <seealso cref="HexFormat"/>
        public string ToString(HexFormat format)
        {
            if (format == null) ThrowHelper.ThrowArgumentNullException(nameof(format));

            // Fast path: return cached string if format matches canonical lowercase
            if (format!.IsCanonicalLowercase && _cachedLowerHex != null)
                return _cachedLowerHex;

            byte[] bytes = BytesOrEmpty;

            if (format.Separator != null || format.Prefix != null)
                return HexEncoder.Encode(bytes, format);

            return HexEncoder.Encode(bytes, format.LetterCase);
        }

        /// <summary>
        /// Returns the hexadecimal string representation using a format specifier string.
        /// </summary>
        /// <param name="format">
        /// A format specifier string, or <see langword="null"/> to use the default format.
        /// Supported specifiers: <c>"l"</c> (lowercase), <c>"U"</c> (uppercase),
        /// <c>"0x"</c> / <c>"0X"</c> (prefixed), <c>"l:"</c> / <c>"U:"</c> (with colons),
        /// <c>"l-"</c> / <c>"U-"</c> (with dashes), <c>"l "</c> / <c>"U "</c> (with spaces).
        /// </param>
        /// <param name="formatProvider">Ignored. Hex formatting does not vary by culture.</param>
        /// <returns>A formatted hexadecimal string.</returns>
        /// <exception cref="FormatException"><paramref name="format"/> is not a recognized specifier.</exception>
        /// <seealso cref="ToString(HexFormat)"/>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ToString(ResolveFormatSpecifier(format));
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Tries to format the hex value into the provided span of characters.
        /// </summary>
        /// <param name="destination">The span to write the formatted characters into.</param>
        /// <param name="charsWritten">When this method returns, the number of characters written.</param>
        /// <param name="format">A format specifier string, or empty/default for the default format.</param>
        /// <param name="provider">Ignored.</param>
        /// <returns><see langword="true"/> if the formatting succeeded; <see langword="false"/> if the destination was too small.</returns>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            // TODO: allocates a string internally; replace with direct-to-span encoding for true zero-alloc
            string result = ToString(ResolveFormatSpecifier(format.Length == 0 ? null : format.ToString()));
            if (result.Length > destination.Length)
            {
                charsWritten = 0;
                return false;
            }

            result.AsSpan().CopyTo(destination);
            charsWritten = result.Length;
            return true;
        }

        /// <summary>
        /// Tries to format the hex value into the provided span of UTF-8 bytes.
        /// </summary>
        /// <param name="utf8Destination">The span to write the UTF-8 bytes into.</param>
        /// <param name="bytesWritten">When this method returns, the number of bytes written.</param>
        /// <param name="format">A format specifier string, or empty/default for the default format.</param>
        /// <param name="provider">Ignored.</param>
        /// <returns><see langword="true"/> if the formatting succeeded; <see langword="false"/> if the destination was too small.</returns>
        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            // TODO: allocates a string internally; replace with direct-to-span encoding for true zero-alloc
            string result = ToString(ResolveFormatSpecifier(format.Length == 0 ? null : format.ToString()));
            int required = Encoding.UTF8.GetByteCount(result);
            if (required > utf8Destination.Length)
            {
                bytesWritten = 0;
                return false;
            }

            bytesWritten = Encoding.UTF8.GetBytes(result.AsSpan(), utf8Destination);
            return true;
        }
#endif

        /// <summary>
        /// Resolves a short format specifier string to the corresponding <see cref="HexFormat"/> instance.
        /// </summary>
        private static HexFormat ResolveFormatSpecifier(string? format)
        {
            if (string.IsNullOrEmpty(format))
                return HexDefaults.Format;

            return format switch
            {
                "l" => HexFormat.Lowercase,
                "U" => HexFormat.Uppercase,
                "0x" => HexFormat.LowercasePrefixed,
                "0X" => HexFormat.UppercasePrefixed,
                "l:" => HexFormat.LowercaseColons,
                "U:" => HexFormat.UppercaseColons,
                "l-" => HexFormat.LowercaseDashes,
                "U-" => HexFormat.UppercaseDashes,
                "l " => HexFormat.LowercaseSpaces,
                "U " => HexFormat.UppercaseSpaces,
                _ => throw new FormatException($"Unknown hex format specifier: \"{format}\". " +
                    "Supported specifiers: \"l\", \"U\", \"0x\", \"0X\", \"l:\", \"U:\", \"l-\", \"U-\", \"l \", \"U \".")
            };
        }
    }
}
