#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Common;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(true)]
    public class CommonController : BaseController
    {
        private readonly CommonService _commonService;

        public CommonController(CommonService commonService)
        {
            _commonService = commonService;
        }

        [JsToken]
        public async Task<AjaxResult> Upload([FileOptions("jpg,jpeg,bmp,png,gif", 2)] IFormFile formFile, string folder)
        {
            var url = await formFile.SaveAs((folder ?? "").Replace("Public", "", StringComparison.OrdinalIgnoreCase), isCreateDayDirectory: true);
            return AjaxResult.Success(url, formFile.Name);
        }

        [JsToken]
        public async Task<AjaxResult> FileToken([FromServices] IDistributedCache distributedCache)
        {
            var token = Guid.NewGuid().ToString("N").ToLower();
            await distributedCache.SetAsync(token, CurrentUser.UserID, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            });
            return AjaxResult.Success(token, default);
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
            request.ViewName = await HttpContext.ValidViewAsync(request.ViewName);
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
            if (param == null || param.Count < 1)
            {
                throw new MessageException("请传递参数，请求体应为json");
            }
            var dic = new Dictionary<string, IDictionary<string, object>>();
            if (param != null)
            {
                await param.ForEachAsync(async item =>
                {
                    var key = await HttpContext.ValidViewAsync(item.Key);
                    dic[key] = item.Value;
                });
            }
            var data = await _commonService.JsonValueMulAsync(dic);
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
            return AjaxResult.Success(await _commonService.QueryAsync<dynamic>(request), "");
        }

        /// <summary>
        /// 缓存
        /// </summary>
        [HeadAuthorize]
        [AllowAnonymous]
        public async Task<AjaxResult> StoreAsync(string json)
        {
            var key = await _commonService.StoreAsync(json);
            return AjaxResult.Success(key, default);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> GetStoreAsync([FromHeader] string key)
        {
            return AjaxResult.Success(await _commonService.GetStoreAsync<string>(key), key);
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问