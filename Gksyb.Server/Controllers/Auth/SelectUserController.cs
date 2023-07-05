using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(true)]
    public class SelectUserController : BaseController<CF_CORP>
    {
        private readonly ISelectUserService _service;

        public SelectUserController(ISelectUserService service) 
        {
            _service = service;
        }
        /// <summary>
        /// 获取当前部门的所有人员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> GetCurentGorpUserList(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.GetCurentGorpUserList(request),"成功");
        }
    }
}
