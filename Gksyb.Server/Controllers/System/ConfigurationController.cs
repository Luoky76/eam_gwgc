using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.System
{
    /// <summary>
    /// 配置管理
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class ConfigurationController : BaseController
    {
        private readonly ConfigurationService _service;

        /// <summary>
        /// 配置管理
        /// </summary>
        /// <param name="service"></param>
        public ConfigurationController(ConfigurationService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult<CF_CONFIGURATION>> GetAsync(long? id)
        {
            if (!id.HasValue) return AjaxResult<CF_CONFIGURATION>.Error("请传递参数");
            return AjaxResult<CF_CONFIGURATION>.Success(await _service.GetAsync(id.Value), "成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<CF_CONFIGURATION> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> UpdateCacheAsync()
        {
            return await _service.UpdateCacheAsync();
        }
    }
}