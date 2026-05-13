using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controller
{
    [GksybAuthorize(MenuNo = "ProviderInfo")]
    public class ProviderInfoController : AreaController
    {
        private readonly ProviderInfoService _service;

        /// <summary>
        /// 供应商信息登记
        /// </summary>
        /// <param name="service"></param>
        public ProviderInfoController(ProviderInfoService service)
        {
            _service = service;
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
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<PROVIDER>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<PROVIDER>.Error("请传递参数");
            return AjaxResult<PROVIDER>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER> request)
        {
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }
    }
}
