using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class BaseSpHouseController : AreaController
    {
        private readonly BaseSpHouseService _service;

        public BaseSpHouseController(BaseSpHouseService service)
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
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string id)
        {
            return await _service.GetAsync(id);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_HOUSE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 获取树形数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }
    }
}
