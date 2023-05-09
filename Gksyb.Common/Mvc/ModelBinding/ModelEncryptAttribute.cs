using Gksyb.Common;
using Gksyb.Common.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 前台加密
    /// </summary>
    public class ModelEncryptAttribute : ParameterHandleAttribute
    {
        /// <summary>
        /// 前台加密
        /// </summary>
        public ModelEncryptAttribute()
        {
        }

        public override int GetOrder() => 10;

        public override object Handle(object value)
        {
            if (value == null) return value;
            if (value is not string sValue) return value;
            sValue = CryptographyHelper.DecryptFront(sValue);
            return sValue;
        }
    }
}