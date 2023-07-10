using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class DeviceStopController : AreaController
    {
        private readonly IDeviceStopService _service;

        public DeviceStopController(IDeviceStopService service)
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
                stopSource = comboxData["StopSource"],
                deviceInfo = comboxData["DeviceInfo"],
                malType = comboxData["MalType"],

            }, "成功");
        }

        /// <summary>
        /// 获取停机记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetStopListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetStopList(request), "成功");
        }

        /// <summary>
        /// 管理停机记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageStopAsync(SaveRequest<RUN_STOP> request)
        {
            return await _service.ManageStop(request);
        }


        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return await _service.Submit(sids);
        }

        /// <summary>
        /// 获取停机分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetStopTypeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetStopTypeList(request), "成功");
        }
        /// <summary>
        /// 管理停机分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageStopTypeAsync(SaveRequest<RUN_STOP_TYPE> request)
        {
            return await _service.ManageStopType(request);
        }
    }
}