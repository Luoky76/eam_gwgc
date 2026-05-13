using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpOrderStopController : AreaController
    {
        private readonly SpOrderStopService _service;

        public SpOrderStopController(SpOrderStopService service)
        {
            _service = service;
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
        /// 保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_ORDER_STOP> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            await _service.SubmitAsync(sids);
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            await _service.RevokeAsync(sids);
            return AjaxResult.Success("撤销提交成功");
        }

        /// <summary>
        /// 获取子表明细列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(request), "成功");
        }

        /// <summary>
        /// 子表保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_STOP_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 订单选择列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SpOrderListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.SpOrderListAsync(request), "成功");
        }
    }
}
