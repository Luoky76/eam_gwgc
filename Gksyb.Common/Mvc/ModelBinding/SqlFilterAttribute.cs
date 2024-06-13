using Gksyb.Common;
using Gksyb.Common.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 防sql注入
    /// </summary>
    public class SqlFilterAttribute : ParameterHandleAttribute
    {
        private readonly int? _limit;

        /// <summary>
        /// 防sql注入
        /// </summary>
        public SqlFilterAttribute()
        {
        }

        /// <summary>
        /// 防sql注入
        /// </summary>
        public SqlFilterAttribute(int limit)
        {
            _limit = limit < 0 ? null : limit;
        }

        /// <summary>
        /// 请慎用（可能会出安全性问题），跳过SqlFilter处理校验
        /// </summary>
        public bool Skip { get; set; }

        public override int GetOrder() => int.MaxValue;

        public override object Handle(object value)
        {
            if (Skip) return value;
            if (value == null) return value;
            if (value is not string sValue) return value;
            return sValue.SqlFilter(_limit);
        }
    }
}