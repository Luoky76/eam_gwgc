using EAM.Special.DTO;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Special.Controller
{
    [GksybAuthorize(true)]
    public class BuildController : AreaController
    {
        private readonly IBuildService _service;


        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                shipInfo = comboxData["ShipInfo"],
            }, "成功");
        }
        public BuildController(IBuildService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        public async Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            return await _service.ImportAsync(formFile);
        }

        /// <summary>
        /// 查询年份
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> QryYearAsync(DateTime year)
        {
            var result = await _service.QryYearAsync(year); 
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 模板导出
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public async Task<FileResult> ExportExcelTemplate([FromServices] IWebHostEnvironment webHostEnvironment, string year)
        {
            try
            {
                var datas = await _service.ExportListAsync(year);
                var template = Path.Combine(webHostEnvironment.WebRootPath, "eam", "basexlsx", "施工能耗年度报表模板.xlsx");
                var list = (List<BuildExportData>)datas.Rows;
                var zytimetotal = list.Select(t => t.ZYTIME).Sum();
                var stoptimetotal = list.Select(t => t.STOPTIME).Sum();
                var dailyconsumptiontotal = list.Select(t => t.DAILYCONSUMPTION).Sum();
                var mastertotal = list.Select(t => t.MASTER).Sum();
                var auxiliarytotal = list.Select(t => t.AUXILIARY).Sum();
                var pumptotal = list.Select(t => t.PUMP).Sum();
                var lubricatetotal = list.Select(t => t.LUBRICATE).Sum();
                var boat = list.Select(t => t.DEVICE_NAME).FirstOrDefault();
                var data = new ExportTemplateData<BuildExportData> { 
                    TABLEDATE = DateTime.Now.ToString("yyyy-MM-dd"), 
                    DATEYEAR = DateTime.Now.ToString("yyyy"),
                    ZYTIMETOTAL = zytimetotal.Value.ToString("F2"),
                    STOPTIMETOTAL = stoptimetotal.Value.ToString("F2"),
                    DAILYCONSUMPTIONTOTAL = dailyconsumptiontotal.Value.ToString("F2"),
                    MASTERTOTAL = mastertotal.Value.ToString("F2"),
                    AUXILIARYTOTAL = auxiliarytotal.Value.ToString("F2"),
                    PUMPTOTAL = pumptotal.Value.ToString("F2"),
                    LUBRICATETOTAL = lubricatetotal.Value.ToString("F2"),
                    TOTAL = (mastertotal + auxiliarytotal + pumptotal).Value.ToString("F2"),
                    DEVICE_NAME = boat,
                    List = list };
                return await FileExport.ExportToExcelByTemplate(data, template, "施工能耗年度报表.xlsx");
            }
            catch (Exception ex)
            {
                throw new MessageException(ex.Message);
            }
        }
    }
}
