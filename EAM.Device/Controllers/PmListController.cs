using EAM.Device.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class PmListController : AreaController
    {
        private readonly PmListService _service;

        public PmListController(PmListService service)
        {
            _service = service;
        }


        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var comboxData = await _service.ComboxDataAsync();
            return AjaxResult.Success(new
            {
                maintDept = comboxData["MaintDept"],
                pmcycleUnit = comboxData["PmcycleUnit"],
                deviceInfo = comboxData["DeviceInfo"],
                pmShippost = comboxData["PmShippost"],
            }, "成功");
        }

        /// <summary>
        /// 设备数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportPmAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            return await _service.ImportPmAsync(formFile);
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _service.ListAsync(request);
        }

        [HttpPost]
        public async Task<AjaxResult> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<PM_STD_LIST> request)
        {
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 周期定时器
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> WeekTimerAsync()
        {
            await _service.WeekTimer();
            return AjaxResult.Success();
        }
        /// <summary>
        /// 月定时器
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> MonthTimerAsync()
        {
            await _service.MonthTimer();
            return AjaxResult.Success();
        }
        /// <summary>
        /// 季度定时器
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> QuarterTimerAsync()
        {
            await _service.QuarterTimer();
            return AjaxResult.Success();
        }
        /// <summary>
        /// 年度定时器
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> YearTimerAsync()
        {
            await _service.YearTimer();
            return AjaxResult.Success();
        }
    }
}
