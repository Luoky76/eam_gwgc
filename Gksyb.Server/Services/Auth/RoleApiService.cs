using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.UI;
using System.Linq.Expressions;

namespace Gksyb.Server.Services.Auth
{
    public partial class RoleService
    {
        /// <inheritdoc/>
        public async Task<List<ComboxData>> ComboxDataAsync()
        {
            return await _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName && Sql.IsNotEqual(c.ROLEID, _options.AdminRole))
                .Select(c => new ComboxData()
                {
                    ID = c.ROLEID,
                    TEXT = c.ROLENAME,
                    VALUE = c.ROLEDESC
                }).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<RoleInfo>> AllRoles()
        {
            return await GetRoles();
        }

        /// <inheritdoc/>
        public async Task<List<RoleInfo>> FindRoles(string CorpId)
        {
            return await GetRoles(c => c.CORPID == CorpId);
        }

        /// <summary>
        /// 根据条件获取用户
        /// </summary>
        private async Task<List<RoleInfo>> GetRoles(Expression<Func<CF_CORP, bool>> filter = null)
        {
            var list = await _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName && Sql.IsNotEqual(c.ROLEID, _options.AdminRole))
                .LeftJoin<CF_CORP>((a, b) => a.CORPID == b.CORPID)
                .WhereEx(filter)
                .Select((a, b) => new { Role = a, Corp = b }).ToListAsync();
            return list.Select(c => new RoleInfo()
            {
                Id = c.Role.ROLEID,
                Name = c.Role.ROLENAME,
                Desc = c.Role.ROLEDESC,
                Corp = c.Corp?.ToCorpInfo()
            }).ToList();
        }
    }
}