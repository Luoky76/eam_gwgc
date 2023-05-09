using Chloe.Extensions;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public partial class UserService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;
        private readonly SysContextOptions _options;
        private DateTime? sysdate;
        private static readonly string _opertype = "用户公司";

        /// <summary>
        /// 用户服务
        /// </summary>
        public UserService(IDbContext dbContext, UserSession userSession, IOptions<SysContextOptions> options)
        {
            _dbContext = dbContext;
            _user = userSession;
            _options = options.Value;
        }

        /// <summary>
        /// 角色下拉
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> RoleData(GridRequest request = null)
        {
            var query = _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == _options.RoleAppName);
            if (!_user.IsAdmin)
            {
                var corpids = _user.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => Sql.IsNotEqual(c.ROLEID, _options.AdminRole) && (corpids.Contains(c.CORPID) || c.CORPID == null));
            }
            return await query.Select(c => new ComboxData { ID = c.ROLEID, TEXT = c.ROLENAME, VALUE = c.RECORDSTATUS })
                .GetGridData(request);
        }

        /// <summary>
        /// 公司下拉
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> CorpData(GridRequest request = null)
        {
            var query = _dbContext.Query<CF_CORP>();
            if (!_user.IsAdmin)
            {
                var corpids = _user.AllCorps.Select(c => c.CorpID).ToList();
                query = query.Where(c => corpids.Contains(c.CORPID));
            }
            return await query.Select(c => new { ID = c.CORPID, TEXT = c.CORP_SNAME, VALUE = c.VALIDFLAG, c.CLASSFLAG })
                .GetGridData(request);
        }

        /// <summary>
        /// 获取初始化密码
        /// </summary>
        /// <returns></returns>
        public string GetInitPassword()
        {
            return _options.InitPassWord;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(UserRequest user, GridRequest request)
        {
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options));
            if (user.ROLE != null)
            {
                var roleid = user.ROLE.CastTo<long>();
                query = query.Where(c => _dbContext.Query<CF_USERROLE>().Where(a => a.USERID == c.USERID && a.ROLEID == roleid).Any());
            }
            query = FilterCorp(query, user.CORP);
            var data = await query.MapTo<UserRequest>().Ignore(a => new { a.LOGINPASSWORD, a.RECORDSTATUS }).GetGridData(request);
            var list = (data.Rows as List<UserRequest>) ?? new List<UserRequest>();
            var userRoles = await _dbContext.Query<CF_USERROLE>().Where(a => a.APPNAME == _options.RoleAppName).Select(c => new CF_USERROLE()
            {
                USERID = c.USERID,
                ROLEID = c.ROLEID
            }).ToListAsync();
            var userPorts = await _dbContext.Query<CF_USER_PORT>().Where(a => a.APPNAME == _options.UserAppName).ToListAsync();
            foreach (var c in list)
            {
                var ports = userPorts.Where(a => a.LOGINNAME == c.LOGINNAME).ToList();
                c.ROLE = userRoles.Where(a => a.USERID == c.USERID).Select(a => a.ROLEID).Join();
                var corps = ports.Where(a => a.OPTYPE == _opertype).DistinctBy(c => c.CORPID).ToList();
                c.CORP = corps.Select(a => a.CORPID).Join();
                c.CorpStation = corps.ToDictionary(c => c.CORPID, c => c.REMARK);
            }
            return data;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<UserRequest> request)
        {
            sysdate = await _dbContext.GetSysdate();
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.REALNAME, c.TITLE, c.SEX, c.PHONE, c.FAX, c.EMAIL, c.QQ, c.NICKNAME, c.ADDRESS, c.FLAG, c.DEPARTCODE, c.STATION, c.CLASS },
                c => a => a.USERID == c.USERID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 密码初始化
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DoInitPassword(long? id)
        {
            if (!id.HasValue) return AjaxResult.Error("请传递参数");
            var initPassWord = UserSession.Encrypt(_options.InitPassWord);
            var row = await _dbContext.UpdateAsync<CF_USER>(c => c.USERID == id.Value, c => new CF_USER()
            {
                LOGINPASSWORD = initPassWord
            });
            if (row < 1) return AjaxResult.Error("找不到此用户");
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(UserRequest entity)
        {
            if (await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.LOGINNAME == entity.LOGINNAME).AnyAsync())
                throw new MessageException($"已经存在用户{entity.LOGINNAME}");
            if (await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.REALNAME == entity.REALNAME).AnyAsync())
                throw new MessageException($"已经存在用户名{entity.REALNAME}");
            entity.LOGINPASSWORD = UserSession.Encrypt(_options.InitPassWord);
            entity.RECORDSTATUS = Oper.Add;
            entity.APPNAME = _options.UserAppName;

            await CorpHandle(entity);
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(UserRequest entity)
        {
            if (await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.LOGINNAME == entity.LOGINNAME && c.USERID != entity.USERID).AnyAsync())
                throw new MessageException($"已经存在用户{entity.LOGINNAME}");
            if (await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.REALNAME == entity.REALNAME && c.USERID != entity.USERID).AnyAsync())
                throw new MessageException($"已经存在用户名{entity.REALNAME}");
            entity.RECORDSTATUS = Oper.Modify;
            await RoleHandle(entity);
            await CorpHandle(entity);
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(UserRequest entity)
        {
            var corps = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.LOGINNAME == entity.LOGINNAME && c.OPTYPE == _opertype && c.APPNAME == _options.UserAppName).Select(c => c.CORPID).ToListAsync();
            if (!_user.IsAdmin)
            {
                if (corps.Count < 1 || corps.Exists(c => !_user.AllCorps.Exists(a => a.CorpID == c)))
                    throw new MessageException($"用户{entity.LOGINNAME}所属组织与您当前的组织不匹配，您无权进行此操作");
            }
            await _dbContext.DeleteAsync<CF_USERROLE>(c => c.USERID == entity.USERID && c.APPNAME == _options.RoleAppName);
            await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == _options.AppName && c.PRIVILEGEMASTER == "CF_USER" && c.PRIVILEGEMASTERKEY == entity.LOGINNAME);
            await _dbContext.DeleteAsync<CF_USER_PORT>(c => c.LOGINNAME == entity.LOGINNAME && c.APPNAME == _options.UserAppName && _optypes.Contains(c.OPTYPE));
        }

        private async Task AfterSave(List<UserRequest> added, List<UserRequest> updated, List<UserRequest> deleted)
        {
            foreach (var entity in added)
            {
                entity.USERID = await _dbContext.Query<CF_USER>().Where(c => c.LOGINNAME == entity.LOGINNAME &&
                c.APPNAME == entity.APPNAME).Select(c => c.USERID).FirstAsync();
                await RoleHandle(entity);
            }
        }

        /// <summary>
        /// 角色处理
        /// </summary>
        /// <returns></returns>
        private async Task RoleHandle(UserRequest entity)
        {
            var corpids = _user.AllCorps.Select(c => c.CorpID).ToList();
            var roleids = (entity.ROLE ?? "").Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Select(c => c.CastTo<long?>()).OrderBy(i => i).ToList();
            Expression<Func<CF_ROLE, bool>> roleCondition = c => c.APPNAME == _options.RoleAppName && roleids.Contains(c.ROLEID);
            Expression<Func<CF_USERROLE, bool>> condition = c => c.USERID == entity.USERID && c.APPNAME == _options.RoleAppName;
            if (!_user.IsAdmin)//非管理员去除非所属公司
            {
                roleCondition = (Expression<Func<CF_ROLE, bool>>)roleCondition.And((Expression<Func<CF_ROLE, bool>>)(c => c.CORPID == null || corpids.Contains(c.CORPID)));
                Expression<Func<CF_USERROLE, bool>> conditionAppend = c => _dbContext.Query<CF_ROLE>(a => a.ROLEID == c.ROLEID && (a.CORPID == null || corpids.Contains(a.CORPID))).Any();
                condition = (Expression<Func<CF_USERROLE, bool>>)condition.And(conditionAppend);
            }
            var roles = await _dbContext.Query<CF_ROLE>().Where(roleCondition).Select(c => c.ROLEID).ToListAsync();
            roles = roles.Distinct().OrderBy(i => i).ToList();
            var oldRoles = (await _dbContext.Query<CF_USERROLE>().Where(condition).Select(c => c.ROLEID).ToListAsync()).Join();
            entity.ADDRESS = roles.ToStr(",");
            if (entity.ADDRESS == oldRoles) return;
            await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == _options.AppName && c.PRIVILEGEMASTER == "CF_USER" && c.PRIVILEGEMASTERKEY == entity.LOGINNAME);
            await _dbContext.DeleteAsync(condition);
            foreach (var roleid in roles)
            {
                await _dbContext.InsertAsync(new CF_USERROLE()
                {
                    USERID = entity.USERID.Value,
                    ROLEID = roleid,
                    CREATEUSER = _user.UserName,
                    CREATEDATE = sysdate,
                    APPNAME = _options.RoleAppName,
                    RECORDSTATUS = Oper.Add
                });
            }
        }

        /// <summary>
        /// 公司处理
        /// </summary>
        /// <returns></returns>
        private async Task CorpHandle(UserRequest entity)
        {
            await UserPortHandle(entity.LOGINNAME, entity.CORP, null, (condition, newIds) =>
            {
                if (_user.IsAdmin) return;
                var userIds = _user.AllCorps.Select(c => c.CorpID).ToList();
                newIds.RemoveAll(c => !userIds.Exists(a => a == c));
                Expression<Func<CF_USER_PORT, bool>> conditionAppend = c => userIds.Contains(c.CORPID);
                condition = (Expression<Func<CF_USER_PORT, bool>>)condition.And(conditionAppend);
            }, corpid =>
            {
                return entity.CorpStation.ContainsKey(corpid) ? entity.CorpStation[corpid] : null;
            });
        }

        /// <summary>
        /// 用户对应表处理
        /// </summary>
        /// <returns></returns>
        private async Task UserPortHandle(string loginName, string ports, string optype = null, Action<Expression<Func<CF_USER_PORT, bool>>, List<string>> action = null, Func<string, string> remarkHandle = null)
        {
            if (string.IsNullOrWhiteSpace(optype)) optype = _opertype;
            var newIds = (ports ?? "").Split(',').DistinctAndOrderBy().ToList();
            Expression<Func<CF_USER_PORT, bool>> condition = c => c.LOGINNAME == loginName && c.OPTYPE == optype && c.APPNAME == _options.UserAppName;
            action?.Invoke(condition, newIds);
            var oldIds = (await _dbContext.Query<CF_USER_PORT>().Where(condition).ToListAsync()).Select(c => $"{c.CORPID}{c.REMARK}").Join();
            var userPorts = new List<CF_USER_PORT>();
            foreach (var data in newIds)
            {
                userPorts.Add(new CF_USER_PORT()
                {
                    CORPID = data,
                    REMARK = remarkHandle?.Invoke(data)
                });
            }
            if (userPorts.Select(c => $"{c.CORPID}{c.REMARK}").Join() == oldIds) return;
            await _dbContext.DeleteAsync(condition);
            foreach (var userPort in userPorts)
            {
                var port = new CF_USER_PORT()
                {
                    LOGINNAME = loginName,
                    OPTYPE = optype,
                    CORPID = userPort.CORPID,
                    REMARK = userPort.REMARK,
                    APPNAME = _options.UserAppName
                };
                await _dbContext.InsertAsync(port);
            }
        }

        private readonly string[] _optypes = new string[] { _opertype };

        /// <summary>
        /// 公司过滤
        /// </summary>
        private IQuery<CF_USER> FilterCorp(IQuery<CF_USER> query, string corp = null)
        {
            if (_user.IsAdmin)
            {
                if (string.IsNullOrWhiteSpace(corp)) return query;
                query = query.Where(c => _dbContext.Query<CF_USER_PORT>().Where(a => a.LOGINNAME == c.LOGINNAME
                && a.APPNAME == c.APPNAME && a.OPTYPE == _opertype && a.CORPID == corp).Any());
                return query;
            }
            var corpids = _user.AllCorps.Select(c => c.CorpID).ToList();
            if (!string.IsNullOrWhiteSpace(corp))
            {
                query = query.Where(c => _dbContext.Query<CF_USER_PORT>().Where(a => a.LOGINNAME == c.LOGINNAME
                && a.APPNAME == c.APPNAME && a.OPTYPE == _opertype && corpids.Contains(a.CORPID) && a.CORPID == corp).Any());
            }
            else
            {
                query = query.Where(c => !_dbContext.Query<CF_USER_PORT>().Where(a => a.LOGINNAME == c.LOGINNAME
                && a.APPNAME == c.APPNAME && a.OPTYPE == _opertype).Any()
                || _dbContext.Query<CF_USER_PORT>().Where(a => a.LOGINNAME == c.LOGINNAME
                && a.APPNAME == c.APPNAME && a.OPTYPE == _opertype && corpids.Contains(a.CORPID)).Any());
            }
            return query;
        }

        /// <summary>
        /// 过滤超管
        /// </summary>
        private static Expression<Func<CF_USER, bool>> FilterSuper(SysContextOptions options) =>
            c => c.APPNAME == options.UserAppName && Sql.IsNotEqual(c.USERID, options.AdminUserID);
    }
}