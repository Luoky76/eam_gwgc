namespace Gksyb.Server.Controllers.Auth.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string UA { get; set; }
    }
}