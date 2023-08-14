using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpScanController : AreaController
    {
        private readonly ISpScanService _service;

        public SpScanController(ISpScanService service)
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
        public async Task<AjaxResult> Save(SaveRequest<SP_SCAN> request)
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
        public async Task<AjaxResult<GridData>> DetailListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_SCAN_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <param name="SCAN_ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GenerateDet(string SCAN_ID)
        {
            return AjaxResult.Success(await _service.GenerateDet(SCAN_ID), "成功");
        }


        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailAnsListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailAnsListAsync(request), "成功");
        }

        /// <summary>
        /// 盘点提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> DetSubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.DetSubmit(sids), "成功");
        }
    }
}
