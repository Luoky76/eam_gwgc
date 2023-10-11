using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpCollectController : AreaController
    {
        private readonly ISpCollectService _service;

        public SpCollectController(ISpCollectService service)
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
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_COLLECT> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_COLLECT_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> RequestListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.RequestListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> RequestSave(SaveRequest<SP_COLLECT_REQUEST> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.RequestSave(request);
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> SpApplyListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpApplyListAsync(request), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult> SelectApplyAsync(List<string> SpdetID,string Cid)
        {
            return AjaxResult.Success(await _service.SelectApply(SpdetID, Cid), "成功");
        }
    }
}
