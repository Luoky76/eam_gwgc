using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Server.Interfaces.Auth;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 角色服务
    /// </summary>
    public partial class RoleService : IRoleService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly UserSession CurrentUser;

        /// <summary>
        /// 角色服务
        /// </summary>
        public RoleService(IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
            CurrentUser = userSession;
        }

        /// <summary>
        /// 组织下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> CorpData()
        {
            var query = _dbContext.Query<CF_CORP>();
            if (!CurrentUser.IsAdmin)
            {
                var corpids = CurrentUser.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => corpids.Contains(c.CORPID));
            }
            return await query.Select(a => new ComboxData { ID = a.CORPID, TEXT = a.CNAME, VALUE = a.CORPID }).ToListAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName && Sql.IsNotEqual(c.ROLEID, _options.AdminRole));
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<CF_ROLE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.ROLEDESC, c.CORPID },
                c => a => a.ROLEID == c.ROLEID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(CF_ROLE entity)
        {
            var isExists = await _dbContext.Query<CF_ROLE>().Where(c => c.ROLENAME == entity.ROLENAME
           && c.APPNAME == _options.RoleAppName).AnyAsync();
            if (isExists) throw new MessageException($"已经存在角色{entity.ROLENAME}");
            if (!CurrentUser.IsAdmin)
            {
                if (!CurrentUser.AllCorps.Exists(c => c.CorpID == entity.CORPID)) throw new MessageException($"请维护角色所属组织");
            }
            entity.RECORDSTATUS = Oper.Add;
            entity.APPNAME = _options.RoleAppName;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(CF_ROLE entity)
        {
            var model = await _dbContext.Query<CF_ROLE>().Where(c => c.ROLEID == entity.ROLEID).FirstOrDefaultAsync()
                ?? throw new MessageException($"角色{entity.ROLENAME}已被删除");
            var isExists = await _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName
                && c.ROLENAME == entity.ROLENAME && c.ROLEID != entity.ROLEID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在角色{entity.ROLENAME}");
            if (!CurrentUser.IsAdmin)
            {
                if (!CurrentUser.AllCorps.Exists(c => c.CorpID == model.CORPID)) throw new MessageException($"您无权进行此操作");
                if (model.CORPID != entity.CORPID)
                {
                    if (!CurrentUser.AllCorps.Exists(c => c.CorpID == entity.CORPID)) throw new MessageException($"请维护角色所属组织");
                }
            }
            entity.RECORDSTATUS = Oper.Modify;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(CF_ROLE entity)
        {
            var model = await _dbContext.Query<CF_ROLE>().Where(c => c.ROLEID == entity.ROLEID).FirstOrDefaultAsync();
            if (!CurrentUser.IsAdmin)
            {
                if (!CurrentUser.AllCorps.Exists(c => c.CorpID == model.CORPID)) throw new MessageException($"您无权进行此操作");
            }
            var isExists = await _dbContext.Query<CF_USERROLE>().Where(c => c.ROLEID == entity.ROLEID).AnyAsync();
            if (isExists) throw new MessageException($"角色{entity.ROLENAME}下已有用户，无法删除");
            await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == CurrentUser.MenuAppname
                                                 && c.PRIVILEGEMASTER == "CF_ROLE"
                                                 && c.PRIVILEGEMASTERKEY == entity.ROLENAME);
            await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == _options.RoleAppName
                                               && c.PRIVILEGEMASTER == "CF_ROLE"
                                               && c.PRIVILEGEMASTERKEY == entity.ROLENAME);
            entity.RECORDSTATUS = Oper.Delete;
        }
    }
}