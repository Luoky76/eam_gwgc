using Newtonsoft.Json;

namespace Gksyb.Core.Interfaces.Auth.Dtos
{
    public class ApiUserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("UI")]
        public long UserID { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [JsonProperty("UN")]
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonProperty("RN")]
        public string RealName { get; set; }

        /// <summary>
        /// 班组
        /// </summary>
        [JsonProperty("C")]
        public string Class { get; set; }

        /// <summary>
        /// 工号 单点以此为凭据
        /// </summary>
        [JsonProperty("WC")]
        public string WorkerCode { get; set; }
    }
}
