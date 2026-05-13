using EAM.Material.DTO;
using EAM.Material.Services;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpApplyController : AreaController
    {
        private readonly SpApplyService _service;

        public SpApplyController(SpApplyService service)
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
        /// 根据物料领用申请ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetApplyDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<SP_APPLY>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetApplyDetail(ID), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestdet"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_APPLY> request, SaveRequest<SP_APPLY_DETAIL> requestdet)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request, requestdet);
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

        /// <summary>
        /// 确认提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> CheckSubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.CheckSubmit(sids), "成功");
        }

        /// <summary>
        /// 确认提交撤销
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> CheckCancelSubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.CheckCancelSubmit(sids), "成功");
        }

        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelHeader(string filename)
        {
            return await FileExport.ExportToExcelHeader(new SpExportData(), filename);
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="folder"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportInDetail([FileOptions("xlsx,xls", 20)] IFormFile formFile, string folder, string sid)
        {
            return await _service.ImportInDetail(formFile, folder, sid);
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



        /// <summary>
        /// 获取物资确认明细表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> GetCheckListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.GetCheckListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveCheckList(SaveRequest<SP_APPLY_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveCheckList(request);
        }

        /// <summary>
        /// 物资需求确认提交
        /// </summary>
        /// <param name="sids">明细表主键数组</param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SubmitCheckList(List<string> sids)
        {
            return await _service.SubmitCheckList(sids);
        }

        /// <summary>
        /// 物资需求确认撤销提交
        /// </summary>
        /// <param name="sids">明细表主键数组</param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> RevokeCheckList(List<string> sids)
        {
            return await _service.RevokeCheckList(sids);
        }
    }
}
