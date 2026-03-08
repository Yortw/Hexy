using System;
using System.Runtime.CompilerServices;

namespace Yort.Hexy
{
    public readonly partial struct Hex
    {
        /// <summary>
        /// Determines whether this <see cref="Hex"/> is equal to another <see cref="Hex"/>
        /// by comparing byte sequences.
        /// </summary>
        /// <param name="other">The <see cref="Hex"/> to compare with.</param>
        /// <returns><see langword="true"/> if the byte sequences are identical; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Comparison is performed on the underlying bytes, so values created from different
        /// string formats (e.g., <c>"DEADBEEF"</c> and <c>"de:ad:be:ef"</c>) are equal if they
        /// represent the same byte sequence.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Hex other)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = other.BytesOrEmpty;

            if (a.Length != b.Length)
                return false;

            if (a.Length == 0)
                return true;

            return a.AsSpan().SequenceEqual(b.AsSpan());
        }

        /// <summary>
        /// Determines whether this <see cref="Hex"/> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with. Must be a <see cref="Hex"/> to return <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Hex"/> with an identical byte sequence; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Hex other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this <see cref="Hex"/> value, computed from the byte contents.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code. Two equal <see cref="Hex"/> values always produce the same hash code.</returns>
        /// <remarks>
        /// <para>On .NET 9+, uses <c>HashCode.AddBytes</c> for optimal distribution.
        /// On .NET Standard 2.0, uses an FNV-1a implementation.</para>
        /// </remarks>
        public override int GetHashCode()
        {
            byte[] bytes = BytesOrEmpty;

            if (bytes.Length == 0)
                return 0;

#if NET9_0_OR_GREATER
            var hash = new HashCode();
            hash.AddBytes(bytes.AsSpan());
            return hash.ToHashCode();
#else
            return Fnv1aHash(bytes);
#endif
        }

        /// <summary>
        /// Compares this <see cref="Hex"/> to another using lexicographic byte ordering (big-endian / left-to-right).
        /// </summary>
        /// <param name="other">The <see cref="Hex"/> to compare with.</param>
        /// <returns>
        /// A negative value if this instance precedes <paramref name="other"/>;
        /// zero if they are equal; a positive value if this instance follows <paramref name="other"/>.
        /// </returns>
        /// <remarks>
        /// <para>Shorter values are considered less than longer values when all compared bytes are equal.
        /// This is consistent with lexicographic ordering of byte sequences.</para>
        /// <para>Uses <see cref="MemoryExtensions.SequenceCompareTo{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
        /// which may benefit from SIMD acceleration on modern runtimes.</para>
        /// </remarks>
        public int CompareTo(Hex other)
        {
            byte[] a = BytesOrEmpty;
            byte[] b = other.BytesOrEmpty;

            return a.AsSpan().SequenceCompareTo(b.AsSpan());
        }

        /// <summary>
        /// Compares this <see cref="Hex"/> to another object.
        /// </summary>
        /// <param name="obj">The object to compare with. Must be <see langword="null"/> or a <see cref="Hex"/>.</param>
        /// <returns>
        /// A negative value if this instance precedes <paramref name="obj"/>;
        /// zero if they are equal; a positive value if this instance follows <paramref name="obj"/>.
        /// A non-null <see cref="Hex"/> always follows <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="Hex"/>.</exception>
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;
            if (obj is Hex other) return CompareTo(other);
            throw new ArgumentException($"Object must be of type {nameof(Hex)}.", nameof(obj));
        }

        /// <summary>Determines whether two <see cref="Hex"/> values are equal.</summary>
        public static bool operator ==(Hex left, Hex right) => left.Equals(right);

        /// <summary>Determines whether two <see cref="Hex"/> values are not equal.</summary>
        public static bool operator !=(Hex left, Hex right) => !left.Equals(right);

        /// <summary>Determines whether the left <see cref="Hex"/> is less than the right in lexicographic byte order.</summary>
        public static bool operator <(Hex left, Hex right) => left.CompareTo(right) < 0;

        /// <summary>Determines whether the left <see cref="Hex"/> is greater than the right in lexicographic byte order.</summary>
        public static bool operator >(Hex left, Hex right) => left.CompareTo(right) > 0;

        /// <summary>Determines whether the left <see cref="Hex"/> is less than or equal to the right in lexicographic byte order.</summary>
        public static bool operator <=(Hex left, Hex right) => left.CompareTo(right) <= 0;

        /// <summary>Determines whether the left <see cref="Hex"/> is greater than or equal to the right in lexicographic byte order.</summary>
        public static bool operator >=(Hex left, Hex right) => left.CompareTo(right) >= 0;

#if !NET9_0_OR_GREATER
        private static int Fnv1aHash(byte[] bytes)
        {
            unchecked
            {
                const int FnvOffsetBasis = unchecked((int)2166136261);
                const int FnvPrime = 16777619;

                int hash = FnvOffsetBasis;
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= FnvPrime;
                }
                return hash;
            }
        }
#endif
    }
}
