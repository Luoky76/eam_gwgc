using EAM.Device.services;
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class ReportController : AreaController
    {
        private readonly ReportService _service;

        public ReportController(ReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// 单船成本统计
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> CostReportAsync(string dateFrom, string dateTo)
        {
            return await _service.CostReportAsync(dateFrom, dateTo);
        }
    }
}