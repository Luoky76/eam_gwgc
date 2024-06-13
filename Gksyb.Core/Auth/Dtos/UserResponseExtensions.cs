using Gksyb.Core.Interfaces.Auth;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 用户信息扩展
    /// </summary>
    public partial class UserResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public CorpInfo Corp { get; set; }
    }
}