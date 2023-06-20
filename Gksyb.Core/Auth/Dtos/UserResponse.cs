namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public partial class UserResponse
    {
        /// <summary>
        /// 票据
        /// </summary>
        public string Ticket { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 微信Openid
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 是否本公司
        /// </summary>
        public bool IsOurCompany { get; set; }
    }
}