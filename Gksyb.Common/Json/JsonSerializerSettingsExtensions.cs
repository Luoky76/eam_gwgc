using Gksyb.Common;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    public static class JsonSerializerSettingsExtensions
    {
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
            if (igronNull) source.NullValueHandling = NullValueHandling.Ignore;
            return source;
        }

        /// <summary>
        /// 去除小数的0
        /// </summary>
        private class MinifiedNumArrayConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value,
                JsonSerializer serializer)
            {
                var rawValue = $"{value:#0.#################}";
                if (rawValue.Length > 16) rawValue = $"\"{rawValue}\"";
                writer.WriteRawValue(rawValue);
            }

            private readonly Type dblType = typeof(double);
            private readonly Type decType = typeof(decimal);
            private readonly Type fltType = typeof(float);

            public override bool CanConvert(Type objectType)
            {
                var realType = objectType.GetUnNullableType();
                return (realType == decType || realType == fltType || realType == dblType);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override object ReadJson(JsonReader reader, Type objectType,
                object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// js长整型精度问题
        /// </summary>
        private class JsLongConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value,
                JsonSerializer serializer)
            {
                var rawValue = value.ToString();
                if (rawValue.Length > 16) rawValue = $"\"{rawValue}\"";
                writer.WriteRawValue(rawValue);
            }

            private readonly Type longType = typeof(long);

            public override bool CanConvert(Type objectType)
            {
                var realType = objectType.GetUnNullableType();
                return (realType == longType);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override object ReadJson(JsonReader reader, Type objectType,
                object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}