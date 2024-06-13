using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Workflow.Controllers.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Gksyb.Workflow.Controllers.Api
{
    /// <summary>
    /// 待办
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class FlowEngineController : BaseController
    {
        private readonly IFlowEngineService _service;
        private readonly IUserService _userService;
        private readonly ScopeUser _user;

        public FlowEngineController(IFlowEngineService service, IUserService userService, ScopeUser user)
        {
            _service = service;
            _userService = userService;
            _user = user;
        }

        /// <summary>
        /// 获取流程列表
        /// </summary>
        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> TaskInfoAsync(string taskId)
        {
            var list = await _service.TaskLogAsync(taskId);
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 启动流程 {"FlowId":"2I9BnRW0HmW","FormData":{"money":500}} 或 {"FlowCode":"流程编码","FormData":{"money":500}}
        /// </summary>
        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> StartAsync(FlowRequest request)
        {
            await GetUserInfo(request);
            await _service.StartAsync(request);
            return AjaxResult.Success(request);
        }

        private async Task<UserInfo> GetUserInfo(FlowRequest request)
        {
            Expression<Func<UserInfo, bool>> filter = string.IsNullOrWhiteSpace(request.WorkerCode) ? c => c.Phone == request.Phone
            : c => c.WorkerCode == request.WorkerCode;
            var list = await _userService.FindUsersAsync(filter, false);
            MessageException.ThrowIf(list.Count < 1, $"找不到{(string.IsNullOrWhiteSpace(request.WorkerCode) ? request.Phone : request.WorkerCode)}的记录");
            var user = list[0];
            _user.UserID = user.Id.Value;
            _user.UserName = user.Name;
            if (user.Corps?.Count == 1)
            {
                request.CorpId = user.Corps.FirstOrDefault().CorpID;
            }
            return user;
        }
    }
}