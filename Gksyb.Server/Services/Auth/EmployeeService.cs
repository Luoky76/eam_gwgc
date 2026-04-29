using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Filter;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.Extensions.Options;
using System.Collections;

namespace Gksyb.Server.Services.Auth
{
    public class EmployeeService : IBaseService
    {
        private const string ROOT_KEY = "ROOT";
        private readonly IDbContext _dbContext;
        private readonly UserService _userService;
        private readonly UserSession _user;
        private readonly SysContextOptions _options;

        /// <summary>
        /// 组织服务
        /// </summary>
        public EmployeeService(UserService userService, IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _userService = userService;
            _dbContext = dbContext;
            _user = userSession;
            _options = options.Value;
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<IList> TreeAsync()
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
                data.Add(new { ID = ROOT_KEY, TEXT = "组织结构", PARENTID = "", CORP_PATH = "", CLASSFLAG = "", ICON = "fa fa-sitemap" });
            }
            return data;
        }

        /// <summary>
        /// 获取公司信息
        /// </summary>
        public async Task<List<ComboxData>> CorpsAsync(List<string> ids)
        {
            if (ids.Count < 1)
            {
                return new List<ComboxData>();
            }
            return await _dbContext.Query<CF_CORP>().Where(c => ids.Contains(c.CORPID))
                .Select(c => new ComboxData { ID = c.CORPID, TEXT = c.CNAME, VALUE = c.CORP_SNAME })
                .ToListAsync();
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<GridData<List<UserRequest>>> GetSameEmployeeAsync(UserRequest request)
        {
            var rules = ToRules(request);
            if (rules.Count < 1)
            {
                return null;
            }
            var gridRequest = new GridRequest()
            {
                EncrpyCondition = new FilterGroup()
                {
                    Op = "or",
                    Rules = rules
                }.ToJson(),
                ChangePage = "1"
            };
            //临时变更，失效userService默认过滤公司行为
            _user.IsAdmin = true;
            var gridData = await _userService.ListAsync(new UserRequest(), gridRequest);
            var rows = gridData.Rows as List<UserRequest>;
            var employees = new List<UserRequest>();
            foreach (var row in rows)
            {
                var corps = row.CORP.Split(",").ToList();
                if (_user.AllCorps.Any(c => corps.Contains(c.CorpID)))
                {
                    MessageException.ThrowIf(row.LOGINNAME == request.LOGINNAME, $"已经存在用户{request.LOGINNAME}");
                    continue;
                }
                employees.Add(row);
            }
            return new GridData<List<UserRequest>>()
            {
                Rows = employees,
                Total = employees.Count
            };
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

        /// <summary>
        /// 请求转查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static List<FilterRule> ToRules(UserRequest request)
        {
            var rules = new List<FilterRule>();
            if (!string.IsNullOrWhiteSpace(request.LOGINNAME))
            {
                rules.Add(new()
                {
                    Field = nameof(request.LOGINNAME),
                    Op = "equal",
                    Value = request.LOGINNAME
                });
            }
            if (!string.IsNullOrWhiteSpace(request.REALNAME))
            {
                rules.Add(new()
                {
                    Field = nameof(request.REALNAME),
                    Op = "equal",
                    Value = request.REALNAME
                });
            }
            if (!string.IsNullOrWhiteSpace(request.PHONE))
            {
                rules.Add(new()
                {
                    Field = nameof(request.PHONE),
                    Op = "equal",
                    Value = request.PHONE
                });
            }
            return rules;
        }
    }
}