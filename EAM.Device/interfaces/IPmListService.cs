using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace EAM.Device.Interfaces
{
    public interface IPmListService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 设备数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        Task<AjaxResult> ImportPmAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        Task<PM_STD_LIST> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SaveAsync(SaveRequest<PM_STD_LIST> request);

        /// <summary>
        /// 周期定时器
        /// </summary>
        /// <returns></returns>
        public Task WeekTimer();
        /// <summary>
        /// 月定时器
        /// </summary>
        /// <returns></returns>
        public Task MonthTimer();
        /// <summary>
        /// 季度定时器
        /// </summary>
        /// <returns></returns>
        public Task QuarterTimer();
        /// <summary>
        /// 年度定时器
        /// </summary>
        /// <returns></returns>
        public Task YearTimer();
    }
}