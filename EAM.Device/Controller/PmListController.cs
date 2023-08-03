using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class PmListController : AreaController
    {
        private readonly IPmListService _service;

        public PmListController(IPmListService service)
        {
            _service = service;
        }


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
                maintDept = comboxData["MaintDept"],
                pmcycleUnit = comboxData["PmcycleUnit"],
                pmShippost = comboxData["PmShippost"],
            }, "成功");
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
        public async Task<AjaxResult<PM_STD_LIST>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<PM_STD_LIST>.Error("请传递参数");
            return AjaxResult<PM_STD_LIST>.Success(await _service.GetAsync(id), "成功");
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
            //return AjaxResult.Success(await _service.WeekTimer(), "成功");
        }
    }
}
