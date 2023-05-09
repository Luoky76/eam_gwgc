namespace Gksyb.Common
{
    /// <summary>
    /// 用于参数检查的扩展方法
    /// </summary>
    public static class ParamterCheckExtensions
    {
        /// <summary>
        /// 检查参数不能为空引用,字符串不能为空引用和空字符串，否则抛出<see cref="ArgumentNullException"/>异常。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="paramName">参数名称</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CheckNotNull(this object value, string paramName)
        {
            if (value != null)
            {
                if (value is string && string.IsNullOrWhiteSpace((value as string))) throw new MessageException(string.Format("“{0}”不能为空。", paramName));
                return;
            }
            throw new MessageException(string.Format("“{0}”不能为空。", paramName));
        }

        /// <summary>
        /// 检查字符串不能为空引用或空字符串，否则抛出<see cref="ArgumentNullException"/>异常。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="paramName">参数名称。</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void CheckNotNullOrWhiteSpace(this string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new MessageException(string.Format("“{0}”不能为空。", paramName));
            }
        }
    }
}