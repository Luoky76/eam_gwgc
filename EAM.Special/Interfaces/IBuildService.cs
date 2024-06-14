using EAM.Special.DTO;
using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace EAM.Special.Interfaces
{
    public interface IBuildService : IService
    {

        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> GetAsync(string ID);

        Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request);

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile);
        /// <summary>
        /// 查询年份
        /// </summary>
        /// <returns></returns>
        Task<GridData> QryYearAsync(GridRequest request, string startdate, string enddate);
        Task<GridData> ExportYearListAsync(string year);
        Task<List<BuildMonthExportData>> ExportMonthListAsync(string year);
    }
}
