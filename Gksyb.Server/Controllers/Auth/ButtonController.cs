using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 按钮管理
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class ButtonController : BaseController
    {
        private readonly ButtonService _service;

        /// <summary>
        /// 按钮管理
        /// </summary>
        /// <param name="service"></param>
        public ButtonController(ButtonService service)
        {
            _service = service;
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
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_BUTTON> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 批量添加增删改查按钮
        /// </summary>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<AjaxResult> BatchAdd(string menuNo, string appname)
        {
            menuNo.CheckNotNullOrWhiteSpace("菜单编号");
            appname.CheckNotNullOrWhiteSpace("应用名称");
            return await _service.BatchAdd(menuNo, appname);
        }

        /// <summary>
        /// 清空按钮
        /// </summary>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<AjaxResult> ClearAsync(string menuNo, string appname)
        {
            menuNo.CheckNotNullOrWhiteSpace("菜单编号");
            appname.CheckNotNullOrWhiteSpace("应用名称");
            return await _service.Clear(menuNo, appname);
        }
    }
}