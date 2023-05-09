namespace Gksyb.Core.Common
{
    /// <summary>
    /// 系统时间请求
    /// </summary>
    public class SysdateRequest
    {
        /// <summary>
        /// 日期格式化
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// 追加类型
        /// </summary>
        public string DateAddType { get; set; }

        /// <summary>
        /// 追加时间
        /// </summary>
        public double DateAdd { get; set; }
    }
}