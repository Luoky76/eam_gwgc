#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Interfaces.Auth;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 权限管理
    /// </summary>
    [GksybAuthorize(Mode = GksybAuthorizeMode.Regex, IsMenuAppname = true, MenuNo = "PrivilegeManage$")]
    public class PrivilegeController : BaseController
    {
        private readonly PrivilegeService _service;

        /// <summary>
        /// 权限管理
        /// </summary>
        /// <param name="service"></param>
        public PrivilegeController(PrivilegeService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取角色
        /// </summary>
        public async Task<AjaxResult<GridData>> RoleListAsync([FromServices] IRoleService service, GridRequest request)
        {
            return AjaxResult<GridData>.Success(await service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        public async Task<AjaxResult<GridData>> UserListAsync([FromServices] UserService service, GridRequest request)
        {
            var data = await service.ListAsync(new UserRequest(), request);
            return AjaxResult<GridData>.Success(data, "成功");
        }

        /// <summary>
        /// 获取菜单及按钮
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> MenuButtonAsync(string appName)
        {
            return await _service.MenuButtonAsync(appName);
        }

        /// <summary>
        /// 获取用户菜单及按钮
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> UserMenuButtonAsync(long? userId, string appName)
        {
            if (!userId.HasValue) return AjaxResult.Error("用户不能为空");
            return await _service.UserMenuButtonAsync(userId.Value, appName);
        }

        /// <summary>
        /// 角色权限
        /// </summary>
        /// <param name="roleName">角色名</param>
        /// <param name="appName">应用名</param>
        /// <returns></returns>
        public async Task<AjaxResult> RolePrivilegeAsync(string roleName, string appName)
        {
            return await _service.RolePrivilegeAsync(roleName, appName);
        }

        /// <summary>
        /// 用户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> UserPrivilegeAsync(long userId, string appName)
        {
            return await _service.UserPrivilegeAsync(userId, appName);
        }

        /// <summary>
        /// 按钮保存
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<AjaxResult> PrivilegeSaveAsync([FromBody] List<PrivilegeRequest> list)
        {
            return await _service.PrivilegeSaveAsync(list);
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问