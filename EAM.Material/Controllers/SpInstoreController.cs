using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(MenuNo = "SpReceive,SpInstore")]
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
            return AjaxResult.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string inId)
        {
            return await _service.GetAsync(inId);
        }

        /// <summary>
        /// 获取明细列表
        /// </summary>
        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(request));
        }

        /// <summary>
        /// 获取可导入的采购物资明细
        /// </summary>
        public async Task<AjaxResult> ImportListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportListAsync(request));
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestDet)
        {
            return await _service.SaveAllAsync(request, requestDet);
        }

        /// <summary>
        /// 提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitAsync(string inId)
        {
            await _service.SubmitAsync(inId);
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeAsync(string inId)
        {
            await _service.RevokeAsync(inId);
            return AjaxResult.Success("撤销提交成功");
        }
    }
}
