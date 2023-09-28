#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Message;
using Gksyb.Server.Services.Services.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Message
{
    /// <summary>
    /// 
    /// </summary>
    [GksybAuthorize(true)]
    public class WelcomeController : BaseController
    {
        private readonly WelcomeService _service;

        /// <summary>
        /// 
        /// </summary>
        public WelcomeController(WelcomeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<AjaxResult> GetDeviceRepairCount()
        {
            var result = await _service.GetDeviceRepairCount(new DateTime());
            return AjaxResult.Success(result);
        }


    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问