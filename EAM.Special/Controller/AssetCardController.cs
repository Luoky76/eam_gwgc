using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(true)]
    public class AssetCardController : AreaController
    {
        private readonly AssetCardService _service;

        /// <summary>
        /// 药品采购登记主表
        /// </summary>
        /// <param name="service"></param>
        public AssetCardController(AssetCardService service)
        {
            _service = service;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 无形资产列表（软件）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> SoftwareListAsync(GridRequest request)
        {
            return await _service.SoftwareListAsync(request);
        }

        /// <summary>
        /// 固定资产列表（设备）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> DeviceListAsync(GridRequest request)
        {
            return await _service.DeviceListAsync(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<ASSET_CARD>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<ASSET_CARD>.Error("请传递参数");
            return AjaxResult<ASSET_CARD>.Success(await _service.GetAsync(id), "成功");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<ASSET_CARD> request)
        {
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitAsync(sids), "成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.RevokeAsync(sids), "成功");
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }
    }
}
