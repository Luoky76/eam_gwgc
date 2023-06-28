using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class DeviceConsTreeController : AreaController
    {
        private readonly IDeviceConsTreeService _service;

        /// <summary>
        /// 设备分类
        /// </summary>
        /// <param name="service"></param>
        public DeviceConsTreeController(IDeviceConsTreeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }

        /// <summary>
        /// 树形
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_DEVICE_COMPOSE> request)
        {
            return await _service.SaveAsync(request);
        }
    }
}
