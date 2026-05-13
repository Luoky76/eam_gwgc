using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpInBackController : AreaController
    {
        private readonly SpInBackService _service;

        public SpInBackController(SpInBackService service)
        {
            _service = service;
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
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        /// <summary>
        /// 获取入库列表
        /// </summary>
        public async Task<AjaxResult> InListAsync()
        {
            return await _service.InListAsync();
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
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_IN_BACK> request, SaveRequest<SP_INBACK_DET> requestdet)
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
    }
}
