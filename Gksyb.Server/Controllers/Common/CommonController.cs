#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Common;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(true)]
    public class CommonController : BaseController
    {
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        [JsToken]
        public async Task<AjaxResult> Upload([FileOptions("jpg,jpeg,bmp,png,gif", 2)] IFormFile formFile, string folder)
        {
            var url = await formFile.SaveAs((folder ?? "").Replace("Public", "", StringComparison.OrdinalIgnoreCase), isCreateDayDirectory: true);
            return AjaxResult.Success(url, formFile.Name);
        }

        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SysdateAsync(SysdateRequest request)
        {
            var sysdate = await _commonService.SysdateAsync(request);
            return AjaxResult.Success(new { Sysdate = sysdate.Item1, Adddate = sysdate.Item2 });
        }

        /// <summary>
        /// 执行视图获取json数据
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, HttpGet]
        public async Task<AjaxResult> JsonValueAsync(QueryViewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ViewName)) return AjaxResult.Error("请传递视图参数");
            var list = await _commonService.JsonValueAsync<dynamic>(request);
            return AjaxResult.Success(list, list.Count.CastTo<string>());
        }

        /// <summary>
        /// 执行多个视图获取json数据
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, HttpGet]
        public async Task<AjaxResult> JsonValueMulAsync(IDictionary<string, IDictionary<string, object>> param)
        {
            var data = await _commonService.JsonValueMulAsync(param);
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取视图配置
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<AjaxResult> QueryConfigAsync([Required] string viewName)
        {
            viewName = await HttpContext.ValidViewAsync(viewName);
            return await _commonService.QueryConfigAsync(viewName);
        }

        /// <summary>
        /// 获取视图配置
        /// </summary>
        /// <returns></returns>
        [JsToken]
        [AllowAnonymous]
        public async Task<AjaxResult> QueryAsync(GridRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.View)) return AjaxResult.Error("视图名称不能为空");
            request.View = await HttpContext.ValidViewAsync(request.View);
            return AjaxResult.Success(await _commonService.QueryAsync(request), "");
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问