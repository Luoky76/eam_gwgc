using Gksyb.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 权限验证
    /// </summary>
    public sealed class GksybAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly string[] SkipBtnNos = new string[] { "List", "Save" };

        /// <summary>
        /// 新增按钮
        /// </summary>
        public const string AddBtn = "add";

        /// <summary>
        /// 更新按钮
        /// </summary>
        public const string UpdateBtn = "save,modify";

        /// <summary>
        /// 删除按钮
        /// </summary>
        public const string DeleteBtn = "delete";

        /// <summary>
        /// 验证是否超级管理员
        /// </summary>
        public bool IsSuper { get; set; }

        /// <summary>
        /// 验证是否管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 验证是否内部用户
        /// </summary>
        public bool IsOurCompany { get; set; }

        /// <summary>
        /// 基础验证 只验证是否登陆（访客用户除外）
        /// </summary>
        public bool IsBaseAuth { get; set; }

        /// <summary>
        /// 验证是否访客以上权限（防止基础验证部分也被访客调用）
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// 验证是否API用户
        /// </summary>
        public bool IsApi { get; set; }

        /// <summary>
        /// 验证是否开发者
        /// </summary>
        public bool IsDeveloper { get; set; }

        /// <summary>
        /// 验证模式
        /// </summary>
        public GksybAuthorizeMode Mode { get; set; } = GksybAuthorizeMode.Equal;

        /// <summary>
        /// 组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public string MenuNo { get; set; }

        /// <summary>
        /// 按钮
        /// </summary>
        public string BtnNo { get; set; }

        /// <summary>
        /// 是否当前登录用户的MenuAppname
        /// </summary>
        public bool IsMenuAppname { get; set; }

        /// <summary>
        /// 通用权限验证
        /// </summary>
        public GksybAuthorizeAttribute()
        {
        }

        /// <summary>
        /// 通用权限验证
        /// </summary>
        /// <param name="isBaseAuth">只验证是否登陆</param>
        public GksybAuthorizeAttribute(bool isBaseAuth)
        {
            IsBaseAuth = isBaseAuth;
        }

        /// <summary>
        /// 通用权限验证
        /// </summary>
        /// <param name="menuNo">菜单编号</param>
        /// <param name="btnNo">按钮</param>
        public GksybAuthorizeAttribute(string menuNo, string btnNo = null)
        {
            MenuNo = menuNo;
            BtnNo = btnNo;
        }

        /// <summary>
        /// 验证菜单
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override async Task<bool> ValidAsync(HttpContext httpContext)
        {
            MenuNo ??= "";
            BtnNo ??= "";
            if (SkipBtnNos.Contains(BtnNo)) BtnNo = "";
            user = await httpContext.GetCurrentUserAsync();
            if (user == null)
            {
                try
                {
                    httpContext.Response.StatusCode = 999;
                }
                catch
                {
                }
                return false;
            }
            if (IsGuest) return true;
            if (IsBaseAuth && !user.IsGuest) return true;
            if (IsSuper) return user.IsSuper;
            if (IsAdmin) return user.IsAdmin;
            if (IsOurCompany) return user.IsOurCompany;
            if (IsApi) return user.IsApi;
            if (IsDeveloper) return user.IsDeveloper;
            SetAppname(httpContext);
            if (CheckGroup()) return true;
            if (!CheckButton()) return false;
            var roleModuleService = httpContext.RequestServices.GetService<IRoleModuleService>();
            foreach (var roleName in user.Roles)
            {
                var isValid = await roleModuleService.ValidButtonModule(roleName, appname, MenuNo, BtnNo, Mode);
                if (isValid) return true;
            }
            return false;
        }

        /// <summary>
        /// 验证按钮权限
        /// </summary>
        public bool CheckButton()
        {
            if (string.IsNullOrWhiteSpace(BtnNo)) return CheckMenu();
            if (user.ForbinButtons == null || user.ForbinButtons.Count < 1) return true;
            var menus = (MenuNo ?? "").Split(",");
            var btns = (BtnNo ?? "").Split(",");
            var match = Mode.GetFunc();
            foreach (var menu in menus)
            {
                var key = $"{menu}__{appname}";
                if (!user.ForbinButtons.Keys.Any(c => match(c, key))) return true;
                foreach (var menuKey in user.ForbinButtons.Keys)
                {
                    foreach (var btn in btns)
                    {
                        if (!user.ForbinButtons[menuKey].Any(c => match(c.BTNNO, btn) && c.APPNAME == appname)) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 验证菜单
        /// </summary>
        public bool CheckMenu()
        {
            if (user.ForbinMenus == null || user.ForbinMenus.Count < 1) return true;
            var menus = (MenuNo ?? "").Split(",");
            var match = Mode.GetFunc();
            foreach (var menu in menus)
            {
                if (!user.ForbinMenus.Any(c => match(c.MENUNO, menu) && c.APPNAME == appname)) return true;
            }
            return false;
        }

        /// <summary>
        /// 验证用户所属组
        /// </summary>
        private bool CheckGroup()
        {
            if (string.IsNullOrWhiteSpace(Group)) return true;
            var match = Mode.GetFunc();
            return match(Group, user.Group);
        }

        /// <summary>
        /// 设置应用名
        /// </summary>
        private void SetAppname(HttpContext httpContext)
        {
            appname = httpContext.Request.Query["appname"].ToString();
            appname = string.IsNullOrWhiteSpace(appname) ? (httpContext.Request.Headers[HeaderNames.Referer].ToString() ?? "").GetParm("appname") : appname;
            appname = string.IsNullOrWhiteSpace(appname) ? user.MenuAppname : appname;
            if (IsMenuAppname) appname = user.MenuAppname;
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        private UserSession user;

        /// <summary>
        /// 应用名
        /// </summary>
        private string appname;

        public override int GetOrder() => 20;
    }

    /// <summary>
    /// 验证模式
    /// </summary>
    public enum GksybAuthorizeMode
    {
        /// <summary>
        /// 相等
        /// </summary>
        Equal,

        /// <summary>
        /// 起始于
        /// </summary>
        StartsWith,

        /// <summary>
        /// 正则
        /// </summary>
        Regex
    }

    public static class GksybAuthorizeModeExtensions
    {
        /// <summary>
        /// 根据模式获取比较函数
        /// </summary>
        public static Func<string, string, bool> GetFunc(this GksybAuthorizeMode mode) => mode switch
        {
            GksybAuthorizeMode.Regex => (a, b) => a.IsMatch(b),
            GksybAuthorizeMode.StartsWith => (a, b) => b.StartsWith(a),
            _ => (a, b) => a == b,
        };
    }
}