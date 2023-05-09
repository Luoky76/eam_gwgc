using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// 定制化json
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dateFormatString"></param>
        /// <returns></returns>
        public static JsonSerializerOptions Custom(this JsonSerializerOptions source, string dateFormatString = null)
        {
            source.AllowTrailingCommas = true;//忽略多余逗号
            source.PropertyNameCaseInsensitive = true;//反序列化 不区分大小写
            source.PropertyNamingPolicy = null;//保持属性名不变
            source.ReadCommentHandling = JsonCommentHandling.Skip;//忽略注释
            source.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;//javascript不严格
            source.Converters.Add(new DateTimeConverter(dateFormatString));
            source.Converters.Add(new ShortConverter());
            source.Converters.Add(new UShortConverter());
            source.Converters.Add(new IntConverter());
            source.Converters.Add(new UIntConverter());
            source.Converters.Add(new LongConverter());
            source.Converters.Add(new ULongConverter());
            source.Converters.Add(new DoubleConverter());
            source.Converters.Add(new FloatConverter());
            source.Converters.Add(new DecimalConverter());
            return source;
        }

        /// <summary>
        /// 日期格式
        /// </summary>
        private class DateTimeConverter : JsonConverter<DateTime>
        {
            private readonly string _dateFormatString;

            public DateTimeConverter(string dateFormatString = null)
            {
                if (string.IsNullOrWhiteSpace(dateFormatString))
                {
                    dateFormatString = "yyyy-MM-dd HH:mm:ss";
                }
                _dateFormatString = dateFormatString;
            }

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (DateTime.TryParse(reader.GetString(), out DateTime result))
                    {
                        return result;
                    }
                }
                return reader.GetDateTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(_dateFormatString));
            }
        }

        #region 数字

        private class ShortConverter : JsonConverter<short>
        {
            public override short Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (short.TryParse(reader.GetString(), out short result))
                    {
                        return result;
                    }
                }
                return reader.GetInt16();
            }

            public override void Write(Utf8JsonWriter writer, short value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class UShortConverter : JsonConverter<ushort>
        {
            public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (ushort.TryParse(reader.GetString(), out ushort result))
                    {
                        return result;
                    }
                }
                return reader.GetUInt16();
            }

            public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class IntConverter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (int.TryParse(reader.GetString(), out int result))
                    {
                        return result;
                    }
                }
                return reader.GetInt32();
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class UIntConverter : JsonConverter<uint>
        {
            public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (uint.TryParse(reader.GetString(), out uint result))
                    {
                        return result;
                    }
                }
                return reader.GetUInt32();
            }

            public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class LongConverter : JsonConverter<long>
        {
            public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (long.TryParse(reader.GetString(), out long result))
                    {
                        return result;
                    }
                }
                return reader.GetInt64();
            }

            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class ULongConverter : JsonConverter<ulong>
        {
            public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (ulong.TryParse(reader.GetString(), out ulong result))
                    {
                        return result;
                    }
                }
                return reader.GetUInt64();
            }

            public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class DoubleConverter : JsonConverter<double>
        {
            public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (double.TryParse(reader.GetString(), out double result))
                    {
                        return result;
                    }
                }
                return reader.GetDouble();
            }

            public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class FloatConverter : JsonConverter<float>
        {
            public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (float.TryParse(reader.GetString(), out float result))
                    {
                        return result;
                    }
                }
                return reader.GetSingle();
            }

            public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        private class DecimalConverter : JsonConverter<decimal>
        {
            public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (decimal.TryParse(reader.GetString(), out decimal result))
                    {
                        return result;
                    }
                }
                return reader.GetDecimal();
            }

            public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        #endregion 数字
    }
}