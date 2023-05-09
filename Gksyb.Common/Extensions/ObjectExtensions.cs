using Mapster;

namespace Gksyb.Common
{
    /// <summary>
    /// 类型<see cref="Object"/>辅助扩展方法类
    /// </summary>
    public static class ObjectExtensions
    {
        private static readonly string _keys = "0123456789abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ";//编码,可加一些字符也可以实现72,96等任意进制转换，但是有符号数据不直观，会影响阅读。
        private static readonly int _exponent = _keys.Length;//幂数

        /// <summary>
        /// 10进制=》62进制
        /// </summary>
        /// <returns></returns>
        public static string ToBase62(this long source)
        {
            return ((decimal)source).ToBase62();
        }

        /// <summary>
        /// 10进制=》62进制
        /// </summary>
        /// <returns></returns>
        public static string ToBase62(this decimal source)
        {
            var sb = new StringBuilder();
            do
            {
                var index = (int)(source % _exponent);
                sb.Insert(0, _keys[index]);
                source = (source - index) / _exponent;
            }
            while (source > 0);
            return sb.ToString();
        }

        /// <summary>
        /// 62进制=》10进制
        /// </summary>
        /// <returns></returns>
        public static decimal FormBase62(this string source)
        {
            decimal result = 0;
            for (int i = 0; i < source.Length; i++)
            {
                int x = source.Length - i - 1;
                result += _keys.IndexOf(source[i]) * Pow(_exponent, x);// Math.Pow(exponent, x);
            }
            return result;
        }

        /// <summary>
        /// N次方
        /// </summary>
        /// <returns></returns>
        private static decimal Pow(decimal x, decimal y)
        {
            decimal value = 1;
            while (y > 0)
            {
                value *= x;
                y--;
            }
            return value;
        }

        /// <summary>
        /// 把对象类型转换为指定类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object CastTo(this object value, Type conversionType)
        {
            var nullType = Nullable.GetUnderlyingType(conversionType);
            var nullable = (nullType != null) || (conversionType == typeof(string));
            if (value == null || Convert.IsDBNull(value))
            {
                if (!nullable) throw new MessageException("空对象");
                return null;
            }
            var valueType = value.GetType();
            if (valueType == conversionType || valueType == nullType) return value;
            conversionType = nullable && nullType != null ? nullType : conversionType;
            if (conversionType.IsEnum)
            {
                if (Enum.TryParse(conversionType, value.ToString(), true, out object enumValue))
                {
                    return enumValue;
                }
                else
                {
                    return Enum.ToObject(conversionType, value);
                }
            }
            if (conversionType == typeof(Guid))
            {
                return Guid.Parse(value.ToString());
            }
            if (conversionType == typeof(bool))
            {
                var tempBool = (value ?? "").ToString().Trim().ToLower();
                if (tempBool == "" || tempBool == "false" || tempBool == "0")
                {
                    return false;
                }
                return true;
            }
            try
            {
                return Convert.ChangeType(value, conversionType);
            }
            catch (Exception)
            {
                try
                {
                    return Convert.ChangeType(value.ToString(), conversionType);
                }
                catch (Exception)
                {
                    if (nullable) return null;
                    throw;
                }
            }
        }

        /// <summary>
        /// 把对象类型转化为指定类型
        /// </summary>
        /// <typeparam name="T"> 动态类型 </typeparam>
        /// <param name="value"> 要转化的源对象 </param>
        /// <returns> 转化后的指定类型的对象，转化失败引发异常。 </returns>
        public static T CastTo<T>(this object value)
        {
            return (T)CastTo(value, typeof(T));
        }

        /// <summary>
        /// 把对象类型转化为指定类型，转化失败时返回指定的默认值
        /// </summary>
        /// <typeparam name="T"> 动态类型 </typeparam>
        /// <param name="value"> 要转化的源对象 </param>
        /// <param name="defaultValue"> 转化失败返回的指定默认值 </param>
        /// <returns> 转化后的指定类型对象，转化失败时返回指定的默认值 </returns>
        public static T CastTo<T>(this object value, T defaultValue)
        {
            try
            {
                return CastTo<T>(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 匿名类型转List匿名类型
        /// </summary>
        /// <returns></returns>
        public static List<T> ToListType<T>(this T anonymousTypeObject)
        {
            return Enumerable.Repeat(anonymousTypeObject, 1).ToList();
        }

        /// <summary>
        /// 转成忽略大小写的字典
        /// </summary>
        public static Dictionary<string, T> ToIgnoreCaseDictionary<T>(this Dictionary<string, T> source)
        {
            if (source == null) return null;
            var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            source.Keys.ForEach(key =>
            {
                result.Add(key, source[key]);
            });
            return result;
        }

        /// <summary>
        /// 映射
        /// </summary>
        /// <returns></returns>
        public static TDestination MapTo<TDestination>(this object source)
        {
            return source.Adapt<TDestination>();
        }

        /// <summary>
        /// 映射
        /// </summary>
        /// <returns></returns>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return source.Adapt(destination);
        }
    }
}