using EAM.Material.DTO;
using EAM.Material.Interfaces;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpOrderController : AreaController
    {
        private readonly ISpOrderService _service;

        public SpOrderController(ISpOrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
		/// 获取下拉框数据
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_ORDER> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }

        /// <summary>
        /// 获取明细列表信息
        /// </summary>
        /// <param name="ORDER_ID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> DetailListAsync(string ORDER_ID, GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DetailListAsync(ORDER_ID, request), "成功");
        }
        /// <summary>
        /// 明细保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_ORDER_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetailSave(request);
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> OrderOverListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.OrderOverListAsync(request), "成功");
        }

        /// <summary>
        /// 订单完成情况
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> OrderListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.OrderListAsync(request), "成功");
        }

        /// <summary>
        /// 模板导出
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelTemplate([FromServices] IWebHostEnvironment webHostEnvironment, GridRequest request)
        {
            try
            {
                var datas = await _service.ExportListAsync(request);
                var template = Path.Combine(webHostEnvironment.WebRootPath, "eam", "basexlsx", "采购订单导出模板.xlsx");
                var list = (List<OrderExportData>)datas.Rows;
                var count = list.Select(t => t.ORDER_MONEY).Sum();
                var data = new ExportTemplateData<OrderExportData> { TABLEDATE = DateTime.Now.ToString("yyyy-MM-dd"), DATEYEAR = DateTime.Now.ToString("yyyy"), TOTAL = count.Value.ToString("F2"), List = list };
                return await FileExport.ExportToExcelByTemplate(data, template, "采购订单年度报表.xlsx");
            }
            catch (Exception ex)
            {
                throw new MessageException(ex.Message);
            }
        }
    }
}
