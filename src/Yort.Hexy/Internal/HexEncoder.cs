using System;
using System.Text;

namespace Yort.Hexy.Internal
{
    /// <summary>
    /// Encodes byte arrays to hexadecimal character sequences.
    /// Uses platform-specific optimizations where available.
    /// </summary>
    internal static class HexEncoder
    {
        // Lookup tables for .NET Standard 2.0 fallback path
        private static readonly char[] s_lowerLookup = CreateLookup(lowercase: true);
        private static readonly char[] s_upperLookup = CreateLookup(lowercase: false);

        /// <summary>
        /// Encodes bytes to a hex string using the specified casing.
        /// </summary>
        internal static string Encode(byte[] bytes, HexLetterCase letterCase)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

#if NET9_0_OR_GREATER
            return EncodeModern(bytes, letterCase);
#else
            return EncodeLegacy(bytes, letterCase);
#endif
        }

        /// <summary>
        /// Encodes bytes to a hex string with separator and optional prefix.
        /// </summary>
        internal static string Encode(byte[] bytes, HexFormat format)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            if (format.Separator == null && format.Prefix == null)
                return Encode(bytes, format.LetterCase);

#if NET9_0_OR_GREATER
            return EncodeWithFormatModern(bytes, format);
#else
            return EncodeWithFormatLegacy(bytes, format);
#endif
        }

#if NET9_0_OR_GREATER
        private static string EncodeModern(byte[] bytes, HexLetterCase letterCase)
        {
            return letterCase == HexLetterCase.Lower
                ? Convert.ToHexStringLower(bytes)
                : Convert.ToHexString(bytes);
        }

        private static string EncodeWithFormatModern(byte[] bytes, HexFormat format)
        {
            var hexChars = format.LetterCase == HexLetterCase.Lower
                ? Convert.ToHexStringLower(bytes)
                : Convert.ToHexString(bytes);

            if (format.Separator == null)
                return format.Prefix != null ? string.Concat(format.Prefix, hexChars) : hexChars;

            // Calculate total length: prefix + hex pairs with separators
            int separatorLen = format.Separator.Length;
            int pairCount = bytes.Length;
            int totalLen = (format.Prefix?.Length ?? 0) + (pairCount * 2) + (pairCount > 1 ? (pairCount - 1) * separatorLen : 0);

            return string.Create(totalLen, (hexChars, format), static (span, state) =>
            {
                int pos = 0;
                var (hex, fmt) = state;

                // Write prefix
                if (fmt.Prefix != null)
                {
                    fmt.Prefix.AsSpan().CopyTo(span.Slice(pos));
                    pos += fmt.Prefix.Length;
                }

                // Write first pair
                span[pos++] = hex[0];
                span[pos++] = hex[1];

                // Write remaining pairs with separator
                for (int i = 2; i < hex.Length; i += 2)
                {
                    fmt.Separator!.AsSpan().CopyTo(span.Slice(pos));
                    pos += fmt.Separator!.Length;
                    span[pos++] = hex[i];
                    span[pos++] = hex[i + 1];
                }
            });
        }
#endif

#if !NET9_0_OR_GREATER
        private static string EncodeLegacy(byte[] bytes, HexLetterCase letterCase)
        {
            char[] lookup = letterCase == HexLetterCase.Lower ? s_lowerLookup : s_upperLookup;
            char[] result = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                result[i * 2] = lookup[b * 2];
                result[i * 2 + 1] = lookup[b * 2 + 1];
            }

            return new string(result);
        }

        private static string EncodeWithFormatLegacy(byte[] bytes, HexFormat format)
        {
            char[] lookup = format.LetterCase == HexLetterCase.Lower ? s_lowerLookup : s_upperLookup;
            int separatorLen = format.Separator?.Length ?? 0;
            int pairCount = bytes.Length;
            int capacity = (format.Prefix?.Length ?? 0) + (pairCount * 2) + (pairCount > 0 ? (pairCount - 1) * separatorLen : 0);

            var sb = new StringBuilder(capacity);

            if (format.Prefix != null)
                sb.Append(format.Prefix);

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0 && format.Separator != null)
                    sb.Append(format.Separator);

                int b = bytes[i];
                sb.Append(lookup[b * 2]);
                sb.Append(lookup[b * 2 + 1]);
            }

            return sb.ToString();
        }
#endif

        private static char[] CreateLookup(bool lowercase)
        {
            string hexChars = lowercase ? "0123456789abcdef" : "0123456789ABCDEF";
            char[] lookup = new char[512]; // 256 bytes × 2 chars each

            for (int i = 0; i < 256; i++)
            {
                lookup[i * 2] = hexChars[i >> 4];
                lookup[i * 2 + 1] = hexChars[i & 0xF];
            }

            return lookup;
        }
    }
}
