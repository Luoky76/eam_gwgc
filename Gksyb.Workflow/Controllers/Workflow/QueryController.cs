#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Workflow.Services.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Workflow.Controllers.Workflow
{
    [GksybAuthorize(IsBaseAuth = true)]
    public class QueryController : AreaController
    {
        private readonly QueryService _service;

        public QueryController(QueryService service)
        {
            _service = service;
        }

        /// <summary>
        /// 公司数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> CorpDataAsync([FromServices] ICorpService service)
        {
            return AjaxResult.Success(await service.ComboxDataAsync(true));
        }

        /// <summary>
        /// 处理意见
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ReasonDataAsync([FromServices] IBCCodeService service)
        {
            return AjaxResult.Success(await service.Get("流程处理意见"));
        }

        /// <summary>
        /// 状态数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> StatusDataAsync([FromServices] IBCCodeService service)
        {
            return AjaxResult.Success(await service.Get("流程节点状态"));
        }

        /// <summary>
        /// 流程列表
        /// </summary>
        public async Task<AjaxResult<GridData>> FlowListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.FlowListAsync(request));
        }

        /// <summary>
        /// 获取待办
        /// </summary>
        public async Task<AjaxResult<GridData>> ToDoAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ToDoAsync(request));
        }

        /// <summary>
        /// 获取已办
        /// </summary>
        public async Task<AjaxResult<GridData>> DoneAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.DoneAsync(request));
        }

        /// <summary>
        /// 获取我发起的流程
        /// </summary>
        public async Task<AjaxResult<GridData>> MyFlowAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.MyFlowAsync(request));
        }

        /// <summary>
        /// 获取知会我的
        /// </summary>
        public async Task<AjaxResult<GridData>> ToReadAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ToReadAsync(request));
        }

        /// <summary>
        /// 任务详情
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> TaskInfoAsync(string id, string flowId)
        {
            return AjaxResult.Success(await _service.TaskInfoAsync(id, flowId));
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问