using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;

namespace Gksyb.Core.Interfaces.Auth
{
    public interface IAuthService : IService
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> LoginAsync(LoginRequest request, Action<UserSession> action = null, bool checkPassword = true);

        /// <summary>
        /// 获取用户对象
        /// </summary>
        /// <returns></returns>
        Task<CF_USER> GetUserAsync(string loginName, string password);

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> ChangePasswordAsync(ChangePasswordRequest request);

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        Task<List<MenuModule>> MyMenusAsync(UserSession userSession, string appname);

        /// <summary>
        /// 获取按钮
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        Task<List<ButtonModule>> MyButtonsAsync(UserSession userSession, string menuNo, string appname);

        /// <summary>
        /// 获取密码
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<string> GetPasswordAsync(string username);

        /// <summary>
        /// 用户组织
        /// </summary>
        /// <returns></returns>
        public Task<List<CorpInfo>> UserCorps(UserSession userSession);

        /// <summary>
        /// 切换组织
        /// </summary>
        /// <returns></returns>
        public Task<bool> ChangeCorp(UserSession userSession, string corpid);

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        Task ExitAsync(UserSession user);
    }
}