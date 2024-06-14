namespace Gksyb.Server.Controllers.Auth.Dtos
{
    public class AccessTokenResponse
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 接口调用凭证超时时间，单位（秒）
        /// </summary>
        public long ExpiresIn { get; set; }
    }
}