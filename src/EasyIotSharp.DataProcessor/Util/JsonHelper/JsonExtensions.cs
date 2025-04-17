using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyIotSharp.DataProcessor.Util
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Converters =
            {
                new StringToByteConverter(),
                new StringToUshortConverter(),
                new StringToIntConverter(),
                new StringToDoubleConverter(),
                new StringToBoolConverter()
            }
        };

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        public static string ToJson<T>(this T obj, JsonSerializerOptions options = null)
        {
            if (obj == null) return null;
            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        public static T FromJson<T>(this string json, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        /// <summary>
        /// 尝试将JSON字符串反序列化为对象
        /// </summary>
        public static bool TryFromJson<T>(this string json, out T result, JsonSerializerOptions options = null)
        {
            result = default;
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class StringToByteConverter : JsonConverter<byte>
    {
        public override byte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (byte.TryParse(stringValue, out byte value))
                {
                    return value;
                }
            }
            return reader.GetByte();
        }

        public override void Write(Utf8JsonWriter writer, byte value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class StringToUshortConverter : JsonConverter<ushort>
    {
        public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (ushort.TryParse(stringValue, out ushort value))
                {
                    return value;
                }
            }
            return reader.GetUInt16();
        }

        public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int value))
                {
                    return value;
                }
            }
            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class StringToDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (double.TryParse(stringValue, out double value))
                {
                    return value;
                }
            }
            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class StringToBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (bool.TryParse(stringValue, out bool value))
                {
                    return value;
                }
            }
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}