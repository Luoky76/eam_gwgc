using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(MenuNo = "SpScan")]
    public class SpScanController : AreaController
    {
        private readonly SpScanService _service;

        public SpScanController(SpScanService service)
        {
            _service = service;
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
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string scanId)
        {
            return AjaxResult.Success(await _service.GetAsync(scanId));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_SCAN> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitAsync(string scanId)
        {
            await _service.SubmitAsync(scanId);
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeAsync(string scanId)
        {
            await _service.RevokeAsync(scanId);
            return AjaxResult.Success("撤销提交成功");
        }

        /// <summary>
        /// 获取子表列表
        /// </summary>
        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(request));
        }

        /// <summary>
        /// 子表保存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_SCAN_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAllAsync
            (SaveRequest<SP_SCAN> request, SaveRequest<SP_SCAN_DET> requestDet)
        {
            request.Added ??= new List<SP_SCAN>();
            request.Updated ??= new List<SP_SCAN>();
            request.Deleted ??= new List<SP_SCAN>();
            if (request.Added.Count + request.Updated.Count != 1)
            {
                return AjaxResult.Error("主表修改记录有且只能有一条");
            }
            if (request.Deleted.Any())
            {
                return AjaxResult.Error("同时保存方法不能删除主表");
            }
            return await _service.SaveAllAsync(request, requestDet);
        }

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <param name="scanId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GenerateDet(string scanId)
        {
            await _service.GenerateDet(scanId);
            return AjaxResult.Success("生成成功");
        }

        /// <summary>
        /// 待盘点项目
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> DetailAnsListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetailAnsListAsync(request), "成功");
        }

        /// <summary>
        /// 盘点项目提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> DetSubmitAsync(List<string> sids)
        {
            await _service.DetSubmit(sids);
            return AjaxResult.Success("成功");
        }
    }
}
