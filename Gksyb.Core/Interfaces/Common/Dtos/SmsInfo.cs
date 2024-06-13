namespace Gksyb.Core.Interfaces.Common
{
    /// <summary>
    /// 短信
    /// </summary>
    public class SmsInfo
    {
        /// <summary>
        /// 手机号码（多个手机号用英文,分隔）
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 短信模板编码
        /// </summary>
        public string TemplateCode { get; set; }

        /// <summary>
        /// 短信模板对应的参数
        /// </summary>
        public string TemplateParam { get; set; }
    }
}