using EAM.Device.services;
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

        private readonly DeviceRunService _service;

        public DeviceRunController(DeviceRunService service)
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
                runStatus = comboxData["RunStatus"],
            }, "成功");
        }
        /// <summary>
        /// 获取设备卡片基础信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> DeviceData()
        {
            return AjaxResult.Success(new
            {
                deviceData = await _service.DeviceData()
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
        /// 根据ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRunDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetRunDetail(ID), "成功");
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
        public async Task<AjaxResult> SubmitAsync(string sids)
        {
            await _service.SubmitAsync(sids);
            return AjaxResult.Success("提交成功");
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
