using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(true)]
    public class AssetReportController : AreaController
    {
        private readonly IAssetReportService _service;

        /// <summary>
        /// 药品采购登记主表
        /// </summary>
        /// <param name="service"></param>
        public AssetReportController(IAssetReportService service)
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
        /// 维修申请列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ApplyListAsync(GridRequest request)
        {
            var result =  await _service.ApplyListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 维修实施列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> CheckListAsync(GridRequest request)
        {
            var result = await _service.CheckListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 委外维修列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> OutsourceListAsync(GridRequest request)
        {
            var result = await _service.OutsourceListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 维修验收列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> AcceptListAsync(GridRequest request)
        {
            var result = await _service.AcceptListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<ASSET_REPORT_AND_CARD>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<ASSET_REPORT_AND_CARD>.Error("请传递参数");
            return AjaxResult<ASSET_REPORT_AND_CARD>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public AjaxResult<string> CreatePrimaryKey()
        {
            return AjaxResult<string>.Success(_service.CreatePrimaryKey(), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<ASSET_REPORT> request)
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
