using Gksyb.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    public static class JsonSerializerSettingsExtensions
    {
        private static bool IgnoreNull = false;

        /// <summary>
        /// 初始化json配置，应该在所有调用之前
        /// </summary>
        public static void Init(IConfiguration configuration)
        {
            IgnoreNull = configuration.GetValue($"{OptionName.SysContext}:IgnoreJsonNull", defaultValue: false);
        }

        /// <summary>
        /// 定制化json
        /// </summary>
        public static JsonSerializerSettings Custom(this JsonSerializerSettings source, string dateFormatString = null, bool igronNull = false)
        {
            if (string.IsNullOrWhiteSpace(dateFormatString))
            {
                dateFormatString = "yyyy-MM-dd HH:mm:ss";
            }
            source.DateFormatString = dateFormatString;
            source.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            source.ContractResolver = new DefaultContractResolver();
            source.Converters.Add(new MinifiedNumArrayConverter());
            source.Converters.Add(new JsLongConverter());
            if (igronNull || IgnoreNull) source.NullValueHandling = NullValueHandling.Ignore;
            return source;
        }

        /// <summary>
        /// 去除小数的0
        /// </summary>
        private class MinifiedNumArrayConverter : JsonConverter
        {
            private static readonly Type dblType = typeof(double);
            private static readonly Type decType = typeof(decimal);
            private static readonly Type fltType = typeof(float);

            public override bool CanConvert(Type objectType)
            {
                var realType = objectType.GetUnNullableType();
                return realType == decType || realType == fltType || realType == dblType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return reader.Value.CastTo(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var rawValue = $"{value:#0.#################}";
                if (rawValue.Length > 16) rawValue = $"\"{rawValue}\"";
                writer.WriteRawValue(rawValue);
            }
        }

        /// <summary>
        /// js长整型精度问题
        /// </summary>
        private class JsLongConverter : JsonConverter
        {
            private static readonly Type longType = typeof(long);

            public override bool CanConvert(Type objectType)
            {
                var realType = objectType.GetUnNullableType();
                return realType == longType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return reader.Value.CastTo(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var rawValue = value.ToString();
                if (rawValue.Length > 16) rawValue = $"\"{rawValue}\"";
                writer.WriteRawValue(rawValue);
            }
        }
    }
}