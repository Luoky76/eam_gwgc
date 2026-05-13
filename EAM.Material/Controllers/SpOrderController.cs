using EAM.Material.DTO;
using EAM.Material.Services;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpOrderController : AreaController
    {
        private readonly SpOrderService _service;

        public SpOrderController(SpOrderService service)
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
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request, string YEAR)
        {
            return AjaxResult.Success(await _service.ListAsync(request, YEAR), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_ORDER> request)
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
        public async Task<AjaxResult> DetListAsync(string ORDER_ID, GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(ORDER_ID, request), "成功");
        }

        /// <summary>
        /// 子表保存
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_ORDER_DETAIL> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 获取超期订单列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> OrderOverListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.OrderOverListAsync(request), "成功");
        }

        /// <summary>
        /// 获取订单完成情况列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> OrderListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.OrderListAsync(request), "成功");
        }

        /// <summary>
        /// 导出Excel模板
        /// </summary>
        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelTemplate([FromServices] IWebHostEnvironment webHostEnvironment, GridRequest request, string YEAR)
        {
            try
            {
                var datas = await _service.ExportListAsync(request, YEAR);
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
