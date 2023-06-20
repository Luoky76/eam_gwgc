#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Common
{
    [GksybAuthorize(true)]
    public class PopupController : BaseController
    {
        /// <summary>
        /// 所有用户
        /// </summary>
        public async Task<AjaxResult> AllUsersAsync([FromServices] IUserService service)
        {
            return AjaxResult.Success(await service.AllUsers());
        }

        /// <summary>
        /// 获取无公司、相同公司、子公司的用户
        /// </summary>
        public async Task<AjaxResult> UsersAsync([FromServices] IUserService service)
        {
            return AjaxResult.Success(await service.Users());
        }

        /// <summary>
        /// 获取无公司、相同公司、子公司的用户（包括自己）
        /// </summary>
        public async Task<AjaxResult> UsersWithSelfAsync([FromServices] IUserService service)
        {
            return AjaxResult.Success(await service.Users(false));
        }

        /// <summary>
        /// 找人类型
        /// </summary>
        public async Task<AjaxResult> FindPersionAsync([FromServices] IBCCodeService codeService)
        {
            return AjaxResult.Success(await codeService.Get("找人类型"));
        }

        /// <summary>
        /// 找人下拉数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> FindPersionComboxDataAsync([FromServices] IBCCodeService codeService, [FromServices] IUserService userService, [FromServices] IRoleService roleService)
        {
            return AjaxResult.Success(new
            {
                FindPersion = await codeService.Get("找人类型"),
                Station = await codeService.Get("岗位"),
                Group = await userService.GroupsAsync(),
                Role = await roleService.ComboxDataAsync(),
                User = await userService.ComboxDataAsync()
            });
        }

        /// <summary>
        /// 岗位数据
        /// </summary>
        public async Task<AjaxResult> StationsAsync([FromServices] IBCCodeService codeService)
        {
            return AjaxResult.Success(await codeService.Get("岗位"));
        }

        /// <summary>
        /// 用户组数据
        /// </summary>
        public async Task<AjaxResult> GroupsAsync([FromServices] IUserService service)
        {
            return AjaxResult.Success(await service.GroupsAsync());
        }

        /// <summary>
        /// 用户数据
        /// </summary>
        public async Task<AjaxResult> UserDataAsync([FromServices] IUserService service)
        {
            return AjaxResult.Success(await service.ComboxDataAsync());
        }

        /// <summary>
        /// 角色数据
        /// </summary>
        public async Task<AjaxResult> RolesAsync([FromServices] IRoleService service)
        {
            return AjaxResult.Success(await service.ComboxDataAsync());
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问
