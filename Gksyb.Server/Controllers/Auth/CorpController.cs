using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Server.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 组织管理
    /// </summary>
    [GksybAuthorize(MenuNo = "CorpManage")]
    public class CorpController : BaseController<CF_CORP>
    {
        private readonly ICorpService _service;

        /// <summary>
        /// 组织管理
        /// </summary>
        /// <param name="service"></param>
        public CorpController(ICorpService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// 树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        public async Task<AjaxResult> ComboxData()
        {
            return AjaxResult.Success(new
            {
                corpData = await _service.CorpData()
            });
        }
    }
}