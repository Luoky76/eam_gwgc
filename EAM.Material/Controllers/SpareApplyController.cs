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
    public class SpareApplyController : AreaController
    {
        private readonly SpareApplyService _service;

        public SpareApplyController(SpareApplyService service)
        {
            _service = service;
        }

        #region 物资编码申请
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
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SPARE_APPLY> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }

        [HttpPost]
        public async Task<AjaxResult> ApplySaveAsync(string memo)
        {
            return AjaxResult.Success(await _service.ApplySave(memo), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }

        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelHeader(string filename)
        {
            return await FileExport.ExportToExcelHeader(new SpDetailExportData(), filename);
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

        #endregion

        #region 明细
        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(request), "成功");
        }
        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SPARE_APPLY_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }
        #endregion

        #region 物资编码禁用
        [HttpPost]
        public async Task<AjaxResult<GridData>> SpcatalogListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpcatalogListAsync(request), "成功");
        }
        [HttpPost]
        public async Task<AjaxResult<GridData>> SpDisableListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpDisableListAsync(request), "成功");
        }
        [HttpPost]
        public async Task<AjaxResult<GridData>> SpDisableDetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpDisableDetailListAsync(request), "成功");
        }

        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SpDisableDetailSave(SaveRequest<SP_DISABLE_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SpDisableDetailSave(request);
        }

        /// <summary>
        /// 申请提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SpDisableSubmit(List<string> sids)
        {
            return AjaxResult.Success(await _service.SpDisableSubmit(sids), "成功");
        }

        /// <summary>
        /// 禁用申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SpDisableSave(SaveRequest<SP_DISABLE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SpDisableSave(request);
        }
        #endregion

        #region 物资编码启用
        [HttpPost]
        public async Task<AjaxResult<GridData>> SpEnableListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpEnableListAsync(request), "成功");
        }
        [HttpPost]
        public async Task<AjaxResult<GridData>> SpEnableDetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.SpEnableDetailListAsync(request), "成功");
        }

        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SpEnableDetailSave(SaveRequest<SP_ENABLE_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SpEnableDetailSave(request);
        }

        /// <summary>
        /// 申请提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SpEnableSubmit(List<string> sids)
        {
            return AjaxResult.Success(await _service.SpEnableSubmit(sids), "成功");
        }

        /// <summary>
        /// 启用申请
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SpEnableSave(SaveRequest<SP_ENABLE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SpEnableSave(request);
        }
        #endregion
    }
}
