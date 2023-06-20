using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 字典管理
    /// </summary>
    [GksybAuthorize(IsOurCompany = true)]
    public class BcCodeController : BaseController<BC_CODE>
    {
        public BcCodeController(IService<BC_CODE> service) : base(service)
        {
        }
    }
}