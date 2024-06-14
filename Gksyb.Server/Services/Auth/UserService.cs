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
        private const string _opertype = "用户公司";
        private const string _roletype = "角色公司";
        protected const string _weixinType = "微信";

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
            var query = _dbContext.Query<CF_USER>().Where(FilterSuper(_options))
                .WhereIf(!_user.IsOurCompany, c => (c.CLASS ?? "0") == "0");
            if (user.ROLE != null)
            {
                var roleid = user.ROLE.CastTo<long>();
                query = query.Where(c => _dbContext.Query<CF_USERROLE>().Where(a => a.USERID == c.USERID && a.ROLEID == roleid).Any());
            }
            query = FilterCorp(query, user.CORP);
            var data = await query.MapTo<UserRequest>().Exclude(a => new { a.LOGINPASSWORD, a.RECORDSTATUS }).GetGridData(request);
            var list = (data.Rows as List<UserRequest>) ?? new List<UserRequest>();
            var userRoles = await _dbContext.Query<CF_USERROLE>().Where(a => a.APPNAME == _options.RoleAppName).Select(c => new CF_USERROLE()
            {
                USERID = c.USERID,
                ROLEID = c.ROLEID
            }).ToListAsync();
            var userPorts = await _dbContext.Query<CF_USER_PORT>().Where(a => a.APPNAME == _options.UserAppName).ToListAsync();
            var hasWeixin = userPorts.Any(c => c.OPTYPE == _weixinType);
            foreach (var c in list)
            {
                var ports = userPorts.Where(a => a.LOGINNAME == c.LOGINNAME).ToList();
                var roles = userRoles.Where(a => a.USERID == c.USERID).Select(a => a.ROLEID).ToList();
                c.ROLE = roles.Join();
                var roleCorps = ports.Where(a => a.OPTYPE == _roletype).DistinctBy(c => c.CORPID).ToList();
                c.RoleCorp = roles.ToDictionary(c => c.Value.ToString(), c => roleCorps.Where(a => a.CORPID == c.Value.ToString()).Select(a => a.REMARK).FirstOrDefault());

                var corps = ports.Where(a => a.OPTYPE == _opertype).DistinctBy(c => c.CORPID).ToList();
                c.CORP = corps.Select(a => a.CORPID).Join();
                c.CorpStation = corps.ToDictionary(c => c.CORPID, c => c.REMARK);
                if (hasWeixin)
                {
                    c.QQ = ports.Where(a => a.OPTYPE == _weixinType).Select(a => a.CORPID).FirstOrDefault();
                }
                c.QQ = string.IsNullOrWhiteSpace(c.QQ) ? "0" : "1";
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
            var messages = new List<string>();
            var result = await _dbContext.SaveEntityAnsyc(request,
                c => new { c.REALNAME, c.PHONE, c.FLAG, c.DEPARTCODE, c.STATION, c.CLASS, c.USER_STATE, c.WORK_CODE },
                c => a => a.USERID == c.USERID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, async (added, updated, deleted) =>
                {
                    foreach (var entity in added)
                    {
                        var password = PasswordHelper.Generate();
                        entity.LOGINPASSWORD = password;
                        messages.Add($"{entity.LOGINNAME}的初始密码为{password}");
                    }
                    await Task.CompletedTask;
                }, AfterSave);
            if (!result.IsError) result.Data = messages;
            return result;
        }

        /// <summary>
        /// 密码初始化
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> DoInitPassword(long? id)
        {
            MessageException.ThrowIf(!id.HasValue, "请传递参数");
            var user = await _dbContext.Query<CF_USER>().Where(c => c.USERID == id.Value).Select(c => new CF_USER()
            {
                USERID = c.USERID,
                LOGINNAME = c.LOGINNAME,
                LOGINPASSWORD = c.LOGINPASSWORD
            }).FirstOrDefaultAsync();
            MessageException.ThrowIf(user == null, "找不到此用户");
            var initPassWord = PasswordHelper.Generate();
            _dbContext.TrackEntity(user);
            user.LOGINPASSWORD = UserSession.Encrypt(initPassWord);
            _dbContext.Update(user);
            await _dbContext.UserLogAsync("密码修改", $"{user.LOGINNAME}密码修改", $"{_user.UserName}初始化{user.LOGINNAME}的密码");
            return initPassWord;
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(UserRequest entity)
        {
            entity.CLASS ??= "0";
            MessageException.ThrowIf(!_user.IsOurCompany && entity.CLASS != "0", "您无权设置用户属性");
            await Handle(entity);

            entity.LOGINPASSWORD = UserSession.Encrypt(entity.LOGINPASSWORD);
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
            var old = await _dbContext.Query<CF_USER>().Where(c => c.USERID == entity.USERID).Select(c => new CF_USER() { CLASS = c.CLASS }).FirstOrDefaultAsync();
            MessageException.ThrowIf(!_user.IsOurCompany && ((entity.CLASS ?? "0") != (old.CLASS ?? "0")), "您无权设置用户属性");
            await Handle(entity);

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
            var old = await _dbContext.Query<CF_USER>().Where(c => c.USERID == entity.USERID).Select(c => new CF_USER() { CLASS = c.CLASS }).FirstOrDefaultAsync();
            MessageException.ThrowIf(!_user.IsOurCompany && (old.CLASS ?? "0") != "0", "您无权删除此用户");
            var corps = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.LOGINNAME == entity.LOGINNAME && c.OPTYPE == _opertype && c.APPNAME == _options.UserAppName).Select(c => c.CORPID).ToListAsync();
            if (!_user.IsAdmin && !(_user.IsOurCompany && (old.CLASS ?? "0") == "0"))//非管理员并且非内部用户操作外部用户
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
        /// 检查和预处理
        /// </summary>
        private async Task Handle(CF_USER entity)
        {
            var isExists = await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.LOGINNAME == entity.LOGINNAME)
                .WhereIfNotNull(entity.USERID, c => c.USERID != entity.USERID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在用户{entity.LOGINNAME}");
            //isExists = await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.REALNAME == entity.REALNAME)
            //    .WhereIfNotNull(entity.USERID, c => c.USERID != entity.USERID).AnyAsync();
            //if (isExists) throw new MessageException($"已经存在账号{entity.REALNAME}");
            //if (!string.IsNullOrWhiteSpace(entity.PHONE))
            //{
            //    isExists = await _dbContext.Query<CF_USER>().Where(c => c.APPNAME == _options.UserAppName && c.PHONE == entity.PHONE)
            //        .WhereIfNotNull(entity.USERID, c => c.USERID != entity.USERID).AnyAsync();
            //    if (isExists) throw new MessageException($"已经存在手机号{entity.PHONE}");
            //}
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

            //角色公司处理
            var roleCorps = entity.RoleCorp == null ? "" : roles.Where(c => entity.RoleCorp.ContainsKey(c.Value.ToString())).ToStr(",");
            await UserPortHandle(entity.LOGINNAME, roleCorps, _roletype, null, id =>
            {
                if (entity.RoleCorp?.ContainsKey(id) == true)
                {
                    var corps = (entity.RoleCorp[id] ?? "").Split(",").DistinctAndOrderBy().ToList();
                    if (corps.Count < 1) return null;
                    corps.RemoveAll(c => !_user.AllCorps.Exists(a => a.CorpID == c));
                    return corps.ToStr(",");
                }
                return null;
            });

            var oldRoles = (await _dbContext.Query<CF_USERROLE>().Where(condition).Select(c => c.ROLEID).ToListAsync()).Join();
            if (roles.ToStr(",") == oldRoles) return;
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
                return entity.CorpStation?.ContainsKey(corpid) == true ? entity.CorpStation[corpid] : null;
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

        private readonly string[] _optypes = new string[] { _opertype, _roletype };

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
        private Expression<Func<CF_USER, bool>> FilterSuper(SysContextOptions options)
        {
            if (_hasSuper) return c => c.APPNAME == options.UserAppName;
            return c => c.APPNAME == options.UserAppName && Sql.IsNotEqual(c.USERID, options.AdminUserID);
        }

        /// <summary>
        /// 包含超管
        /// </summary>
        private bool _hasSuper = false;
    }
}