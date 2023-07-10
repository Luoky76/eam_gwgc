using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class DeviceRunController : AreaController
    {

        private readonly IDeviceRunService _service;

        public DeviceRunController(IDeviceRunService service)
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
                runStatus = comboxData["RunStatus"],
                deviceInfo = comboxData["DeviceInfo"],
            }, "成功");
        }
        /// <summary>
        /// 获取运行状态转换列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRunAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRun(request), "成功");
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageAsync(SaveRequest<RUN_TRANS> request)
        {
            return AjaxResult.Success(await _service.Manage(request), "成功");
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(string sids, string deid, string newStatus)
        {
            return await _service.Submit(sids, deid, newStatus);
        }


        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetAllRunAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetAllRun(request), "成功");
        }

    }
}
