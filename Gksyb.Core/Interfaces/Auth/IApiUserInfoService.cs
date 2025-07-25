using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Http;

namespace Gksyb.Core.Interfaces.Auth
{
    /// <summary>
    /// Api用户附加信息处理
    /// </summary>
    public interface IApiUserInfoService : IService
    {
        /// <summary>
        /// 附加信息的header头
        /// </summary>
        public const string HeaderKey = "Api-Extend-Info";

        /// <summary>
        /// 根据header获取的附加信息，修改当前的用户信息
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="user">当前用户信息</param>
        Task FromRequestAsync(HttpRequest request, UserSession user);
    }
}
