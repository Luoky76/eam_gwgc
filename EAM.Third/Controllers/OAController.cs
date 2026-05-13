using EAM.Third.Services;
using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Third.Controllers
{
    [GksybAuthorize(true)]
    public class OAController : ThirdAreaController
    {
        private readonly OAService _service;

        public OAController(OAService service)
        {
            _service = service;
        }

        /// <summary>
        /// OA回调入口
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [SkipXssFilter]
        public Task<string> OADocking(dynamic data)
        {
            var result = _service.GetOADataAsync(data);
            return result;
        }
    }
}
