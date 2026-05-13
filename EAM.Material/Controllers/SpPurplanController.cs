using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpPurplanController : AreaController
    {
        private readonly SpPurplanService _service;

        public SpPurplanController(SpPurplanService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
		/// 获取下拉框数据
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_PURPLAN> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult> CancelSubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.CancelSubmit(sids), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailListAsync(string PURPLAN_ID, GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(PURPLAN_ID, request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_PURPLAN_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }
    }
}
