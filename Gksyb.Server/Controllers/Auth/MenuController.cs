using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [GksybAuthorize(IsDeveloper = true)]
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

        /// <summary>
        /// 生成图标
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> GenerateAsync(string appname)
        {
            await _service.GenerateAsync(appname);
            return AjaxResult.Success();
        }

        [AllowAnonymous]
        [HttpGet("[action]/{prefix}.json")]
        public IActionResult IConify([FromServices] IWebHostEnvironment environment, string prefix, [FromQuery] string icons)
        {
            var filePath = Path.Combine(environment.WebRootPath, "vben", "iconify", $"{prefix}-{icons}.json");
            if (!IOFile.Exists(filePath))
            {
                return NotFound($"找不到 {prefix}.json 文件");
            }
            return PhysicalFile(filePath, "application/json");
        }
    }
}