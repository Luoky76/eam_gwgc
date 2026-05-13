using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpInstoreController : AreaController
    {
        private readonly SpInstoreService _service;

        public SpInstoreController(SpInstoreService service)
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
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 获取子表明细列表
        /// </summary>
        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            var result = await _service.DetListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 获取子表明细列表（详情用）
        /// </summary>
        public async Task<AjaxResult> DetailListAsync(GridRequest request)
        {
            var result = await _service.DetailListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet)
        {
            return await _service.SaveAllAsync(request, requestdet);
        }

        /// <summary>
        /// 提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return await _service.SubmitAsync(sids);
        }

        /// <summary>
        /// 退回
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> BackAsync(List<string> sids)
        {
            return await _service.BackAsync(sids);
        }

        /// <summary>
        /// 获取仓库列表
        /// </summary>
        public async Task<AjaxResult> HouseList()
        {
            return await _service.HouseList();
        }
    }
}
