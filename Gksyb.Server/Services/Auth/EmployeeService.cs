using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class EmployeeService : IBaseService
    {
        private const string ROOT_KEY = "ROOT";
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;
        private readonly SysContextOptions _options;
        /// <summary>
        /// 组织服务
        /// </summary>
        public EmployeeService(IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _user = userSession;
            _options = options.Value;
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await CoprQuery().OrderBy(c => c.CORP_SORT).ThenBy(c => c.CORPID).ToListAsync();
            var data = list.Select(c => new
            {
                ID = c.CORPID,
                TEXT = c.CORP_SNAME,
                PARENTID = (string.IsNullOrWhiteSpace(c.CORPPARENTID) || c.CORPPARENTID == "0") ? ROOT_KEY : c.CORPPARENTID,
                c.CORP_PATH,
                c.CLASSFLAG,
                ICON = "fa fa-group"
            }).ToList();
            if (data.Where(c => c.PARENTID == ROOT_KEY).Take(2).Count() == 2)
            {
                data.Add(new { ID = "ROOT", TEXT = "组织结构", PARENTID = "", CORP_PATH = "", CLASSFLAG = "", ICON = "fa fa-sitemap" });
            }
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 将用户所属的父公司对应的子公司加入公司列表
        /// </summary>
        /// <returns></returns>
        public async Task AddParentCompanys()
        {
            if (_user.IsAdmin || _user.ParentCompany == null || _user.ParentCompany?.CorpID == _user.Corp.CorpID)
            {
                return;
            }
            var allCorps = await CoprQuery().Select(CorpInfoExtensions.SelectCorpInfo).ToListAsync();
            _user.AllCorps.Add(_user.ParentCompany);
            _user.AllCorps.AddRange(_user.ParentCompany.ChildCorp(allCorps));
            _user.AllCorps = _user.AllCorps.DistinctBy(c => c.CorpID).OrderBy(c => c.CorpID).ToList();
        }

        /// <summary>
        /// 过滤父公司的数据
        /// </summary>
        private IQuery<CF_CORP> CoprQuery()
        {
            var query = _dbContext.Query<CF_CORP>().Where(c => c.VALIDFLAG == "1");
            if (!_user.IsAdmin)
            {
                var treeNode = _user.ParentCompany?.TreeNode;
                query = query.Where(c => c.CORP_PATH.StartsWith(treeNode));
            }
            return query;
        }

    }
}
