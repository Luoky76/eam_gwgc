#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Message;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Message
{
    /// <summary>
    /// 消息中心
    /// </summary>
    [GksybAuthorize(true)]
    public class MessageController : BaseController
    {
        private readonly MessageService _service;

        /// <summary>
        /// 消息中心>
        /// </summary>
        public MessageController(MessageService service)
        {
            _service = service;
        }

        /// <summary>
        /// 树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync([FromServices] MessageTemplateService service)
        {
            return await service.TreeAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> UnReadCountAsync()
        {
            return AjaxResult.Success(await _service.UnReadCountAsync());
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        public async Task<AjaxResult> ReadAsync(long id)
        {
            await _service.ReadAsync(id);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 读取所有消息
        /// </summary>
        public async Task<AjaxResult> ReadAllAsync()
        {
            await _service.ReadAllAsync();
            return AjaxResult.Success();
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问