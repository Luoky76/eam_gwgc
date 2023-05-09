using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Model.XXX.Business;
using Microsoft.Extensions.Options;
using XXX.Business.Interfaces.Sample;

namespace XXX.Business.Services.Sample
{
    public class SingleService : ISingleService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly UserSession CurrentUser;

        /// <summary>
        /// 服务
        /// </summary>
        public SingleService(IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
            CurrentUser = userSession;
        }

        /// <summary>
        /// 角色下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> RoleData()
        {
            var query = _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName);
            if (!CurrentUser.IsAdmin)
            {
                var corpids = CurrentUser.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => Sql.IsNotEqual(c.ROLEID, _options.AdminRole) && (corpids.Contains(c.CORPID) || c.CORPID == null));
            }
            return await query.Select(c => new ComboxData { ID = c.ROLEID, TEXT = c.ROLENAME, VALUE = c.RECORDSTATUS }).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<SAMPLE_TABLE> GetAsync(string id)
        {
            return await _dbContext.QueryByKeyAsync<SAMPLE_TABLE>(id);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = _dbContext.Query<SAMPLE_TABLE>();
            if (!CurrentUser.IsAdmin)
            {
                var corpids = CurrentUser.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => corpids.Contains(c.CORPID));
            }
            return await query.GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SAMPLE_TABLE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.STRING_COLUMN, c.INT_COLUMN, c.FLOAT_COLUMN, c.DATE_COLUMN, c.COMB_COLUMN, c.DIFF_COMB_COLUMN, c.CORPID },
                c => a => a.SID == c.SID
                , BeforeAdd, BeforeUpdate, BeforeDelete, true);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(SAMPLE_TABLE entity)
        {
            var isExists = await _dbContext.Query<SAMPLE_TABLE>().Where(c => c.STRING_COLUMN == entity.STRING_COLUMN).AnyAsync();
            if (isExists) throw new MessageException($"已经存在名称{entity.STRING_COLUMN}");
            entity.SID = GuidHelper.NewShortId();
            entity.RECORDSTATUS = Oper.Add;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SAMPLE_TABLE entity)
        {
            var model = await _dbContext.Query<SAMPLE_TABLE>().Where(c => c.SID == entity.SID).FirstOrDefaultAsync() ?? throw new MessageException($"名称{entity.STRING_COLUMN}已被删除");
            var isExists = await _dbContext.Query<SAMPLE_TABLE>().Where(c => c.STRING_COLUMN == entity.STRING_COLUMN && c.SID != entity.SID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在名称{entity.STRING_COLUMN}");
            entity.RECORDSTATUS = Oper.Modify;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SAMPLE_TABLE entity)
        {
            await Task.CompletedTask;
            entity.RECORDSTATUS = Oper.Delete;
        }
    }
}