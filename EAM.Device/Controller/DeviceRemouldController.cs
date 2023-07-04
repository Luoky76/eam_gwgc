using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Gksyb.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class DeviceRemouldController : AreaController
    {
        private readonly IDeviceRemouldService _service;

        public DeviceRemouldController(IDeviceRemouldService service)
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_REMOULD> request)
        {
            return await _service.SaveAsync(request);
        }
    }
}
