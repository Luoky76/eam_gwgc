using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace Gksyb.Server.Services.Message
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public class MessageTemplateService : IBaseService
    {
        private readonly IDbContext _dbContext;

        public MessageTemplateService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<SYS_MESSAGE_TEMPLATE>().Select(c => c.GROUP).Distinct().ToListAsync();
            var data = list.DistinctAndOrderBy().Select(c => new
            {
                ID = c,
                TEXT = c,
                PARENTID = "ROOT",
                ICON = "fa fa-folder-open"
            }).ToList();
            data.Insert(0, new { ID = "UNREAD", TEXT = "未读", PARENTID = "ROOT", ICON = "fa fa-send" });
            data.Add(new { ID = "ROOT", TEXT = "全部", PARENTID = "", ICON = "fa fa-folder" });
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_MESSAGE_TEMPLATE>().GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SYS_MESSAGE_TEMPLATE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.CODE, c.NAME, c.GROUP, c.MSG_TYPE, c.DIALOG_MODE, c.DIALOG_TYPE, c.AUTO_READED, c.NOTICE_TYPE, c.NOTICE_USERS, c.MSG_HREF, c.MSG_MOBILE_HREF },
                c => a => a.ID == c.ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, true);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(SYS_MESSAGE_TEMPLATE entity)
        {
            await Check(entity);
            entity.ID = GuidHelper.NewShortId();
            entity.FLAG = "1";
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SYS_MESSAGE_TEMPLATE entity)
        {
            await Check(entity);
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SYS_MESSAGE_TEMPLATE entity)
        {
            await Task.CompletedTask;
            entity.FLAG = "0";
        }

        private async Task Check(SYS_MESSAGE_TEMPLATE entity)
        {
            var isExists = await _dbContext.Query<SYS_MESSAGE_TEMPLATE>().Where(c => c.CODE == entity.CODE && c.ID != entity.ID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在消息模板{entity.CODE}");
        }
    }
}