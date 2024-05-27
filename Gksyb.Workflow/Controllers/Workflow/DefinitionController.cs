#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Model.WorkFlow;
using Gksyb.Workflow.Services.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Workflow.Controllers.Workflow
{
    /// <summary>
    /// 流程定义
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class DefinitionController : AreaController
    {
        private readonly DefinitionService _service;

        public DefinitionController(DefinitionService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync([FromServices] ICorpService service, [FromServices] IBCCodeService codeService)
        {
            var operatorTypeData = await codeService.Get("找人类型");
            operatorTypeData.Add(new ComboxData()
            {
                ID = "FromService",
                TEXT = "自定义"
            });
            return AjaxResult.Success(new
            {
                corpData = await service.ComboxDataAsync(true),
                operatorTypeData
            });
        }

        /// <summary>
        /// 获取流程定义
        /// </summary>
        public async Task<AjaxResult<WF_FLOW>> GetAsync(string id)
        {
            return AjaxResult<WF_FLOW>.Success(await _service.GetAsync(id));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        [JsToken, SkipXssFilter]
        public async Task<AjaxResult> SaveAsync(SaveRequest<WF_FLOW> request)
        {
            return await _service.SaveAsync(request);
        }

        [JsToken, SkipXssFilter]
        public async Task<AjaxResult> SaveOrderAsync(SaveRequest<WF_FLOW> request)
        {
            await _service.SaveOrderAsync(request.Updated);
            return AjaxResult.Success();
        }

        [JsToken, SkipXssFilter]
        public async Task<AjaxResult> CopyAsync(List<string> ids, List<string> corps)
        {
            await _service.CopyAsync(ids, corps);
            return AjaxResult.Success();
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问