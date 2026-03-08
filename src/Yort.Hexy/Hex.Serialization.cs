using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Yort.Hexy.Internal;

namespace Yort.Hexy
{
    // -----------------------------------------------------------------------
    // IConvertible implementation
    // -----------------------------------------------------------------------

    public readonly partial struct Hex
    {
        /// <inheritdoc/>
        TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

        /// <inheritdoc/>
        string IConvertible.ToString(IFormatProvider? provider) => ToString();

        /// <summary>
        /// Converts this <see cref="Hex"/> to the specified type via <see cref="IConvertible"/>.
        /// Supports conversion to <see cref="string"/> and <c>byte[]</c>.
        /// </summary>
        /// <param name="conversionType">The type to convert to.</param>
        /// <param name="provider">Ignored.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">
        /// <paramref name="conversionType"/> is not <see cref="string"/> or <c>byte[]</c>.
        /// </exception>
        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            if (conversionType == typeof(Hex))
                return this;

            if (conversionType == typeof(string))
                return ToString();

            if (conversionType == typeof(byte[]))
                return ToByteArray();

            ThrowHelper.ThrowInvalidCastException(typeof(Hex), conversionType);
            return null!; // unreachable
        }

        // All other IConvertible methods throw
        bool IConvertible.ToBoolean(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Boolean.");
        byte IConvertible.ToByte(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Byte.");
        char IConvertible.ToChar(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Char.");
        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to DateTime.");
        decimal IConvertible.ToDecimal(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Decimal.");
        double IConvertible.ToDouble(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Double.");
        short IConvertible.ToInt16(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Int16.");
        int IConvertible.ToInt32(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Int32.");
        long IConvertible.ToInt64(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Int64.");
        sbyte IConvertible.ToSByte(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to SByte.");
        float IConvertible.ToSingle(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to Single.");
        ushort IConvertible.ToUInt16(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to UInt16.");
        uint IConvertible.ToUInt32(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to UInt32.");
        ulong IConvertible.ToUInt64(IFormatProvider? provider) => throw new InvalidCastException($"Cannot convert {nameof(Hex)} to UInt64.");
    }

    // -----------------------------------------------------------------------
    // System.Text.Json converter
    // -----------------------------------------------------------------------

    /// <summary>
    /// JSON converter for <see cref="Hex"/> that serializes as a hex string and deserializes
    /// using permissive parsing.
    /// </summary>
    /// <remarks>
    /// <para>This converter is automatically applied via <see cref="JsonConverterAttribute"/>
    /// on the <see cref="Hex"/> type. It always serializes using canonical lowercase hex
    /// (no prefix, no separators) to guarantee round-trip fidelity, regardless of
    /// <see cref="HexDefaults.Format"/>. Deserialization uses <see cref="Hex.TryParse(string, out Hex)"/>
    /// and accepts all supported input formats.</para>
    /// <para>A JSON <c>null</c> value deserializes to <see cref="Hex.Empty"/> rather than throwing,
    /// because <see cref="Hex"/> is a value type and cannot be <see langword="null"/>.</para>
    /// </remarks>
    public sealed class HexJsonConverter : JsonConverter<Hex>
    {
        /// <inheritdoc/>
        public override Hex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return Hex.Empty;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a JSON string for Hex, but got {reader.TokenType}.");

            string? value = reader.GetString();
            if (value == null)
                return Hex.Empty;

            if (!Hex.TryParse(value, out Hex result))
                throw new JsonException($"Unable to parse \"{value}\" as a hexadecimal value.");

            return result;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Hex value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(HexFormat.Lowercase));
        }
    }

    // -----------------------------------------------------------------------
    // TypeConverter for designer / model-binding support
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provides type conversion support for <see cref="Hex"/>, enabling conversion to/from
    /// <see cref="string"/> and <c>byte[]</c> in designers, model binding, and other
    /// frameworks that use <see cref="TypeConverter"/>.
    /// </summary>
    public sealed class HexTypeConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(byte[]) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || destinationType == typeof(byte[]) || base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc/>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
                return Hex.Parse(s);

            if (value is byte[] bytes)
                return new Hex(bytes);

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is Hex hex)
            {
                if (destinationType == typeof(string))
                    return hex.ToString();

                if (destinationType == typeof(byte[]))
                    return hex.ToByteArray();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
