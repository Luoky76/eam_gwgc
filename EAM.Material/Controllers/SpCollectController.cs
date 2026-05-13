using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpCollectController : AreaController
    {
        private readonly SpCollectService _service;

        public SpCollectController(SpCollectService service)
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
        /// 根据ID获取记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetAsync(string collectId)
        {
            if (collectId.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(collectId), "成功");
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_COLLECT> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_COLLECT> request, SaveRequest<SP_COLLECT_REQUEST> requestdet)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAllAsync(request, requestdet);
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
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_COLLECT_DET> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 获取需求列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> RequestListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.RequestListAsync(request), "成功");
        }

        /// <summary>
        /// 需求保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> RequestSaveAsync(SaveRequest<SP_COLLECT_REQUEST> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.RequestSaveAsync(request);
        }

        /// <summary>
        /// 获取待请购的采购申请明细
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SpApplyListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.SpApplyListAsync(request), "成功");
        }

        /// <summary>
        /// 选中采购申请明细
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SelectApplyAsync(List<string> SpdetID, string Cid)
        {
            return AjaxResult.Success(await _service.SelectApplyAsync(SpdetID, Cid), "成功");
        }
    }
}
