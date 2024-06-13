using Gksyb.Core.Auth;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 用户个性信息
    /// </summary>
    [GksybAuthorize(true)]
    public class MyInfoController : BaseController
    {
        private readonly MyInfoService _service;

        public MyInfoController(MyInfoService service)
        {
            _service = service;
        }

        /// <summary>
        /// 记录菜单点击
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> MenuClickAsync(string menuNo, string appname)
        {
            if (string.IsNullOrWhiteSpace(menuNo)) return AjaxResult.Success();
            await _service.MenuClickAsync(menuNo, appname);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 自定义列
        /// </summary>
        public async Task<AjaxResult> CustomColumnAsync(string id, string appname)
        {
            if (string.IsNullOrWhiteSpace(id)) return AjaxResult.Success();
            var column = await _service.CustomColumnAsync(id, appname);
            return AjaxResult.Success(column, "");
        }

        /// <summary>
        /// 保存自定义列
        /// </summary>
        public async Task<AjaxResult> CustomColumnSaveAsync(string id, string columns, string appname)
        {
            if (string.IsNullOrWhiteSpace(id)) return AjaxResult.Success();
            await _service.CustomColumnSaveAsync(id, columns, appname);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ExportLog(string menuNo, string url)
        {
            menuNo = string.IsNullOrWhiteSpace(menuNo) ? url : menuNo;
            menuNo.CheckNotNullOrWhiteSpace("编号");
            await _service.UserLogAsync("导出数据", menuNo, $"导出{url}的数据");
            return AjaxResult.Success();
        }
    }
}