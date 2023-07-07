using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace Gksyb.Server.Services.Message
{
    /// <summary>
    /// 消息
    /// </summary>
    public class MessageService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;

        public MessageService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _user = userSession;
        }

        /// <summary>
        /// 获取未读数量
        /// </summary>
        public async Task<int> UnReadCountAsync()
        {
            return await _dbContext.Query<SYS_MESSAGE>(c => c.NOTICE_USER == _user.UserName && c.APPNAME == _user.MenuAppname && c.READDATE == null).CountAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_MESSAGE>(c => c.NOTICE_USER == _user.UserName && c.APPNAME == _user.MenuAppname)
                .Select(c => new { c.ID, c.MSG_GROUP, c.MSG_TITLE, c.MSG_CONTENT, c.MSG_HREF, c.MSG_HREF_TARGET, c.MSG_MOBILE_HREF, c.CREATEDATE, c.READDATE }).GetGridData(request);
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        public async Task ReadAsync(long id)
        {
            await _dbContext.UpdateAsync<SYS_MESSAGE>(c => c.ID == id && c.NOTICE_USER == _user.UserName && c.READDATE == null, c => new SYS_MESSAGE
            {
                READDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 读取所有消息
        /// </summary>
        public async Task ReadAllAsync()
        {
            await _dbContext.UpdateAsync<SYS_MESSAGE>(c => c.NOTICE_USER == _user.UserName && c.READDATE == null, c => new SYS_MESSAGE
            {
                READDATE = DateTime.Now
            });
        }
    }
}