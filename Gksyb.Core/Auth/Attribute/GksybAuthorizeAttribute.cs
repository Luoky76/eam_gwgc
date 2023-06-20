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
        /// 基础验证 只验证是否登陆
        /// </summary>
        public bool IsBaseAuth { get; set; }

        /// <summary>
        /// 起始于
        /// </summary>
        public bool IsStartsWith { get; set; }

        /// <summary>
        /// 权限验证启用正则匹配
        /// </summary>
        public bool IsRegex { get; set; }

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
        /// <param name="addBtn">新增按钮</param>
        /// <param name="updateBtn">更新按钮</param>
        /// <param name="deleteBtn">删除按钮</param>
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
            var user = await httpContext.GetCurrentUserAsync();
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
            if (IsBaseAuth) return true;
            if (IsSuper && user.IsSuper) return true;
            if (IsAdmin && user.IsAdmin) return true;
            if (IsOurCompany && user.IsOurCompany) return true;
            var appname = httpContext.Request.Query["appname"].ToString();
            appname = string.IsNullOrWhiteSpace(appname) ? (httpContext.Request.Headers[HeaderNames.Referer].ToString() ?? "").GetParm("appname") : appname;
            appname = string.IsNullOrWhiteSpace(appname) ? user.MenuAppname : appname;
            if (IsMenuAppname) appname = user.MenuAppname;
            var mode = IsRegex ? GksybAuthorizeMode.Regex : IsStartsWith ? GksybAuthorizeMode.StartsWith : GksybAuthorizeMode.Equal;
            if (!user.CheckButton(MenuNo, BtnNo, mode, appname)) return false;
            var roleModuleService = httpContext.RequestServices.GetService<IRoleModuleService>();
            foreach (var roleName in user.Roles)
            {
                var isValid = await roleModuleService.ValidButtonModule(roleName, appname, MenuNo, BtnNo, mode);
                if (isValid) return true;
            }
            return false;
        }

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
}