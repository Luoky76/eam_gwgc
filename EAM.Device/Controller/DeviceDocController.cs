using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class DeviceDocController : AreaController
    {
        private readonly IDeviceDocService _service;
        public DeviceDocController(IDeviceDocService service)
        {
            _service = service;
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

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_DOC> request)
        {
            return await _service.SaveAsync(request);
        }
    }
}
