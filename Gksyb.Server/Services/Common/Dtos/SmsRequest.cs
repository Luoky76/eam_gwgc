using Gksyb.Core.Interfaces.Common;

namespace Gksyb.Server.Services.Common.Dtos
{
    public class SmsRequest : SmsInfo
    {
        /// <summary>
        /// 公司
        /// </summary>
        public string Corp { get; set; }

        /// <summary>
        /// 时戳 格式为：2022-05-06 12:48:50
        /// </summary>
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string UA { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 签名 Phone + TemplateCode + TemplateParam + TimeStamp(格式为：2022-05-06 12:48:50) + 密钥 做MD5 编码为UTF-8
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 计算签名
        /// </summary>
        public void ComputeSign(string secret)
        {
            TimeStamp = DateTime.Now;
            Sign = CryptographyHelper.GetMd5($"{Phone}{TemplateCode}{TemplateParam}{TimeStamp.Value:yyyy-MM-dd HH:mm:ss}{secret}");
        }
    }
}