using Newtonsoft.Json;

namespace Gksyb.Common
{
    /// <summary>
    /// Json扩展方法
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings _miniSettings = new JsonSerializerSettings().Custom(igronNull: true);

        public static string ToMiniJson(this object source)
        {
            return JSONHelper.ToJson(source, _miniSettings);
        }

        public static string ToJson(this object source, JsonSerializerSettings settings = null)
        {
            return JSONHelper.ToJson(source, settings);
        }

        public static T ToObject<T>(this string source, JsonSerializerSettings settings = null)
        {
            return JSONHelper.FromJson<T>(source, settings);
        }

        public static T ToObject<T>(this string source, T anonymousTypeObject, JsonSerializerSettings settings = null)
        {
            return JSONHelper.DeserializeAnonymousType(source, anonymousTypeObject, settings);
        }

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
        /// <summary>
        /// 类对象转换成json格式
        /// </summary>
        /// <returns></returns>
        public static string ToJson(object value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        /// json格式转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T FromJson<T>(string value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(value, settings);
        }

        public static object FromJson(string value, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(value, type, settings);
        }

        public static dynamic FromJsonDynamic(string value, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<dynamic>(value, settings);
        }

        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeAnonymousType(value, anonymousTypeObject, settings);
        }
    }
}