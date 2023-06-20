using Gksyb.Common.Data;
using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class RoleModuleService : IRoleModuleService
    {
        private readonly IDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;

        public RoleModuleService(IDbContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            if (DbContextFactory.ConnectionString != _dbContext.Session.CurrentConnection.ConnectionString)
            {
                _dbContext = DbContextFactory.CreateContext();
            }
            _distributedCache = distributedCache;
        }

        /// <inheritdoc/>
        public async Task<List<ButtonModule>> GetButtonModule(string roleName, string menuAppname, string menuNo, GksybAuthorizeMode mode)
        {
            var key = $"{ButtonCachePrefix}{roleName}_{menuAppname}";
            var sortList = await GetButtonAsync(key);
            if (sortList == null)
            {
                var isCommonRole = await IsCommonRole(roleName);
                var query = _dbContext.Query<SYS_BUTTON>().Where(s => s.APPNAME == menuAppname)
                    .WhereIf(isCommonRole, s => _dbContext.Query<CF_PRIVILEGE>().Where(c => c.PRIVILEGEMASTER == "CF_ROLE"
                                                       && c.PRIVILEGEACCESS == "SYS_BUTTON"
                                                       && c.PRIVILEGEOPERATION == "Permit"
                                                       && c.PRIVILEGEMASTERKEY == roleName
                                                       && Sql.IsEqual(c.APPNAME, s.APPNAME)
                                                       && Sql.IsEqual(c.PRIVILEGEACCESSKEY, (s.MENUNO + "*" + s.BTNNO))).Any());
                var list = await query.ToListAsync<ButtonModule>();
                sortList = new SortedList<string, List<ButtonModule>>();
                list.GroupBy(c => c.MENUNO).ForEach((list) =>
                {
                    sortList.Add(list.Key, list.ToList());
                });
                await SetButtonAsync(key, sortList);
            }
            if (mode != GksybAuthorizeMode.Regex)
            {
                return sortList.ContainsKey(menuNo) ? sortList[menuNo] : new List<ButtonModule>();
            }
            var buttons = new List<ButtonModule>();
            foreach (var menu in sortList.Keys)
            {
                if (menu.IsMatch(menuNo)) buttons.AddRange(sortList[menu]);
            }
            return buttons;
        }

        /// <inheritdoc/>
        public async Task<List<MenuModule>> GetMenuModule(string roleName, string menuAppname)
        {
            var key = $"{MenuCachePrefix}{roleName}_{menuAppname}";
            var list = await GetMenuAsync(key);
            if (list == null)
            {
                var query = _dbContext.Query<SYS_MENU>().Where(s => s.APPNAME == menuAppname)
                    .WhereIf(roleName != UserSession.SuperRoleName, c => c.ISVISIBLE == 1);
                list = await query.ToListAsync<MenuModule>();
                var isCommonRole = await IsCommonRole(roleName);
                if (isCommonRole)
                {
                    var menunos = await _dbContext.Query<CF_PRIVILEGE>().Where(c => c.PRIVILEGEMASTER == "CF_ROLE"
                                             && c.PRIVILEGEACCESS == "SYS_MENU"
                                             && c.PRIVILEGEOPERATION == "Permit"
                                             && c.PRIVILEGEMASTERKEY == roleName
                                             && Sql.IsEqual(c.APPNAME, menuAppname)).Select(c => c.PRIVILEGEACCESSKEY).ToListAsync();
                    var menus = list.Where(c => menunos.Any(a => a == c.MENUNO)).ToList();
                    var parents = menus.Where(c => !string.IsNullOrWhiteSpace(c.MENUPARENTNO) && !menus.Any(a => a.MENUNO == c.MENUPARENTNO)).ToList();
                    if (parents.Count > 0)
                    {
                        void recursion(string menuno)
                        {
                            var menu = list.Find(c => c.MENUNO == menuno);
                            if (menu == null) return;
                            menus.Add(menu);
                            if (string.IsNullOrWhiteSpace(menu.MENUPARENTNO)) return;
                            recursion(menu.MENUPARENTNO);
                        }
                        parents.ForEach(c =>
                        {
                            recursion(c.MENUPARENTNO);
                        });
                        menus = menus.DistinctBy(c => new { c.MENUNO, c.APPNAME }).ToList();
                    }
                    list = menus.Where(c => menus.Any(a => a.MENUPARENTNO == c.MENUNO) || !list.Any(a => a.MENUPARENTNO == c.MENUNO)).ToList();
                    list ??= new List<MenuModule>();
                }
                await SetMenuAsync(key, list);
            }
            return list;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidButtonModule(string roleName, string menuAppname, string menuNo, string btnNo, GksybAuthorizeMode mode)
        {
            if (string.IsNullOrWhiteSpace(btnNo)) return await ValidMenuModule(roleName, menuAppname, menuNo, mode);
            var menus = (menuNo ?? "").Split(",");
            var btns = (btnNo ?? "").Split(",");
            if (mode != GksybAuthorizeMode.Regex)
            {
                foreach (var menu in menus)
                {
                    var list = await GetButtonModule(roleName, menuAppname, menu, mode);
                    if (mode == GksybAuthorizeMode.StartsWith)
                    {
                        foreach (var btn in btns)
                        {
                            if (list.Contains(c => btn.StartsWith(c.BTNNO))) return true;
                        }
                    }
                    else
                    {
                        foreach (var btn in btns)
                        {
                            if (list.Contains(c => c.BTNNO == btn)) return true;
                        }
                    }
                }
                return false;
            }
            foreach (var menu in menus)
            {
                var list = await GetButtonModule(roleName, menuAppname, menu, mode);
                foreach (var btn in btns)
                {
                    if (list.Contains(c => c.BTNNO.IsMatch(btn))) return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidMenuModule(string roleName, string menuAppname, string menuNo, GksybAuthorizeMode mode)
        {
            var list = await GetMenuModule(roleName, menuAppname);
            var menus = (menuNo ?? "").Split(",");
            if (mode != GksybAuthorizeMode.Regex)
            {
                if (mode == GksybAuthorizeMode.StartsWith)
                {
                    foreach (var menu in menus)
                    {
                        if (list.Exists(c => menu.StartsWith(c.MENUNO))) return true;
                    }
                }
                else
                {
                    foreach (var menu in menus)
                    {
                        if (list.Exists(c => c.MENUNO == menu)) return true;
                    }
                }
                return false;
            }
            foreach (var menu in menus)
            {
                if (list.Exists(c => c.MENUNO.IsMatch(menu))) return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task Remove(string roleName, string menuAppname)
        {
            var key = $"{MenuCachePrefix}{roleName}_{menuAppname}";
            await _distributedCache.RemoveAsync(key);
            key = $"{ButtonCachePrefix}{roleName}_{menuAppname}";
            await _distributedCache.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public async Task Clear(string roleAppname, string menuAppname)
        {
            var roles = await _dbContext.Query<CF_ROLE>().Where(c => c.APPNAME == roleAppname)
                .Select(c => c.ROLENAME).ToListAsync();
            roles.Add(UserSession.SuperRoleName);
            foreach (var roleName in roles)
            {
                await Remove(roleName, menuAppname);
            }
        }

        /// <summary>
        /// 管理员角色名
        /// </summary>
        private static string _adminRoleName;

        /// <summary>
        /// 是否普通角色
        /// </summary>
        private async Task<bool> IsCommonRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(_adminRoleName))
            {
                var options = HttpContext.RequestServices.GetService<IOptions<SysContextOptions>>();
                _adminRoleName = await _dbContext.Query<CF_ROLE>().Where(c => c.ROLEID == options.Value.AdminRole).Select(c => c.ROLENAME).FirstOrDefaultAsync();
            }
            return roleName != UserSession.SuperRoleName && _adminRoleName != roleName;
        }

        /// <summary>
        /// 获取角色菜单
        /// </summary>
        private async Task<List<MenuModule>> GetMenuAsync(string key)
        {
            return await _distributedCache.GetAsync<List<MenuModule>>(key, null);
        }

        /// <summary>
        /// 设置角色菜单
        /// </summary>
        private async Task SetMenuAsync(string key, List<MenuModule> value)
        {
            await _distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(12)
            });
        }

        /// <summary>
        /// 获取角色按钮
        /// </summary>
        private async Task<SortedList<string, List<ButtonModule>>> GetButtonAsync(string key)
        {
            return await _distributedCache.GetAsync<SortedList<string, List<ButtonModule>>>(key, null);
        }

        /// <summary>
        /// 设置角色按钮
        /// </summary>
        private async Task SetButtonAsync(string key, SortedList<string, List<ButtonModule>> value)
        {
            await _distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(12)
            });
        }

        /// <summary>
        /// 菜单缓存前缀
        /// </summary>
        private static readonly string MenuCachePrefix = "Menu_";

        /// <summary>
        /// 按钮缓存前缀
        /// </summary>
        private static readonly string ButtonCachePrefix = "Button_";
    }
}