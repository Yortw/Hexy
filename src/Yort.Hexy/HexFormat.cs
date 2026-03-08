using System;

namespace Yort.Hexy
{
    /// <summary>
    /// Specifies the letter casing used when formatting hexadecimal characters.
    /// </summary>
    public enum HexLetterCase
    {
        /// <summary>
        /// Lowercase hexadecimal characters (a-f). Example: <c>"deadbeef"</c>.
        /// </summary>
        Lower = 0,

        /// <summary>
        /// Uppercase hexadecimal characters (A-F). Example: <c>"DEADBEEF"</c>.
        /// </summary>
        Upper = 1
    }

    /// <summary>
    /// Describes how a <see cref="Hex"/> value should be formatted as a string.
    /// Combines letter casing, an optional byte-pair separator, and an optional prefix.
    /// </summary>
    /// <remarks>
    /// <para>Use one of the pre-built static instances for common formats, or construct a
    /// custom <see cref="HexFormat"/> for specialized needs.</para>
    /// <para>Instances of this class are immutable and safe to share across threads.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use a pre-built format:
    /// string result = myHex.ToString(HexFormat.UppercaseColons); // "DE:AD:BE:EF"
    ///
    /// // Or create a custom format:
    /// var fmt = new HexFormat(HexLetterCase.Lower, separator: ".", prefix: "0x");
    /// string result = myHex.ToString(fmt); // "0xde.ad.be.ef"
    /// </code>
    /// </example>
    public sealed class HexFormat
    {
        /// <summary>
        /// Initializes a new <see cref="HexFormat"/> with the specified casing, separator, and prefix.
        /// </summary>
        /// <param name="letterCase">The letter casing to use for hexadecimal digits a-f / A-F.</param>
        /// <param name="separator">
        /// An optional string inserted between each byte pair in the output (e.g., <c>":"</c>, <c>"-"</c>, <c>" "</c>).
        /// Pass <see langword="null"/> or <see cref="string.Empty"/> for no separator.
        /// </param>
        /// <param name="prefix">
        /// An optional string prepended to the output (e.g., <c>"0x"</c>, <c>"#"</c>).
        /// Pass <see langword="null"/> or <see cref="string.Empty"/> for no prefix.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="letterCase"/> is not a defined <see cref="HexLetterCase"/> value.
        /// </exception>
        public HexFormat(HexLetterCase letterCase, string? separator = null, string? prefix = null)
        {
            if (letterCase != HexLetterCase.Lower && letterCase != HexLetterCase.Upper)
                throw new ArgumentException($"Invalid {nameof(HexLetterCase)} value: {letterCase}.", nameof(letterCase));

            LetterCase = letterCase;
            Separator = string.IsNullOrEmpty(separator) ? null : separator;
            Prefix = string.IsNullOrEmpty(prefix) ? null : prefix;
        }

        /// <summary>
        /// Gets the letter casing used for hexadecimal digits.
        /// </summary>
        /// <value>The <see cref="HexLetterCase"/> for this format.</value>
        public HexLetterCase LetterCase { get; }

        /// <summary>
        /// Gets the separator inserted between each byte pair, or <see langword="null"/> if none.
        /// </summary>
        /// <value>A separator string such as <c>":"</c>, <c>"-"</c>, or <c>" "</c>, or <see langword="null"/>.</value>
        public string? Separator { get; }

        /// <summary>
        /// Gets the prefix prepended to the formatted output, or <see langword="null"/> if none.
        /// </summary>
        /// <value>A prefix string such as <c>"0x"</c> or <c>"#"</c>, or <see langword="null"/>.</value>
        public string? Prefix { get; }

        /// <summary>Lowercase, no separator, no prefix. Output: <c>"deadbeef"</c>.</summary>
        public static HexFormat Lowercase { get; } = new HexFormat(HexLetterCase.Lower);

        /// <summary>Uppercase, no separator, no prefix. Output: <c>"DEADBEEF"</c>.</summary>
        public static HexFormat Uppercase { get; } = new HexFormat(HexLetterCase.Upper);

        /// <summary>Lowercase with <c>"0x"</c> prefix. Output: <c>"0xdeadbeef"</c>.</summary>
        public static HexFormat LowercasePrefixed { get; } = new HexFormat(HexLetterCase.Lower, prefix: "0x");

        /// <summary>Uppercase with <c>"0X"</c> prefix. Output: <c>"0XDEADBEEF"</c>.</summary>
        public static HexFormat UppercasePrefixed { get; } = new HexFormat(HexLetterCase.Upper, prefix: "0X");

        /// <summary>Lowercase with colon separator. Output: <c>"de:ad:be:ef"</c>.</summary>
        public static HexFormat LowercaseColons { get; } = new HexFormat(HexLetterCase.Lower, separator: ":");

        /// <summary>Uppercase with colon separator. Output: <c>"DE:AD:BE:EF"</c>.</summary>
        public static HexFormat UppercaseColons { get; } = new HexFormat(HexLetterCase.Upper, separator: ":");

        /// <summary>Lowercase with dash separator. Output: <c>"de-ad-be-ef"</c>.</summary>
        public static HexFormat LowercaseDashes { get; } = new HexFormat(HexLetterCase.Lower, separator: "-");

        /// <summary>Uppercase with dash separator. Output: <c>"DE-AD-BE-EF"</c>.</summary>
        public static HexFormat UppercaseDashes { get; } = new HexFormat(HexLetterCase.Upper, separator: "-");

        /// <summary>Lowercase with space separator. Output: <c>"de ad be ef"</c>.</summary>
        public static HexFormat LowercaseSpaces { get; } = new HexFormat(HexLetterCase.Lower, separator: " ");

        /// <summary>Uppercase with space separator. Output: <c>"DE AD BE EF"</c>.</summary>
        public static HexFormat UppercaseSpaces { get; } = new HexFormat(HexLetterCase.Upper, separator: " ");

        /// <summary>
        /// Returns <see langword="true"/> if this format is plain lowercase with no separator and no prefix,
        /// matching the canonical cached form used internally by <see cref="Hex"/>.
        /// </summary>
        internal bool IsCanonicalLowercase => LetterCase == HexLetterCase.Lower && Separator == null && Prefix == null;
    }
}
