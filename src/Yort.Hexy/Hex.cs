using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    /// <summary>
    /// Represents an immutable sequence of bytes with first-class support for hexadecimal
    /// string formatting, parsing, comparison, and interop.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Hex"/> is a <see langword="readonly struct"/> backed by an internal
    /// <see cref="byte"/> array. The array is never exposed directly; all public APIs return
    /// copies or read-only views to preserve immutability.</para>
    ///
    /// <para>A <c>default(Hex)</c> value is treated as an empty (zero-length) hex value,
    /// equivalent to <see cref="Empty"/>.</para>
    ///
    /// <para>When constructed via <see cref="Parse(string)"/> or <see cref="TryParse(string, out Hex)"/>,
    /// the normalized lowercase hex string is cached internally. Subsequent calls to
    /// <see cref="ToString()"/> with the default <see cref="HexFormat.Lowercase"/> format
    /// return the cached string with zero allocation.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Parse from string
    /// Hex value = Hex.Parse("0xDEADBEEF");
    ///
    /// // Create from bytes
    /// Hex fromBytes = new Hex(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
    ///
    /// // Use as dictionary key
    /// var dict = new Dictionary&lt;Hex, string&gt; { [value] = "found it" };
    ///
    /// // Format with different styles
    /// Console.WriteLine(value.ToString(HexFormat.UppercaseColons)); // "DE:AD:BE:EF"
    /// </code>
    /// </example>
    /// <seealso cref="HexFormat"/>
    /// <seealso cref="HexDefaults"/>
    /// <seealso cref="HexBuilder"/>
    [DebuggerDisplay("Hex[{Length}] \"{DebuggerDisplayValue}\"")]
    [TypeConverter(typeof(HexTypeConverter))]
    [JsonConverter(typeof(HexJsonConverter))]
    public readonly partial struct Hex : IEquatable<Hex>, IComparable<Hex>, IComparable, IFormattable, IConvertible
#if NET9_0_OR_GREATER
        , ISpanFormattable, IUtf8SpanFormattable, IParsable<Hex>, ISpanParsable<Hex>
#endif
    {
        private readonly byte[]? _bytes;
        private readonly string? _cachedLowerHex;

        /// <summary>
        /// A zero-length <see cref="Hex"/> value. Equivalent to <c>default(Hex)</c> but backed
        /// by <see cref="Array.Empty{T}"/> to avoid null-internal-array edge cases.
        /// </summary>
        /// <remarks>
        /// <para>This field follows the same pattern as <see cref="string.Empty"/>.</para>
        /// </remarks>
        public static readonly Hex Empty = new Hex(Array.Empty<byte>(), string.Empty);

        /// <summary>
        /// Initializes a new <see cref="Hex"/> from a byte array. The input is copied;
        /// subsequent modifications to <paramref name="bytes"/> do not affect this instance.
        /// </summary>
        /// <param name="bytes">The bytes to store. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
        /// <example>
        /// <code>
        /// byte[] data = { 0xCA, 0xFE };
        /// Hex hex = new Hex(data);
        /// Console.WriteLine(hex); // "cafe"
        /// </code>
        /// </example>
        /// <seealso cref="Parse(string)"/>
        /// <seealso cref="CreateRandom(int)"/>
        public Hex(byte[] bytes)
        {
            if (bytes == null) ThrowHelper.ThrowArgumentNullException(nameof(bytes));
            _bytes = bytes.Length == 0 ? Array.Empty<byte>() : (byte[])bytes.Clone();
            _cachedLowerHex = null;
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Initializes a new <see cref="Hex"/> from a read-only span of bytes. The input is copied.
        /// </summary>
        /// <param name="bytes">The bytes to store.</param>
        /// <example>
        /// <code>
        /// ReadOnlySpan&lt;byte&gt; data = stackalloc byte[] { 0xCA, 0xFE };
        /// Hex hex = new Hex(data);
        /// Console.WriteLine(hex); // "cafe"
        /// </code>
        /// </example>
        public Hex(ReadOnlySpan<byte> bytes)
        {
            _bytes = bytes.Length == 0 ? Array.Empty<byte>() : bytes.ToArray();
            _cachedLowerHex = null;
        }
#endif

        /// <summary>
        /// Internal constructor used by the parse path to store both bytes and the cached normalized hex string.
        /// </summary>
        internal Hex(byte[] bytes, string? cachedLowerHex)
        {
            _bytes = bytes;
            _cachedLowerHex = cachedLowerHex;
        }

        /// <summary>
        /// Gets the internal byte array, never returning <see langword="null"/>.
        /// </summary>
        internal byte[] BytesOrEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bytes ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the number of bytes in this hex value.
        /// </summary>
        /// <value>The byte count. Returns <c>0</c> for <c>default(Hex)</c> and <see cref="Empty"/>.</value>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BytesOrEmpty.Length;
        }

        /// <summary>
        /// Gets a value indicating whether this hex value contains zero bytes.
        /// </summary>
        /// <value><see langword="true"/> if <see cref="Length"/> is <c>0</c>; otherwise, <see langword="false"/>.</value>
        /// <seealso cref="Empty"/>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BytesOrEmpty.Length == 0;
        }

        /// <summary>
        /// Gets the byte at the specified index.
        /// </summary>
        /// <param name="index">The zero-based byte index.</param>
        /// <returns>The byte at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Length"/>.</exception>
        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BytesOrEmpty[index];
        }

        /// <summary>
        /// Creates a new <see cref="Hex"/> containing cryptographically random bytes.
        /// </summary>
        /// <param name="byteCount">The number of random bytes to generate. Must be non-negative.</param>
        /// <returns>A new <see cref="Hex"/> containing <paramref name="byteCount"/> random bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="byteCount"/> is negative.</exception>
        /// <remarks>
        /// <para>Uses <see cref="RandomNumberGenerator"/> for cryptographic-quality randomness.
        /// Suitable for generating nonces, correlation IDs, tokens, and test data.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Hex nonce = Hex.CreateRandom(16); // 16 bytes = 32 hex chars
        /// Console.WriteLine(nonce);          // e.g., "a1b2c3d4e5f60718293a4b5c6d7e8f90"
        /// </code>
        /// </example>
        /// <seealso cref="Hex(byte[])"/>
        public static Hex CreateRandom(int byteCount)
        {
            if (byteCount < 0) ThrowHelper.ThrowArgumentOutOfRangeException(nameof(byteCount), "Byte count must be non-negative.");
            if (byteCount == 0) return Empty;

            byte[] bytes = new byte[byteCount];
#if NET9_0_OR_GREATER
            RandomNumberGenerator.Fill(bytes);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
#endif
            return new Hex(bytes, null);
        }

        /// <summary>
        /// Returns the internally cached normalized lowercase hex string, if available.
        /// Used for ToString() optimization when the default format matches.
        /// </summary>
        internal string? CachedLowerHex => _cachedLowerHex;

        // Used by DebuggerDisplay — only format first 32 bytes to avoid large allocations
        private string DebuggerDisplayValue
        {
            get
            {
                byte[] bytes = BytesOrEmpty;
                if (bytes.Length == 0)
                {
                    return string.Empty;
                }

                if (bytes.Length <= 32)
                {
                    return HexEncoder.Encode(bytes, HexLetterCase.Lower);
                }

                byte[] preview = new byte[32];
                Array.Copy(bytes, 0, preview, 0, 32);
                return HexEncoder.Encode(preview, HexLetterCase.Lower) + "…";
            }
        }
    }
}
