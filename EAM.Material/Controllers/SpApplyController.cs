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
        /// 获取下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetAsync(string applyId)
        {
            if (applyId.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(applyId), "成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_APPLY> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_APPLY> request, SaveRequest<SP_APPLY_DETAIL> requestdet)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAllAsync(request, requestdet);
        }

        /// <summary>
        /// 提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            await _service.SubmitAsync(sids);
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            await _service.RevokeAsync(sids);
            return AjaxResult.Success("撤销提交成功");
        }

        /// <summary>
        /// 获取子表明细列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(request), "成功");
        }

        /// <summary>
        /// 子表保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_APPLY_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 确认提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> CheckSubmitAsync(List<string> sids)
        {
            await _service.CheckSubmitAsync(sids);
            return AjaxResult.Success("确认提交成功");
        }

        /// <summary>
        /// 确认提交撤销
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> CheckRevokeAsync(List<string> sids)
        {
            await _service.CheckRevokeAsync(sids);
            return AjaxResult.Success("确认提交撤销成功");
        }

        /// <summary>
        /// 导出Excel模板
        /// </summary>
        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelHeader(string filename)
        {
            return await FileExport.ExportToExcelHeader(new SpExportData(), filename);
        }

        /// <summary>
        /// 导入明细
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ImportInDetailAsync([FileOptions("xlsx,xls", 20)] IFormFile formFile, string folder, string sid)
        {
            return await _service.ImportInDetailAsync(formFile, folder, sid);
        }

        #region 采购进度跟踪

        /// <summary>
        /// 获取采购进度列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ApplyListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ApplyListAsync(request), "成功");
        }

        /// <summary>
        /// 获取采购进度下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ApplyComboxDataAsync()
        {
            return await _service.ApplyComboxDataAsync();
        }

        /// <summary>
        /// 获取采购进度明细流程
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ApplyDetFlowAsync(string SPDET_ID)
        {
            return await _service.ApplyDetFlowAsync(SPDET_ID);
        }

        #endregion

        /// <summary>
        /// 获取物资确认明细表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetCheckListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetCheckListAsync(request), "成功");
        }

        /// <summary>
        /// 保存物资确认明细
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveCheckListAsync(SaveRequest<SP_APPLY_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveCheckListAsync(request);
        }

        /// <summary>
        /// 物资需求确认提交
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SubmitCheckListAsync(List<string> sids)
        {
            return await _service.SubmitCheckListAsync(sids);
        }

        /// <summary>
        /// 物资需求确认撤销提交
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> RevokeCheckListAsync(List<string> sids)
        {
            return await _service.RevokeCheckListAsync(sids);
        }
    }
}
