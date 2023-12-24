using Gksyb.Core.Auth;
using Gksyb.Model.UI;
using Newtonsoft.Json;

namespace Gksyb.Model.Dtos
{
    public class LoginResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 设备唯一识别码
        /// </summary>
        public string IMEI { get; set; }

        /// <summary>
        /// 用户登录环境发生变化
        /// </summary>
        public bool IsChange { get; set; }

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