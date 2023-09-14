using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Grid;
using Gksyb.Model.WorkFlow;
using Microsoft.Extensions.Options;

namespace Gksyb.Workflow.Services.Workflow
{
    /// <summary>
    /// 流程定义
    /// </summary>
    public class DefinitionService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly UserSession _user;

        public DefinitionService(IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _user = userSession;
        }

        /// <summary>
        /// 获取流程定义
        /// </summary>
        /// <returns></returns>
        public async Task<WF_FLOW> GetAsync(string id)
        {
            var model = await _dbContext.Query<WF_FLOW>().Where(c => c.ID == id).FirstOrDefaultAsync();
            return model;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = _dbContext.Query<WF_FLOW>().Where(c => c.APPNAME == _options.AppName);
            if (!_user.IsAdmin)
            {
                var corpids = _user.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => corpids.Contains(c.CORPID));
            }
            return await query.Exclude(c => new { c.FLOW_CONTENT, c.FLOW_FORM, c.FLOW_FORM_URL, c.FLOW_FORM_MOBILE_URL }).GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task SaveOrderAsync(List<WF_FLOW> updates)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var update in updates)
                {
                    await _dbContext.UpdateAsync<WF_FLOW>(c => c.ID == update.ID, c => new WF_FLOW()
                    {
                        FLOW_ORDER = update.FLOW_ORDER,
                        MODIFYUSERID = _user.UserID,
                        MODIFYUSER = _user.Display,
                        MODIFYDATE = DateTime.Now
                    });
                }
            });
        }

        /// <summary>
        /// 复制
        /// </summary>
        public async Task CopyAsync(List<string> ids, List<string> corps)
        {
            var sysdate = await _dbContext.GetSysdate();
            var list = await _dbContext.Query<WF_FLOW>().Where(c => ids.Contains(c.ID)).ToListAsync();
            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var id in ids)
                {
                    var model = list.Find(c => c.ID == id);
                    if (model == null) continue;
                    foreach (var corp in corps)
                    {
                        model.ID = GuidHelper.NewShortId();
                        model.CORPID = corp;
                        model.MODIFYUSERID = _user.UserID;
                        model.MODIFYUSER = _user.Display;
                        model.MODIFYDATE = sysdate;
                        await _dbContext.InsertAsync(model);
                    }
                }
            });
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<WF_FLOW> request)
        {
            var result = await _dbContext.SaveEntityAnsyc(request,
                c => new { c.FLOW_NAME, c.FLOW_GROUP, c.FLOW_TITLE, c.FLOW_ORDER, c.FLOW_CONTENT, c.FLOW_FORM, c.FLOW_FORM_URL, c.FLOW_FORM_MOBILE_URL, c.PASSIVE, c.CORPID },
                c => a => a.ID == c.ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, true, BeforeSave, AfterSave);
            if (result.IsError) return result;
            return AjaxResult.Success(request.Added);
        }

        /// <summary>
        /// 保存前
        /// </summary>
        private async Task BeforeSave(List<WF_FLOW> added, List<WF_FLOW> updated, List<WF_FLOW> deleted)
        {
            for (var i = updated.Count - 1; i >= 0; i--)
            {
                var entity = updated[i];
                var isExists = await _dbContext.Query<WF_TASK>().Where(c => c.FLOW_ID == entity.ID).AnyAsync() ||
                    await _dbContext.Query<WF_HISTORY_TASK>().Where(c => c.FLOW_ID == entity.ID).AnyAsync();
                if (!isExists) continue;
                added.Add(entity.MapTo<WF_FLOW>());
                updated.Remove(entity);
                deleted.Add(entity);
            }
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(WF_FLOW entity)
        {
            await Handle(entity);
            if (!_user.IsAdmin)
            {
                if (!_user.AllCorps.Exists(c => c.CorpID == entity.CORPID)) throw new MessageException($"请维护流程所属组织");
            }
            entity.ID = GuidHelper.NewShortId();
            entity.FLAG = "1";
            entity.PASSIVE = entity.PASSIVE == "1" ? "1" : "0";
            entity.FLOW_VERSION = (entity.FLOW_VERSION ?? 0) + 1;
            entity.APPNAME = _options.AppName;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(WF_FLOW entity)
        {
            await Handle(entity);
            var model = await _dbContext.Query<WF_FLOW>().Where(c => c.ID == entity.ID).FirstOrDefaultAsync()
                ?? throw new MessageException($"找不到流程{entity.FLOW_NAME}");
            if (!_user.IsAdmin)
            {
                if (!_user.AllCorps.Exists(c => c.CorpID == model.CORPID)) throw new MessageException($"您无权进行此操作");
                if (model.CORPID != entity.CORPID)
                {
                    if (!_user.AllCorps.Exists(c => c.CorpID == entity.CORPID)) throw new MessageException($"请维护流程所属组织");
                }
            }
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(WF_FLOW entity)
        {
            var model = await _dbContext.Query<WF_FLOW>().Where(c => c.ID == entity.ID).Exclude(c => new { c.FLOW_CONTENT, c.FLOW_FORM }).FirstOrDefaultAsync();
            if (!_user.IsAdmin)
            {
                if (!_user.AllCorps.Exists(c => c.CorpID == model.CORPID)) throw new MessageException($"您无权进行此操作");
            }
            entity.FLAG = "0";
        }

        /// <summary>
        /// 保存后
        /// </summary>
        private async Task AfterSave(List<WF_FLOW> added, List<WF_FLOW> updated, List<WF_FLOW> deleted)
        {
            for (var i = deleted.Count - 1; i >= 0; i--)
            {
                var entity = deleted[i];
                var isExists = await _dbContext.Query<WF_TASK>().Where(c => c.FLOW_ID == entity.ID).AnyAsync() ||
                    await _dbContext.Query<WF_HISTORY_TASK>().Where(c => c.FLOW_ID == entity.ID).AnyAsync();
                if (isExists) continue;
                await _dbContext.DeleteAsync(entity);
            }
        }

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task Handle(WF_FLOW entity)
        {
            var isExists = await _dbContext.Query<WF_FLOW>()
                .Where(c => c.FLOW_NAME == entity.FLOW_NAME && c.CORPID == entity.CORPID && c.FLAG == "1" && c.APPNAME == _options.AppName)
                .WhereIfNotNullOrEmpty(entity.ID, c => c.ID != entity.ID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在流程名称{entity.FLOW_NAME}");
        }
    }
}