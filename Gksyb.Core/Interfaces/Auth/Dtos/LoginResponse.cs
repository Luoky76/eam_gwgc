using Gksyb.Core.Auth;
using Gksyb.Model.UI;
using Newtonsoft.Json;

namespace Gksyb.Model.Dtos
{
    public class LoginResponse
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 上次设备唯一识别码
        /// </summary>
        public string LastIMEI { get; set; }

        /// <summary>
        /// 设备唯一识别码
        /// </summary>
        public string IMEI { get; set; }

        /// <summary>
        /// 是否启用二次验证
        /// </summary>
        public bool IsAuth { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public List<KeyValueItem> Phone { get; set; }

        /// <summary>
        /// 返回前台的用户信息
        /// </summary>
        public UserResponse Response { get; set; }

        /// <summary>
        /// 当前存储的用户信息
        /// </summary>
        [JsonIgnore]
        public UserSession Session { get; set; }
    }
}