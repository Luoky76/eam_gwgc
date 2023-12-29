#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.Weixin;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [GksybAuthorize(MenuNo = "UserManage")]
    public class UserController : BaseController
    {
        private readonly UserService _service;

        /// <summary>
        /// 用户管理
        /// </summary>
        /// <param name="service"></param>
        public UserController(UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxData([FromServices] IBCCodeService codeService)
        {
            return AjaxResult.Success(new
            {
                role = (await _service.RoleData()).Rows,
                corp = (await _service.CorpData()).Rows,
                station = await codeService.Get("岗位"),
                initPassword = _service.GetInitPassword()
            });
        }

        /// <summary>
        /// 角色数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> RoleData(GridRequest request)
        {
            return AjaxResult.Success(await _service.RoleData(request));
        }

        /// <summary>
        /// 公司数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> CorpData(GridRequest request)
        {
            return AjaxResult.Success(await _service.CorpData(request));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult<GridData>> ListAsync(UserRequest user, GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(user, request));
        }

        /// <summary>
        /// 初始化密码
        /// </summary>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> DoInitPassword(long? id)
        {
            return AjaxResult.Success(await _service.DoInitPassword(id), default);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<UserRequest> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }

        /// <summary>
        /// 微信解绑
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> UnBind([FromServices] IWeixinService weixinService, string openid)
        {
            if (string.IsNullOrWhiteSpace(openid)) return AjaxResult.Error("请传递参数");
            await weixinService.UnBind(openid);
            return AjaxResult.Success();
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问