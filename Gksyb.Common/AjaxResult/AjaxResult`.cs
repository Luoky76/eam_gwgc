using Newtonsoft.Json;

namespace Gksyb.Common
{
    /// <summary>
    /// 前台Ajax请求的统一返回结果类
    /// </summary>
    public class AjaxResult<T> : AjaxResult
    {
        [JsonProperty(Order = 40)]
        public new T Data
        {
            get { return (T)base.Data; }
            set { base.Data = value; }
        }

        public static new AjaxResult<T> Error(string message = default)
        {
            return new AjaxResult<T>()
            {
                IsError = true,
                Message = (message ?? "").Replace("ORA", "错误码")
            };
        }

        public static new AjaxResult<T> Error(string message, object data)
        {
            return new AjaxResult<T>()
            {
                IsError = true,
                Message = (message ?? "").Replace("ORA", "错误码"),
                Data = (T)data
            };
        }

        public static new AjaxResult<T> Success(string message)
        {
            return new AjaxResult<T>()
            {
                IsError = false,
                Message = message
            };
        }

        public static new AjaxResult<T> Success(object data = default, string message = "成功")
        {
            return new AjaxResult<T>()
            {
                IsError = false,
                Data = (T)data,
                Message = message
            };
        }
    }
}