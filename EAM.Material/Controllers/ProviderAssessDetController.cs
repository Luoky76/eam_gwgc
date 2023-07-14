using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controller
{
    [GksybAuthorize(MenuNo = "ProviderAssess")]
    public class ProviderAssessDetController : AreaController
    {
        private readonly IProviderAssessDetService _service;

        /// <summary>
        /// 供应商评估明细
        /// </summary>
        /// <param name="service"></param>
        public ProviderAssessDetController(IProviderAssessDetService service)
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
        /// 根据评估id ASSESS_ID 获取列表
        /// </summary>
        /// <param name="assessId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> CertainAssessListAsync(string assessId)
        {
            return await _service.CertainAssessListAsync(assessId);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<PROVIDER_ASSESS_DET>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<PROVIDER_ASSESS_DET>.Error("请传递参数");
            return AjaxResult<PROVIDER_ASSESS_DET>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_DET> request)
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
