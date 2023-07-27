using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class DeviceVaryController : AreaController
    {
        private readonly IDeviceVaryService _service;

        public DeviceVaryController(IDeviceVaryService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                VaryType = comboxData["VaryType"],
            }, "成功");
        }

        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }
    }
}
