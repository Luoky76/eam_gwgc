using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpCatalogScanController : AreaController
    {
        private readonly SpCatalogScanService _service;

        public SpCatalogScanController(SpCatalogScanService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var comboxData = await _service.ComboxDataAsync();
            return AjaxResult.Success(new
            {
                auditing = comboxData["Auditing"]
            }, "成功");
        }

        /// <summary>
        /// 导入物料功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportSpListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportSpList(request), "成功");
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

        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_CATALOG_SCAN> request, SaveRequest<SP_CATALOG_SCAN_DET> requestdet)
        {
            return await _service.SaveAsync(request, requestdet);
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return await _service.SubmitAsync(sids);
        }

        [HttpPost]
        public async Task<AjaxResult> DetailListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetailListAsync(request), "成功");
        }
    }
}
