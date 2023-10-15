using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpApplyController : AreaController
    {
        private readonly ISpApplyService _service;

        public SpApplyController(ISpApplyService service)
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
        public async Task<AjaxResult> Save(SaveRequest<SP_APPLY> request)
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

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> CancelSubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.CancelSubmit(sids), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_APPLY_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }

        #region 采购进度跟踪
        [HttpPost]
        public async Task<AjaxResult<GridData>> ApplyListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ApplyListAsync(request), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult> ApplyComboxData()
        {
            return await _service.ApplyComboxData();
        }

        /// <summary>
        /// 采购进度
        /// </summary>
        /// <param name="SPDET_ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ApplyDetFlowAsync(string SPDET_ID)
        {
            return await _service.ApplyDetFlowAsync(SPDET_ID);
        }
        #endregion
    }
}
