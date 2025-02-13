using Gksyb.Core.Auth;

namespace Gksyb.Core.Interfaces.Auth
{
    public interface IRoleModuleService : IService
    {
        /// <summary>
        /// 获取菜单权限
        /// </summary>
        /// <returns></returns>
        Task<List<MenuModule>> GetMenuModule(string roleName, string menuAppname);

        /// <summary>
        /// 获取按钮权限
        /// </summary>
        /// <returns></returns>
        Task<List<ButtonModule>> GetButtonModule(string roleName, string menuAppname, string menuNo, GksybAuthorizeMode mode);

        /// <summary>
        /// 添加缺失父菜单
        /// </summary>
        /// <param name="menus">菜单</param>
        Task AddMissingParent(List<MenuModule> menus);

        /// <summary>
        /// 验证菜单权限
        /// </summary>
        /// <returns></returns>
        Task<bool> ValidMenuModule(string roleName, string menuAppname, string menuNo, GksybAuthorizeMode mode);

        /// <summary>
        /// 验证按钮权限
        /// </summary>
        /// <returns></returns>
        Task<bool> ValidButtonModule(string roleName, string menuAppname, string menuNo, string btnNo, GksybAuthorizeMode mode);

        /// <summary>
        /// 移除角色缓存
        /// </summary>
        /// <returns></returns>
        Task Remove(string roleName, string menuAppname);

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <returns></returns>
        Task Clear(string roleAppname, string menuAppname);
    }
}