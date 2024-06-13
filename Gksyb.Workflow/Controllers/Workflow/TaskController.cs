using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Workflow.Services.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Workflow.Controllers.Workflow
{
    [GksybAuthorize(IsBaseAuth = true)]
    public class TaskController : AreaController
    {
        private readonly TaskService _service;

        public TaskController(TaskService service)
        {
            _service = service;
        }

        /// <summary>
        /// 创建
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> StartAsync(FlowExecuteInfo info)
        {
            await _service.StartAsync(info);
            return AjaxResult.Success(info);
        }

        /// <summary>
        /// 草稿
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> DraftAsync(FlowExecuteInfo info)
        {
            info.NodeStatus = NodeStatus.Draft;
            await _service.StartAsync(info);
            return AjaxResult.Success(info);
        }

        /// <summary>
        /// 同意
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> AgreeAsync(FlowExecuteInfo info)
        {
            info.NodeStatus = NodeStatus.Agree;
            return await ExcuteAsync(info);
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RejectAsync(FlowExecuteInfo info)
        {
            info.NodeStatus = NodeStatus.Reject;
            return await ExcuteAsync(info);
        }

        /// <summary>
        /// 退回
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> BackAsync(FlowExecuteInfo info)
        {
            info.NodeStatus = NodeStatus.Back;
            return await ExcuteAsync(info);
        }

        /// <summary>
        /// 取消
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> CancelAsync(FlowExecuteInfo info)
        {
            await _service.CancelAsync(info);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 标记成已阅
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> ReadAsync(List<string> ids)
        {
            await _service.ReadAsync(ids);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 全部标记成已阅
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> ReadAllAsync()
        {
            await _service.ReadAllAsync();
            return AjaxResult.Success();
        }

        /// <summary>
        /// 执行
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> ExcuteAsync(FlowExecuteInfo info)
        {
            await _service.ExcuteAsync(info);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 抄送
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> ShareAsync(FlowExecuteInfo info)
        {
            await _service.ShareAsync(info);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 转办
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> TransferAsync(FlowExecuteInfo info)
        {
            await _service.TransferAsync(info);
            return AjaxResult.Success();
        }
    }
}