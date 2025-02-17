using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class MenuController : BaseController
    {
        private readonly MenuService _service;

        /// <summary>
        /// 菜单管理
        /// </summary>
        /// <param name="service"></param>
        public MenuController(MenuService service)
        {
            _service = service;
        }

        /// <summary>
        /// 树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync(string appname)
        {
            return await _service.TreeAsync(appname);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [JsToken, SkipXssFilter]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_MENU> request)
        {
            return await _service.Save(request);
        }
    }
}