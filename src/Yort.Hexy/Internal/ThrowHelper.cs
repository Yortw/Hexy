using System;
using System.Diagnostics.CodeAnalysis;

namespace Yort.Hexy.Internal
{
    /// <summary>
    /// Centralized exception throwing to keep hot paths free of exception-construction overhead.
    /// Methods are marked NoInlining so the throw site doesn't bloat calling methods.
    /// </summary>
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException(string paramName, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [DoesNotReturn]
        internal static void ThrowFormatException(string message)
        {
            throw new FormatException(message);
        }

        [DoesNotReturn]
        internal static void ThrowFormatExceptionInvalidChar(char c, int position)
        {
            throw new FormatException(
                $"Invalid hexadecimal character '{c}' (U+{(int)c:X4}) at position {position}.");
        }

        [DoesNotReturn]
        internal static void ThrowFormatExceptionOddLength(int length)
        {
            throw new FormatException(
                $"Hexadecimal string has an odd number of characters ({length}). " +
                "Hex strings must have an even number of characters to represent complete bytes.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        internal static void ThrowInvalidCastException(Type sourceType, Type targetType)
        {
            throw new InvalidCastException(
                $"Cannot convert from {sourceType.FullName} to {targetType.FullName}.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException(string message, string paramName)
        {
            throw new ArgumentException(message, paramName);
        }
    }
}
