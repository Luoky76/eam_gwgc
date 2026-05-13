using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpReceiveController : AreaController
    {
        private readonly SpReceiveService _service;
        public SpReceiveController(SpReceiveService service)
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
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            var result = await _service.DetListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }
        public async Task<AjaxResult> Save(SaveRequest<SP_RECEIVE> request, SaveRequest<SP_RECEIVE_DET> requestdet)
        {
            return await _service.Save(request, requestdet);
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return await _service.SubmitAsync(sids);
        }

        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            return await _service.RevokeAsync(sids);
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitCheckAsync(List<string> sids)
        {
            return await _service.SubmitCheckAsync(sids);
        }

        [HttpPost]
        public async Task<AjaxResult> RevokeCheckAsync(List<string> sids)
        {
            return await _service.RevokeCheckAsync(sids);
        }

        public async Task<AjaxResult> SaveDet(SaveRequest<SP_RECEIVE_DET> request)
        {
            return await _service.SaveDet(request);
        }
        public async Task<AjaxResult> OrderList()
        {
            return await _service.OrderList();
        }
        public async Task<AjaxResult> SpList(GridRequest request)
        {
            return await _service.SpList(request);
        }
    }
}
