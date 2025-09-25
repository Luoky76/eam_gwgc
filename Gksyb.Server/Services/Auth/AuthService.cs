using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;
using Gksyb.Model.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Gksyb.Server.Services.Auth
{
    public partial class AuthService : IAuthService, IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IRoleModuleService _roleModuleService;
        private readonly SysContextOptions _options;
        private const string _guestRole = "微信访客";
        private const string _opertype = "用户公司";
        private const string _roletype = "角色公司";

        public AuthService(IDbContext dbContext, IRoleModuleService roleModuleService, IOptions<SysContextOptions> sysContext)
        {
            _dbContext = dbContext;
            _roleModuleService = roleModuleService;
            _options = sysContext.Value;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> LoginAsync(LoginRequest request, Action<UserSession> action = null, bool checkPassword = true, Func<LoginResponse, Task<AjaxResult>> handle = null)
        {
            AjaxResult result = null;
            try
            {
                result = await LoginInnerAsync(request, action, checkPassword, handle);
            }
            catch (Exception ex)
            {
                result = AjaxResult.Error(ex.ToString());
                throw;
            }
            finally
            {
                if (string.IsNullOrWhiteSpace(request.Source)) request.Source = "用户登录";
                await _dbContext.UserLogAsync("用户登录", request.Source, (result.IsError ? $"失败：{result.Message}" : "成功"), new UserSession()
                {
                    UserName = request.Username,
                    IP = request.IP,
                    UserAgent = request.UserAgent,
                    MenuAppname = request.MenuAppname
                });
            }
            return result;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private async Task<AjaxResult> LoginInnerAsync(LoginRequest request, Action<UserSession> action = null, bool checkPassword = true, Func<LoginResponse, Task<AjaxResult>> handle = null)
        {
            request.RoleAppname = request.RoleAppname.HasValue() ? request.RoleAppname : _options.RoleAppName;
            request.MenuAppname = request.MenuAppname.HasValue() ? request.MenuAppname : _options.AppName;

            CF_USER user = await GetUserAsync(request.Username, request.Password);
            if (user == null) return AjaxResult.Error("账号密码错误");
            if (user.LOGINPASSWORD != request.Password) return AjaxResult.Error("账号密码错误");

            var lastChangeTime = user.SUPPLIERID == null ? (await _dbContext.GetSysdate()) : DateTime.UnixEpoch.AddSeconds(user.SUPPLIERID.CastTo<double>());
            var errorMsg = await CheckPassword(request.Username, request.InputPassword, lastChangeTime);
            if (checkPassword && !string.IsNullOrWhiteSpace(errorMsg)) return AjaxResult.Error(errorMsg, "1");

            var roles = await _dbContext.Query<CF_ROLE>()
                .InnerJoin<CF_USERROLE>((role, userrole) => role.ROLEID == userrole.ROLEID && userrole.USERID == user.USERID)
                .Select((role, userrole) => role)
                .Where(c => c.APPNAME == request.RoleAppname)
                .ToListAsync();

            var isSuper = user.USERID == _options.AdminUserID;
            var isDeveloper = isSuper || (user.USERID == _options.DeveloperID);
            var isAdmin = isSuper || roles.Contains(c => c.ROLEID == _options.AdminRole);
            var isOurCompany = isAdmin || ((user.CLASS ?? "0").CastTo(0) > 0);
            var ports = await GetUserPortsAsync(user.LOGINNAME, c => c.OPTYPE == _roletype);
            var userSession = new UserSession()
            {
                UserID = user.USERID.Value,
                UserName = user.LOGINNAME,
                RealName = user.REALNAME,
                Class = user.CLASS,
                WorkerCode = user.DEPARTCODE,
                Group = user.STATION ?? "",
                AllRoles = roles.Select(c => c.ROLENAME).Distinct().ToList(),
                RoleCorps = ToRoleCorps(roles, ports),
                IsSuper = isSuper,
                IsAdmin = isAdmin,
                IsOurCompany = isOurCompany,
                IsDeveloper = isDeveloper,
                IP = request.IP,
                UserAgent = request.UserAgent,
                UserAppName = _options.UserAppName,
                RoleAppName = request.RoleAppname,
                MenuAppname = request.MenuAppname,
                ForbinMenus = await GetForbidMenu(user.LOGINNAME),
                ForbinButtons = await GetForbidButtons(user.LOGINNAME)
            };
            if (isSuper) userSession.AllRoles.Add(UserSession.SuperRoleName);
            await LoginHandle(userSession, user);//当前系统登录的特殊处理
            action?.Invoke(userSession);
            //加入微信访客角色
            if (!string.IsNullOrWhiteSpace(userSession.Openid) && !userSession.AllRoles.Contains(_guestRole))
            {
                userSession.AllRoles.Add(_guestRole);
            }
            //登录成功，更新用户数据
            _dbContext.TrackEntity(user);
            user.LASTLOGINTIME = await _dbContext.GetSysdate();
            user.FAX = request.IP;
            user.ADDRESS = request.UserAgent;
            user.SUPPLIERID ??= (user.LASTLOGINTIME.Value - DateTime.UnixEpoch).TotalSeconds.CastTo<long>();
            await _dbContext.UpdateAsync(user);

            var userResponse = await userSession.SaveToTicketAsync();
            AjaxResult result = null;
            if (handle != null)
            {
                var lastImei = await _dbContext.Query<CF_USER_PORT>().Where(c => c.LOGINNAME == user.LOGINNAME && c.APPNAME == _options.UserAppName && c.REMARK == request.MenuAppname & c.OPTYPE == "IMEI")
                    .Select(c => c.CORPID).FirstOrDefaultAsync();
                var phone = (user.PHONE ?? "").Split(',').DistinctAndOrderBy()
                    .Where(c => c.IsMobileNumber()).Select(c => new KeyValueItem(c, $"{c[..3]}****{c[7..]}")).ToList();
                result = await handle(new LoginResponse()
                {
                    Account = userSession.UserName,
                    MenuAppname = request.MenuAppname,
                    IMEI = request.IMEI,
                    LastIMEI = lastImei,
                    IsAuth = _options.SmsAuth,
                    Phone = phone,
                    Response = userResponse
                });
            }
            return result ?? AjaxResult.Success(userResponse);
        }

        /// <inheritdoc/>
        public async Task SetUserImeiAsync(LoginResponse request)//string account, string imei
        {
            var now = await _dbContext.GetSysdate();
            var entity = new CF_USER_PORT()
            {
                LOGINNAME = request.Account,
                APPNAME = _options.UserAppName,
                REMARK = request.MenuAppname,
                OPTYPE = "IMEI",
                CREATEDATE = now
            };
            _dbContext.TrackEntity(entity);
            entity.CORPID = request.IMEI;
            entity.MODIFYDATE = now;
            await _dbContext.InsertOrUpdateAsync(entity, c => c.LOGINNAME == entity.LOGINNAME && c.APPNAME == entity.APPNAME && c.REMARK == entity.REMARK && c.OPTYPE == entity.OPTYPE);
        }

        /// <inheritdoc/>
        public async Task<CF_USER> GetUserAsync(string loginName, string password = null)
        {
            CF_USER user = null;
            if (loginName.IsMobileNumber())//手机号登录支持
            {
                var users = await _dbContext.Query<CF_USER>()
                .Where(c => c.PHONE == loginName && c.APPNAME == _options.UserAppName && c.FLAG == "1").ToListAsync();
                if (!string.IsNullOrWhiteSpace(password))
                {
                    users = users.Where(c => c.LOGINPASSWORD == password).ToList();
                }
                user = users.OrderBy(c => c.USERID).FirstOrDefault();
            }
            loginName = loginName?.ToUpper();
            user ??= await _dbContext.Query<CF_USER>()
                    .Where(c => c.LOGINNAME == loginName && c.APPNAME == _options.UserAppName && c.FLAG == "1").FirstOrDefaultAsync();
            return user;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<AjaxResult> ChangePasswordAsync(ChangePasswordRequest request)
        {
            if (request.OldPassword == request.NewPassword) return AjaxResult.Error("修改失败，密码不能与上次密码一样");
            request.OldPassword = UserSession.Encrypt(request.OldPassword);
            var user = await GetUserAsync(request.Username, request.OldPassword);
            if (user == null) return AjaxResult.Error("修改失败，请输入正确的账号密码");
            if (user.LOGINPASSWORD != request.OldPassword) return AjaxResult.Error("修改失败，请输入正确的账号密码");
            return await ResetPasswordAsync(request, user, "修改");
        }

        /// <inheritdoc/>
        public async Task<AjaxResult> ResetPasswordAsync(ChangePasswordRequest request, CF_USER user, string op = "重置")
        {
            var errorMsg = await CheckPassword(user.LOGINNAME, request.NewPassword);
            if (!string.IsNullOrWhiteSpace(errorMsg)) return AjaxResult.Error(errorMsg);
            request.NewPassword = UserSession.Encrypt(request.NewPassword);
            var ticks = ((await _dbContext.GetSysdate()).Value - DateTime.UnixEpoch).TotalSeconds.CastTo<long>();
            await _dbContext.UpdateAsync<CF_USER>(a => a.USERID == user.USERID, a => new CF_USER()
            {
                SUPPLIERID = ticks,
                LOGINPASSWORD = request.NewPassword
            });
            await _dbContext.UserLogAsync($"密码{op}", $"{user.LOGINNAME}密码{op}", $"{user.LOGINNAME}{op}自己的密码");
            var messageCenter = HttpContext.RequestServices.GetService<IMessageCenterService>();
            await messageCenter.SendAsync(new MessageInfo
            {
                Action = "ChangePassword",
                Receives = new List<string>() { user.LOGINNAME }
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<List<MenuModule>> MyMenusAsync(UserSession userSession, string appname)
        {
            if (userSession.IsAdmin)
            {
                var query = _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == appname);
                if (userSession.UserID != _options.AdminUserID)
                {
                    query = query.Where(c => c.ISVISIBLE == 1);
                }
                var list = await query.ToListAsync<MenuModule>();
                return list.OrderBy(c => c.MENUORDER).ToList();
            }
            else if (userSession.IsDeveloper)
            {
                var list = await _dbContext.Query<SYS_MENU>()
                    .Where(c => c.APPNAME == appname && c.ISVISIBLE == 0)
                    .ToListAsync<MenuModule>();
                await _roleModuleService.AddMissingParent(list);
                return list.OrderBy(c => c.MENUORDER).ToList();
            }
            var menus = new List<MenuModule>();
            foreach (var roleName in userSession.Roles)
            {
                var list = await _roleModuleService.GetMenuModule(roleName, appname);
                menus.AddRange(list);
            }
            if (userSession.ForbinMenus?.Count > 0)
            {
                menus.RemoveAll(c => userSession.ForbinMenus.Exists(m => c.MENUNO == m.MENUNO && c.APPNAME == m.APPNAME));
                await _roleModuleService.AddMissingParent(menus);
            }
            return menus.DistinctBy(c => new { c.MENUNO, c.APPNAME }).OrderBy(c => c.MENUORDER).ToList();
        }

        /// <summary>
        /// 获取按钮
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="menuNo"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        public async Task<List<ButtonModule>> MyButtonsAsync(UserSession userSession, string menuNo, string appname)
        {
            if (userSession.IsAdmin || userSession.IsDeveloper)
            {
                var query = _dbContext.Query<SYS_BUTTON>().Where(c => c.MENUNO == menuNo && c.APPNAME == appname);
                var list = await query.ToListAsync<ButtonModule>();
                return list.OrderBy(c => c.SEQNO).ToList();
            }
            var buttons = new List<ButtonModule>();
            foreach (var roleName in userSession.Roles)
            {
                var list = await _roleModuleService.GetButtonModule(roleName, appname, menuNo, GksybAuthorizeMode.Equal);
                buttons.AddRange(list);
            }
            var key = $"{menuNo}__{appname}";
            if (userSession.ForbinButtons?.ContainsKey(key) == true)
            {
                buttons.RemoveAll(c => userSession.ForbinButtons[key].Exists(m => c.MENUNO == m.MENUNO && c.BTNNO == m.BTNNO && c.APPNAME == m.APPNAME));
            }
            return buttons.DistinctBy(c => new { c.BTNNO, c.APPNAME, c.INITSTATUS }).OrderBy(c => c.SEQNO).ToList();
        }

        /// <summary>
        /// 获取密码
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<string> GetPasswordAsync(string username)
        {
            string password = null;
            if (username.IsMobileNumber())//手机号支持
            {
                password = await _dbContext.Query<CF_USER>()
                .Where(c => c.PHONE == username && c.APPNAME == _options.UserAppName && c.FLAG == "1")
                .OrderByDesc(c => c.LASTLOGINTIME)
                .Select(c => c.LOGINPASSWORD).FirstOrDefaultAsync();
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                password = await _dbContext.Query<CF_USER>()
                .Where(c => c.LOGINNAME == username && c.APPNAME == _options.UserAppName && c.FLAG == "1")
                .Select(c => c.LOGINPASSWORD).FirstOrDefaultAsync();
            }
            return password;
        }

        /// <summary>
        /// 切换公司
        /// </summary>
        /// <returns></returns>
        public async Task<List<CorpInfo>> UserCorps(UserSession user)
        {
            List<CF_CORP> corps = null;
            if (user.IsAdmin)
            {
                corps = await _dbContext.Query<CF_CORP>().Where(a => a.VALIDFLAG == "1").ToListAsync();
            }
            else
            {
                corps = await _dbContext.Query<CF_CORP>().InnerJoin<CF_USER_PORT>((a, b) => a.CORPID == b.CORPID && a.VALIDFLAG == "1"
                && b.APPNAME == user.UserAppName && b.LOGINNAME == user.UserName).Select((a, b) => a).ToListAsync();
            }
            return corps.Select(c => c.ToCorpInfo()).ToList();
        }

        /// <summary>
        /// 切换公司
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ChangeCorp(UserSession user, string corpid)
        {
            user.Corp = null;
            user.AllCorps = new List<CorpInfo>();
            user.Corps = new List<CorpInfo>();
            var userCorps = await _dbContext.Query<CF_USER_PORT>().Where(c => c.LOGINNAME == user.UserName && c.APPNAME == user.UserAppName && c.OPTYPE == _opertype).ToListAsync();
            if (!user.IsAdmin && userCorps.Count < 1) return true;
            var allCorps = await _dbContext.Query<CF_CORP>().Where(c => c.VALIDFLAG == "1").Select(CorpInfoExtensions.SelectCorpInfo).ToListAsync();
            var corps = user.IsAdmin ? allCorps : allCorps.Where(c => userCorps.Exists(a => a.CORPID == c.CorpID)).Select(a =>
            {
                a.Station = (userCorps.Find(c => c.CORPID == a.CorpID)?.REMARK ?? "").Split(",").DistinctAndOrderBy().ToList();
                return a;
            }).ToList();
            user.Corps = corps.OrderBy(c => c.CorpID).ToList();
            corps.ForEach(c =>
            {
                user.AllCorps.AddRange(c.ChildCorp(allCorps));
            });
            user.AllCorps = user.AllCorps.DistinctBy(c => c.CorpID).OrderBy(c => c.CorpID).ToList();
            if (corps == null || corps.Count < 1) return true;
            user.Corp = corps.FirstOrDefault(c => c.CorpID == corpid) ?? corps[0];
            user.ParentCompany = user.Corp.ClassFlag == CorpInfoExtensions.Company ? user.Corp : user.Corp.ParentCorp(allCorps, c => c.ClassFlag == CorpInfoExtensions.Company);
            await _dbContext.UpdateAsync<CF_USER>(a => a.USERID == user.UserID, a => new CF_USER()
            {
                C_TERMINAL = user.Corp.CorpID
            });
            return true;
        }

        /// <inheritdoc/>
        public async Task ExitAsync(UserSession user)
        {
            await _dbContext.UserLogAsync("用户退出", "用户退出", "成功");
        }

        /// <summary>
        /// 用户登录扩展处理
        /// </summary>
        /// <returns></returns>
        private async Task LoginHandle(UserSession userSession, CF_USER user)
        {
            await ChangeCorp(userSession, user.C_TERMINAL);
        }

        /// <summary>
        /// 判断密码是否符合规则
        /// </summary>
        /// <returns></returns>
        private async Task<string> CheckPassword(string username, string password, DateTime? lastChangeTime = null)
        {
            if (string.IsNullOrWhiteSpace(password)) return string.Empty;//不可删除，换token由于获取不到解密前的密码，忽略密码处理。
            if (!PasswordHelper.IsStrong(password ?? "", username))
            {
                return PasswordHelper.DirectionMsg;
            }
            if (lastChangeTime == null) return string.Empty;
            var sysdate = await _dbContext.GetSysdate();
            if (lastChangeTime.Value.AddDays(_options.GetPasswordExpiresIn) < sysdate)
            {
                return "密码已过期，请先修改";
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取用户扩展数据
        /// </summary>
        private async Task<List<CF_USER_PORT>> GetUserPortsAsync(string userName, Expression<Func<CF_USER_PORT, bool>> predicate)
        {
            return await _dbContext.Query<CF_USER_PORT>().Where(c => c.LOGINNAME == userName && c.APPNAME == _options.UserAppName)
                .WhereIfNotNull(predicate, predicate).ToListAsync();
        }

        /// <summary>
        /// 获取用户扩展数据
        /// </summary>
        private static List<KeyValueItem> ToRoleCorps(List<CF_ROLE> roles, List<CF_USER_PORT> ports)
        {
            var roleCorps = ports.SelectMany(c =>
            {
                var name = roles.Where(a => a.ROLEID.Value.ToString() == c.CORPID).Select(a => a.ROLENAME).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(name)) return new List<KeyValueItem>();
                var corps = (c.REMARK ?? "").Split(",").DistinctAndOrderBy().ToList();
                return corps.Select(a => new KeyValueItem(name, a)).ToList();
            }).ToList();
            roles.Where(c => !string.IsNullOrWhiteSpace(c.CORPID)).ForEach(a =>
            {
                if (ports.Any(c => a.ROLEID.Value.ToString() == c.CORPID))
                {
                    return;
                }
                roleCorps.Add(new KeyValueItem(a.ROLENAME, a.CORPID));
            });
            return roleCorps.Count < 1 ? null : roleCorps;
        }

        /// <summary>
        /// 获取用户禁止菜单
        /// </summary>
        private async Task<List<MenuModule>> GetForbidMenu(string userName)
        {
            var list = await _dbContext.Query<SYS_MENU>().Where(s => _dbContext.Query<CF_PRIVILEGE>().Where(c => c.PRIVILEGEMASTER == "CF_USER"
                                                   && c.PRIVILEGEACCESS == "SYS_MENU"
                                                   && c.PRIVILEGEOPERATION == "Forbid"
                                                   && c.PRIVILEGEMASTERKEY == userName
                                                   && Sql.IsEqual(c.APPNAME, s.APPNAME)
                                                   && Sql.IsEqual(c.PRIVILEGEACCESSKEY, s.MENUNO)).Any())
                                                  .ToListAsync<MenuModule>();
            return list.Count > 0 ? list : null;
        }

        /// <summary>
        /// 获取用户禁止按钮
        /// </summary>
        private async Task<SortedList<string, List<ButtonModule>>> GetForbidButtons(string userName)
        {
            var list = await _dbContext.Query<SYS_BUTTON>().Where(s => _dbContext.Query<CF_PRIVILEGE>().Where(c => c.PRIVILEGEMASTER == "CF_USER"
                                                  && c.PRIVILEGEACCESS == "SYS_BUTTON"
                                                  && c.PRIVILEGEOPERATION == "Forbid"
                                                  && c.PRIVILEGEMASTERKEY == userName
                                                  && Sql.IsEqual(c.APPNAME, s.APPNAME)
                                                  && Sql.IsEqual(c.PRIVILEGEACCESSKEY, (s.MENUNO + "*" + s.BTNNO))).Any())
                                                 .ToListAsync<ButtonModule>();
            var sortList = new SortedList<string, List<ButtonModule>>();
            list.GroupBy(c => $"{c.MENUNO}__{c.APPNAME}").ForEach((list) =>
            {
                sortList.Add(list.Key, list.ToList() ?? new List<ButtonModule>());
            });
            return sortList.Count > 0 ? sortList : null;
        }
    }
}