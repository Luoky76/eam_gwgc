using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class DeviceCardController : AreaController
    {
        private readonly IDeviceCardService _service;

        public DeviceCardController(IDeviceCardService service)
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

        [HttpPost]
        public async Task<AjaxResult<DEVICE_CARD>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<DEVICE_CARD>.Error("请传递参数");
            return AjaxResult<DEVICE_CARD>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request)
        {
            return await _service.SaveAsync(request);
        }
    }
}
