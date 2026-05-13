using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(MenuNo = "BaseSpCatalog")]
    public class BaseSpCatalogController : AreaController
    {
        private readonly BaseSpCatalogService _service;

        public BaseSpCatalogController(BaseSpCatalogService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }

        /// <summary>
        /// 树形
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _service.ListAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_SPCATALOG> request)
        {
            return await _service.SaveAsync(request);
        }
    }
}
