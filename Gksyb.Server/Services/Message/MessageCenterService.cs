#pragma warning disable IDE0051 // 删除未使用的私有成员
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.Weixin;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Gksyb.Server.Services.Message
{
    public class MessageCenterService : IMessageCenterService
    {
        private readonly IDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserSession _user;

        public MessageCenterService(IDbContext dbContext, UserSession user, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _user = user ?? new UserSession()
            {
                UserID = 0,
                UserName = "Auto"
            };
            if (_user == null)
            {
                var options = serviceProvider.GetService<IOptions<SysContextOptions>>().Value;
                _user = new UserSession()
                {
                    UserID = 0,
                    UserName = "Auto",
                    MenuAppname = options.AppName
                };
            }
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task SendToAllAsync(MessageInfo info)
        {
            var hubContext = _serviceProvider.GetService<IHubContext<BroadcastChannelHub, IBroadcastChannelClient>>();
            await hubContext.Clients.SendAsync(info, true);
        }

        /// <inheritdoc/>
        public async Task SendAsync(MessageInfo info, bool isCode = false)
        {
            if (!string.IsNullOrWhiteSpace(info.MsgGroup) && string.IsNullOrWhiteSpace(info.Code))
            {
                var codes = await _dbContext.Query<SYS_MESSAGE_TEMPLATE>().Where(c => c.MSG_GROUP == info.MsgGroup).Select(c => c.CODE).ToListAsync();
                foreach (var code in codes)
                {
                    var model = info.MapTo<MessageInfo>();
                    model.Code = code;
                    await SendInnerAsync(model, isCode);
                }
                return;
            }
            await SendInnerAsync(info, isCode);
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        public async Task SendInnerAsync(MessageInfo info, bool isCode = false)
        {
            var hasCode = await InitWithTemplate(info);
            if (!hasCode && isCode) return;
            info.MsgType = string.IsNullOrWhiteSpace(info.MsgType) ? "Message" : info.MsgType;
            var method = $"Send{info.MsgType ?? MessageInfoExtensions.Message}Async";
            MessageException.ThrowIf(!_methodInfos.ContainsKey(method), $"不支持{info.MsgType}");
            var invokeResult = _methodInfos[method].Invoke(this, new object[] { info });
            if (invokeResult is Task task) await task;
        }

        /// <summary>
        /// 根据模板赋值
        /// </summary>
        private async Task<bool> InitWithTemplate(MessageInfo info)
        {
            info.MsgGroup = "其他";
            if (string.IsNullOrWhiteSpace(info.Code)) return false;
            var model = await _dbContext.Query<SYS_MESSAGE_TEMPLATE>().Where(c => c.CODE == info.Code).FirstOrDefaultAsync();
            var hasCode = model != null;
            if (hasCode)
            {
                info.MsgType = model.MSG_TYPE;
                info.DialogMode = model.DIALOG_MODE;
                info.DialogType = model.DIALOG_TYPE;
                info.AutoReaded = model.AUTO_READED;
                info.MsgGroup = model.MSG_GROUP;
                info.Template = model.TEMPLATE;
                info.Href = string.IsNullOrWhiteSpace(info.Href) ? model.MSG_HREF : info.Href;
                info.MobileHref = string.IsNullOrWhiteSpace(info.MobileHref) ? model.MSG_MOBILE_HREF : info.MobileHref;
                info.Receives ??= new List<string>();
                if (!string.IsNullOrWhiteSpace(model.NOTICE_TYPE))
                {
                    var userService = _serviceProvider.GetService<IUserService>();
                    var receives = await userService.FindOperators(new FindOperatorInfo()
                    {
                        Type = model.NOTICE_TYPE,
                        Corp = info.CorpId ?? _user.Corp?.CorpID,
                        Operators = model.NOTICE_USERS,
                        HasSuper = true
                    });
                    info.Receives.AddRange(receives.Select(c => c.Account));
                    info.Receives = info.Receives.DistinctAndOrderBy().ToList();
                }
            }
            info.Handle();
            return hasCode;
        }


        /// <summary>
        /// 加入站内消息表
        /// </summary>
        private async Task SendMessageAsync(MessageInfo info)
        {
            if (info.AutoReaded == "1" || (!string.IsNullOrWhiteSpace(info.Action) && info.Action != MessageInfoExtensions.ActionName))
            {
                var hubContext = _serviceProvider.GetService<IHubContext<BroadcastChannelHub, IBroadcastChannelClient>>();
                await hubContext.Clients.SendAsync(info);
                return;
            }
            var now = await _dbContext.GetSysdate();
            var appname = string.IsNullOrWhiteSpace(info.Appname) ? _user.MenuAppname : info.Appname;
            var messages = info.Receives.Select(c => new SYS_MESSAGE()
            {
                ID = GuidHelper.NewSnowflakeId(),
                TEMPLATE_CODE = info.Code,
                MSG_TITLE = info.Title,
                MSG_CONTENT = string.IsNullOrWhiteSpace(info.Content) ? info.Template : info.Content,
                MSG_GROUP = info.MsgGroup,
                DIALOG_MODE = info.DialogMode,
                DIALOG_TYPE = info.DialogType,
                MSG_HREF = info.Href,
                MSG_HREF_TARGET = info.Target,
                MSG_MOBILE_HREF = info.MobileHref,
                MSG_KEY = info.Key,
                AUTO_READED = info.AutoReaded ?? "0",
                NOTICE_USER = c,
                CREATEUSERID = _user.UserID,
                CREATEUSER = _user.UserName,
                CREATEDATE = now,
                APPNAME = appname
            }).ToList();
            await _dbContext.InsertRangeAsync(messages);
        }

        /// <summary>
        /// 加入微信
        /// </summary>
        private async Task SendWeixinAsync(MessageInfo info)
        {
            var service = _serviceProvider.GetService<IWeixinService>();
            await service.CreateNotice(new WeixinNoticeRequest()
            {
                Receiver = info.Receives.ToStr(","),
                Template = string.IsNullOrWhiteSpace(info.Template) ? info.Code : info.Template,
                Url = info.Href,
                TData = info.Data == null ? info.Content : info.Data.ToJson(),
                Creater = _user.UserName,
                SendTime = info.SendTime
            });
        }

        private static readonly Dictionary<string, MethodInfo> _methodInfos = null;

        /// <summary>
        /// 初始化
        /// </summary>
        static MessageCenterService()
        {
            _methodInfos = typeof(MessageCenterService).GetDicMethods();
        }
    }
}
#pragma warning restore IDE0051 // 删除未使用的私有成员