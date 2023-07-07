using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [GksybAuthorize(MenuNo = "OrganizationManage")]
    public class RoleController : BaseController
    {
        private readonly IRoleService _service;

        /// <summary>
        /// 角色管理
        /// </summary>
        /// <param name="service"></param>
        public RoleController(IRoleService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return AjaxResult.Success(new
            {
                corpData = await _service.CorpData()
            });
        }

        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        [JsToken]
        public virtual async Task<AjaxResult> SaveAsync(SaveRequest<CF_ROLE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }
    }
}