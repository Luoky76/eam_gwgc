using Newtonsoft.Json;

namespace Gksyb.Common
{
    /// <summary>
    /// Json扩展方法
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings _miniSettings = new JsonSerializerSettings().Custom(igronNull: true);

        /// <summary>
        /// 对象转最小化json（忽略空对象）
        /// </summary>
        public static string ToMiniJson(this object source)
        {
            return JSONHelper.ToJson(source, _miniSettings);
        }

        /// <summary>
        /// 对象转json
        /// </summary>
        public static string ToJson(this object source, JsonSerializerSettings settings = null)
        {
            return JSONHelper.ToJson(source, settings);
        }

        /// <summary>
        /// json转对象
        /// </summary>
        public static T ToObject<T>(this string source, JsonSerializerSettings settings = null)
        {
            return JSONHelper.FromJson<T>(source, settings);
        }

        /// <summary>
        /// json转对象（支持匿名类型）
        /// </summary>
        public static T ToObject<T>(this string source, T anonymousTypeObject, JsonSerializerSettings settings = null)
        {
            return JSONHelper.DeserializeAnonymousType(source, anonymousTypeObject, settings);
        }

        /// <summary>
        /// json转List对象（支持匿名类型）
        /// </summary>
        public static List<T> ToObjectList<T>(this string source, T anonymousTypeObject, JsonSerializerSettings settings = null)
        {
            return JSONHelper.DeserializeAnonymousType(source, anonymousTypeObject.ToListType(), settings);
        }
    }

    /// <summary>
    /// JSON帮助类
    /// </summary>
    public static class JSONHelper
    {
        private static readonly JsonSerializerSettings _customSettings;

        static JSONHelper()
        {
            _customSettings = new JsonSerializerSettings().Custom();
        }

        /// <summary>
        /// 对象转json
        /// </summary>
        public static string ToJson(object value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(value, settings ?? _customSettings);
        }

        /// <summary>
        /// json转对象
        /// </summary>
        public static T FromJson<T>(string value, JsonSerializerSettings settings = null)
        {
            if (value == null) return default;
            return JsonConvert.DeserializeObject<T>(value, settings ?? _customSettings);
        }

        public static object FromJson(string value, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(value, type, settings ?? _customSettings);
        }

        public static dynamic FromJsonDynamic(string value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<dynamic>(value, settings ?? _customSettings);
        }

        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeAnonymousType(value, anonymousTypeObject, settings ?? _customSettings);
        }
    }
}