using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    /// <summary>
    /// 权限服务
    /// </summary>
    public class PrivilegeService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly SysContextOptions _options;
        private readonly UserSession _user;
        private readonly IRoleModuleService _roleModuleService;

        /// <summary>
        /// 角色服务
        /// </summary>
        public PrivilegeService(IDbContext dbContext, IOptions<SysContextOptions> options
            , UserSession user, IRoleModuleService roleModuleService)
        {
            _dbContext = dbContext;
            _options = options.Value;
            _user = user;
            _roleModuleService = roleModuleService;
        }

        /// <summary>
        /// 获取菜单及按钮
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> MenuButtonAsync(string appName)
        {
            if (!_user.IsAdmin)
            {
                return await UserMenuButtonAsync(_user.UserID, appName);
            }
            if (string.IsNullOrEmpty(appName)) appName = _options.AppName;
            var menus = await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == appName && c.ISVISIBLE == 1).OrderBy(c => c.MENUPARENTNO).ThenBy(c => c.MENUORDER)
                .Select(c => new { ID = c.MENUNO, PID = c.MENUPARENTNO ?? "", ACCESSNAME = c.MENUNAME, ACCESSICON = c.MENUICON, ACCESSNO = c.MENUNO, c.MENUID, BTNID = (long?)0, c.MENUORDER }).ToListAsync();
            var buttons = await _dbContext.Query<SYS_BUTTON>().Where(c => c.APPNAME == appName).OrderBy(c => c.MENUNO).ThenBy(c => c.SEQNO)
                .Select(c => new { ID = c.MENUNO + "*" + c.BTNNO, PID = c.MENUNO, ACCESSNAME = c.BTNNAME, ACCESSICON = c.BTNICON, ACCESSNO = c.MENUNO + "*" + c.BTNNO, MENUID = (long?)0, c.BTNID, MENUORDER = c.SEQNO }).ToListAsync();
            buttons.RemoveAll(c => !menus.Contains(a => a.ID == c.PID));
            menus.AddRange(buttons);
            menus = menus.OrderBy(c => c.PID).ThenBy(c => c.MENUORDER).ToList();
            return AjaxResult.Success(menus);
        }

        /// <summary>
        /// 获取用户菜单及按钮
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> UserMenuButtonAsync(long userId, string appName)
        {
            if (string.IsNullOrEmpty(appName)) appName = _options.AppName;

            var list = await _dbContext.Query<SYS_MENU>().Where(c => c.APPNAME == appName && c.ISVISIBLE == 1)
                .Select(c => new { ID = c.MENUNO, PID = c.MENUPARENTNO ?? "", ACCESSNAME = c.MENUNAME, ACCESSICON = c.MENUICON, ACCESSNO = c.MENUNO, c.MENUID, BTNID = (long?)0, c.MENUORDER }).ToListAsync();
            var menunos = await _dbContext.Query<CF_PRIVILEGE>().Where(a => a.PRIVILEGEACCESS == "SYS_MENU"
            && a.PRIVILEGEOPERATION == "Permit" && a.PRIVILEGEMASTER == "CF_ROLE" && a.APPNAME == appName
            && _dbContext.Query<CF_ROLE>().Where(d => d.ROLENAME == a.PRIVILEGEMASTERKEY).InnerJoin<CF_USERROLE>((d, e) => d.ROLEID == e.ROLEID
                                                                            && e.USERID == userId
                                                                            && d.APPNAME == _options.RoleAppName).Select((d, e) => d.ROLEID).Any()).Select(c => c.PRIVILEGEACCESSKEY).ToListAsync();
            var menus = list.Where(c => menunos.Any(a => a == c.ID)).ToList();
            var parents = menus.Where(c => !string.IsNullOrWhiteSpace(c.PID) && !menus.Any(a => a.ID == c.PID)).ToList();
            if (parents.Count > 0)
            {
                void recursion(string menuno)
                {
                    var menu = list.Find(c => c.ID == menuno);
                    if (menu == null) return;
                    menus.Add(menu);
                    if (string.IsNullOrWhiteSpace(menu.PID)) return;
                    recursion(menu.PID);
                }
                parents.ForEach(c =>
                {
                    recursion(c.PID);
                });
                menus = menus.DistinctBy(c => new { c.ID }).ToList();
            }
            menus = menus.Where(c => menus.Any(a => a.PID == c.ID) || !list.Any(a => a.PID == c.ID)).ToList();

            var queryButtons = _dbContext.Query<SYS_BUTTON>().Where(c => c.APPNAME == appName
            && _dbContext.Query<CF_PRIVILEGE>().Where(a => a.PRIVILEGEACCESSKEY == c.MENUNO + "*" + c.BTNNO && a.PRIVILEGEACCESS == "SYS_BUTTON"
            && a.PRIVILEGEOPERATION == "Permit" && a.PRIVILEGEMASTER == "CF_ROLE" && a.APPNAME == appName
            && _dbContext.Query<CF_ROLE>().Where(d => d.ROLENAME == a.PRIVILEGEMASTERKEY).InnerJoin<CF_USERROLE>((d, e) => d.ROLEID == e.ROLEID
                                                                            && e.USERID == userId
                                                                            && d.APPNAME == _options.RoleAppName).Select((d, e) => d.ROLEID).Any()).Any());

            var buttons = await queryButtons.OrderBy(c => c.MENUNO).ThenBy(c => c.SEQNO)
                .Select(c => new { ID = c.BTNNO, PID = c.MENUNO, ACCESSNAME = c.BTNNAME, ACCESSICON = c.BTNICON, ACCESSNO = c.MENUNO + "*" + c.BTNNO, MENUID = (long?)0, c.BTNID, MENUORDER = c.SEQNO }).ToListAsync();
            buttons.RemoveAll(c => !menus.Contains(a => a.ID == c.PID));
            menus.AddRange(buttons);
            menus = menus.OrderBy(c => c.PID).ThenBy(c => c.MENUORDER).ToList();
            return AjaxResult.Success(menus);
        }

        /// <summary>
        /// 角色权限
        /// </summary>
        /// <param name="roleName">角色名</param>
        /// <param name="appName">应用名</param>
        /// <returns></returns>
        public async Task<AjaxResult> RolePrivilegeAsync(string roleName, string appName)
        {
            if (string.IsNullOrEmpty(appName)) appName = _options.AppName;
            var privilegeList = await _dbContext.Query<CF_PRIVILEGE>().Where(c => c.APPNAME == appName && c.PRIVILEGEMASTER == "CF_ROLE" && c.PRIVILEGEMASTERKEY == roleName)
                .Select(c => new { c.PRIVILEGEACCESS, c.PRIVILEGEACCESSKEY }).ToListAsync();
            var list = privilegeList.Select(c =>
            {
                return c.PRIVILEGEACCESS == "SYS_MENU" ? new { MENUNO = c.PRIVILEGEACCESSKEY, BTNNO = "0" } : new { MENUNO = "0", BTNNO = c.PRIVILEGEACCESSKEY };
            }).ToList();
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 获取指定用户有权限控制(允许或禁止)的 菜单/按钮
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<AjaxResult> UserPrivilegeAsync(long userId, string appName)
        {
            if (string.IsNullOrEmpty(appName)) appName = _options.AppName;

            //角色权限
            var privilegeList = await _dbContext.Query<CF_PRIVILEGE>().Where(c => c.APPNAME == appName && c.PRIVILEGEMASTER == "CF_ROLE"
            && _dbContext.Query<CF_ROLE>().Where(d => d.ROLENAME == c.PRIVILEGEMASTERKEY).InnerJoin<CF_USERROLE>((d, e) => d.ROLEID == e.ROLEID
                                                                            && e.USERID == userId
                                                                            && d.APPNAME == _options.RoleAppName).Select((d, e) => d.ROLEID).Any())
                .Select(c => new { c.PRIVILEGEACCESS, c.PRIVILEGEACCESSKEY, c.PRIVILEGEOPERATION }).ToListAsync();

            //用户权限
            var userPrivilegeList = await _dbContext.Query<CF_PRIVILEGE>().Where(c => c.APPNAME == appName && c.PRIVILEGEMASTER == "CF_USER"
            && _dbContext.Query<CF_USER>().Where(a => a.USERID == userId && a.LOGINNAME == c.PRIVILEGEMASTERKEY).Any())
                .Select(c => new { c.PRIVILEGEACCESS, c.PRIVILEGEACCESSKEY, c.PRIVILEGEOPERATION }).ToListAsync();

            var list = privilegeList.Select(c =>//处理
            {
                var permit = c.PRIVILEGEOPERATION == "Permit";
                if (permit)
                {
                    if (userPrivilegeList.Exists(a => a.PRIVILEGEACCESS == c.PRIVILEGEACCESS && a.PRIVILEGEACCESSKEY == c.PRIVILEGEACCESSKEY && a.PRIVILEGEOPERATION != c.PRIVILEGEOPERATION))
                    {
                        permit = !permit;
                    }
                }
                return c.PRIVILEGEACCESS == "SYS_MENU" ? new { MENUNO = c.PRIVILEGEACCESSKEY, BTNNO = "0", Permit = permit } : new { MENUNO = "0", BTNNO = c.PRIVILEGEACCESSKEY, Permit = permit };
            }).ToList();

            list.AddRange(userPrivilegeList.Select(c =>//合并
            {
                var permit = c.PRIVILEGEOPERATION == "Permit";
                return c.PRIVILEGEACCESS == "SYS_MENU" ? new { MENUNO = c.PRIVILEGEACCESSKEY, BTNNO = "0", Permit = permit } : new { MENUNO = "0", BTNNO = c.PRIVILEGEACCESSKEY, Permit = permit };
            }).ToList());

            list = list.DistinctBy(c => $"{c.MENUNO}*{c.BTNNO}").ToList();//去重

            return AjaxResult.Success(list);
        }

        public async Task<AjaxResult> PrivilegeSaveAsync(List<PrivilegeRequest> list)
        {
            list.ForEach(c =>
            {
                c.AppName = string.IsNullOrEmpty(c.AppName) ? _options.AppName : c.AppName;
            });
            var roleList = (list.Where(c => !c.IsUser)?.DistinctBy(c => $"{c.Masterkey}*{c.AppName}*{c.IsUser}")?.ToList()) ?? new List<PrivilegeRequest>();
            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var a in roleList)
                {
                    await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == a.AppName && c.PRIVILEGEMASTERKEY == a.Masterkey && c.PRIVILEGEMASTER == "CF_ROLE");
                }
                foreach (var entity in list)
                {
                    var isChange = false;
                    if (entity.IsUser)
                    {
                        if (entity.Permit.Equals(entity.Forbid))
                        {
                            await _dbContext.DeleteAsync<CF_PRIVILEGE>(c => c.APPNAME == entity.AppName
                            && c.PRIVILEGEMASTERKEY == entity.Masterkey
                            && c.PRIVILEGEACCESSKEY == entity.Accessno
                            && c.PRIVILEGEMASTER == "CF_USER"
                            && c.PRIVILEGEACCESS == (entity.IsButton ? "SYS_BUTTON" : "SYS_MENU"));
                            isChange = true;
                        }
                    }
                    else if (entity.Permit)
                    {
                        isChange = true;
                    }
                    if (isChange)
                    {
                        var status = $"新增{(entity.IsUser ? "用户" : "角色")}权限";
                        var master = entity.IsUser ? "CF_USER" : "CF_ROLE";
                        var access = entity.IsButton ? "SYS_BUTTON" : "SYS_MENU";
                        var peration = entity.Permit ? "Permit" : "Forbid";
                        await _dbContext.InsertAsync(() => new CF_PRIVILEGE()
                        {
                            APPNAME = entity.AppName,
                            PRIVILEGEMASTER = master,
                            PRIVILEGEMASTERKEY = entity.Masterkey,
                            PRIVILEGEACCESS = access,
                            PRIVILEGEACCESSKEY = entity.Accessno,
                            PRIVILEGEOPERATION = peration,
                            CREATEUSER = _user.Display,
                            CREATEDATE = DateTime.Now,
                            RECORDSTATUS = status
                        });
                    }
                }
                ;
            });
            foreach (var a in roleList)
            {
                await _roleModuleService.Remove(a.Masterkey, a.AppName);
            }
            return AjaxResult.Success("成功");
        }
    }
}