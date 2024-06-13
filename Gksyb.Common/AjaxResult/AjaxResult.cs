using Newtonsoft.Json;

namespace Gksyb.Common
{
    /// <summary>
    /// 前台Ajax请求的统一返回结果类
    /// </summary>
    public class AjaxResult
    {
        /// <summary>
        /// 是否产生错误
        /// </summary>
        [JsonProperty(Order = 10)]
        public bool IsError { get; set; }

        /// <summary>
        /// 错误信息，或者成功信息
        /// </summary>
        [JsonProperty(Order = 20)]
        public string Message { get; set; }

        /// <summary>
        /// 成功可能时返回的数据
        /// </summary>
        [JsonProperty(Order = 30)]
        public object Data { get; set; }

        public static AjaxResult Error(string message = default)
        {
            return new AjaxResult()
            {
                IsError = true,
                Message = (message ?? "").Replace("ORA", "错误码")
            };
        }

        public static AjaxResult Error(string message, object data)
        {
            return new AjaxResult()
            {
                IsError = true,
                Message = (message ?? "").Replace("ORA", "错误码"),
                Data = data
            };
        }

        public static AjaxResult Success(string message)
        {
            return new AjaxResult()
            {
                IsError = false,
                Message = message
            };
        }

        public static AjaxResult Success(object data = default, string message = "成功")
        {
            return new AjaxResult()
            {
                IsError = false,
                Data = data,
                Message = message
            };
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}