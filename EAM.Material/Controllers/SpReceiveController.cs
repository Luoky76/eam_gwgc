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
        /// 根据收货ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string receiveId)
        {
            return await _service.GetAsync(receiveId);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_RECEIVE> request, SaveRequest<SP_RECEIVE_DET> requestdet)
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
        /// 撤销提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            return await _service.RevokeAsync(sids);
        }

        /// <summary>
        /// 验收提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SubmitCheckAsync(List<string> sids)
        {
            return await _service.SubmitCheckAsync(sids);
        }

        /// <summary>
        /// 验收提交撤销
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> RevokeCheckAsync(List<string> sids)
        {
            return await _service.RevokeCheckAsync(sids);
        }

        /// <summary>
        /// 子表保存
        /// </summary>
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_RECEIVE_DET> request)
        {
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        public async Task<AjaxResult> OrderList()
        {
            return await _service.OrderList();
        }

        /// <summary>
        /// 获取物资列表
        /// </summary>
        public async Task<AjaxResult> SpList(GridRequest request)
        {
            return await _service.SpList(request);
        }
    }
}
