#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Message;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Message
{
    /// <summary>
    /// 消息类型管理
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class MessageTemplateController : BaseController
    {
        private readonly MessageTemplateService _service;

        /// <summary>
        /// 消息类型管理
        /// </summary>
        /// <param name="service"></param>
        public MessageTemplateController(MessageTemplateService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_MESSAGE_TEMPLATE> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SendAsync([FromServices] IMessageCenterService service, [FromBody] MessageInfo info)
        {
            await service.SendAsync(info);
            return AjaxResult.Success();
        }

    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问