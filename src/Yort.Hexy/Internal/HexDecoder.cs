using System;

namespace Yort.Hexy.Internal
{
    /// <summary>
    /// Decodes hexadecimal character sequences to byte arrays.
    /// Handles permissive input formats: prefixes (0x, 0X, #), separators (:, -, space), whitespace.
    /// </summary>
    internal static class HexDecoder
    {
        /// <summary>
        /// Attempts to decode a hex string, returning the byte array and the normalized lowercase hex string.
        /// </summary>
        /// <param name="input">The input string to decode.</param>
        /// <param name="bytes">When successful, the decoded bytes.</param>
        /// <param name="normalizedLowerHex">When successful, the normalized lowercase hex string (for caching).</param>
        /// <param name="errorMessage">When unsuccessful, a descriptive error message.</param>
        /// <returns><see langword="true"/> if decoding succeeded; otherwise, <see langword="false"/>.</returns>
        internal static bool TryDecode(string input, out byte[]? bytes, out string? normalizedLowerHex, out string? errorMessage)
        {
#if NET9_0_OR_GREATER
            return TryDecodeSpan(input.AsSpan(), out bytes, out normalizedLowerHex, out errorMessage);
#else
            return TryDecodeLegacy(input, out bytes, out normalizedLowerHex, out errorMessage);
#endif
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Span-based decode for .NET 9+.
        /// </summary>
        internal static bool TryDecodeSpan(ReadOnlySpan<char> input, out byte[]? bytes, out string? normalizedLowerHex, out string? errorMessage)
        {
            bytes = null;
            normalizedLowerHex = null;
            errorMessage = null;

            // Trim whitespace — track leading whitespace for error position reporting
            int leadingWhitespace = 0;
            while (leadingWhitespace < input.Length && char.IsWhiteSpace(input[leadingWhitespace]))
            {
                leadingWhitespace++;
            }

            var trimmed = input.Trim();

            if (trimmed.IsEmpty)
            {
                bytes = Array.Empty<byte>();
                normalizedLowerHex = string.Empty;
                return true;
            }

            // Strip prefix — track prefix length for error position reporting
            var hex = StripPrefixSpan(trimmed);
            int positionOffset = leadingWhitespace + (trimmed.Length - hex.Length);

            // Strip separators and collect clean hex chars
            Span<char> clean = hex.Length <= 128
                ? stackalloc char[hex.Length]
                : new char[hex.Length];

            int cleanLen = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                char c = hex[i];
                if (c == ':' || c == '-' || c == ' ')
                    continue;

                if (!IsHexChar(c))
                {
                    errorMessage = $"Invalid hexadecimal character '{c}' (U+{(int)c:X4}) at position {positionOffset + i}.";
                    return false;
                }

                clean[cleanLen++] = c;
            }

            var cleanSpan = clean.Slice(0, cleanLen);

            // Check even length
            if (cleanLen % 2 != 0)
            {
                errorMessage = $"Hexadecimal string has an odd number of characters ({cleanLen}). " +
                    "Hex strings must have an even number of characters to represent complete bytes.";
                return false;
            }

            // Decode
            int byteCount = cleanLen / 2;
            byte[] result = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                int hi = HexCharToNibble(cleanSpan[i * 2]);
                int lo = HexCharToNibble(cleanSpan[i * 2 + 1]);
                result[i] = (byte)((hi << 4) | lo);
            }

            bytes = result;
            normalizedLowerHex = Convert.ToHexStringLower(result);
            return true;
        }

        private static ReadOnlySpan<char> StripPrefixSpan(ReadOnlySpan<char> input)
        {
            if (input.Length >= 2 && input[0] == '0' && (input[1] == 'x' || input[1] == 'X'))
                return input.Slice(2);

            if (input.Length >= 1 && input[0] == '#')
                return input.Slice(1);

            return input;
        }
#endif

#if !NET9_0_OR_GREATER
        private static bool TryDecodeLegacy(string input, out byte[]? bytes, out string? normalizedLowerHex, out string? errorMessage)
        {
            bytes = null;
            normalizedLowerHex = null;
            errorMessage = null;

            // Track leading whitespace for error position reporting
            int leadingWhitespace = 0;
            while (leadingWhitespace < input.Length && char.IsWhiteSpace(input[leadingWhitespace]))
            {
                leadingWhitespace++;
            }

            string trimmed = input.Trim();

            if (trimmed.Length == 0)
            {
                bytes = Array.Empty<byte>();
                normalizedLowerHex = string.Empty;
                return true;
            }

            // Strip prefix — track prefix length for error position reporting
            string hex = StripPrefixLegacy(trimmed);
            int positionOffset = leadingWhitespace + (trimmed.Length - hex.Length);

            // Strip separators and collect clean hex chars
            char[] clean = new char[hex.Length];
            int cleanLen = 0;

            for (int i = 0; i < hex.Length; i++)
            {
                char c = hex[i];
                if (c == ':' || c == '-' || c == ' ')
                    continue;

                if (!IsHexChar(c))
                {
                    errorMessage = $"Invalid hexadecimal character '{c}' (U+{(int)c:X4}) at position {positionOffset + i}.";
                    return false;
                }

                clean[cleanLen++] = c;
            }

            // Check even length
            if (cleanLen % 2 != 0)
            {
                errorMessage = $"Hexadecimal string has an odd number of characters ({cleanLen}). " +
                    "Hex strings must have an even number of characters to represent complete bytes.";
                return false;
            }

            // Decode
            int byteCount = cleanLen / 2;
            byte[] result = new byte[byteCount];

            for (int i = 0; i < byteCount; i++)
            {
                int hi = HexCharToNibble(clean[i * 2]);
                int lo = HexCharToNibble(clean[i * 2 + 1]);
                result[i] = (byte)((hi << 4) | lo);
            }

            bytes = result;

            // Generate normalized lowercase hex
            char[] lowerChars = new char[byteCount * 2];
            for (int i = 0; i < byteCount; i++)
            {
                int b = result[i];
                lowerChars[i * 2] = NibbleToLowerHexChar(b >> 4);
                lowerChars[i * 2 + 1] = NibbleToLowerHexChar(b & 0xF);
            }
            normalizedLowerHex = new string(lowerChars);

            return true;
        }

        private static string StripPrefixLegacy(string input)
        {
            if (input.Length >= 2 && input[0] == '0' && (input[1] == 'x' || input[1] == 'X'))
                return input.Substring(2);

            if (input.Length >= 1 && input[0] == '#')
                return input.Substring(1);

            return input;
        }

        private static char NibbleToLowerHexChar(int nibble)
        {
            return (char)(nibble < 10 ? '0' + nibble : 'a' + nibble - 10);
        }
#endif

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }

        private static int HexCharToNibble(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            return c - 'A' + 10; // Already validated as hex char
        }
    }
}
