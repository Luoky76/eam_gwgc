using EAM.Third.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Third.Controllers.OA
{
    public class OAController : BaseController
    {
        private readonly OAService _service;
        
        public OAController(OAService service)
        {
            _service = service;
        }

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
