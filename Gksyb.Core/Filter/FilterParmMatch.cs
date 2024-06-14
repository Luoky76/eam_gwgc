using Gksyb.Common.Static;
using Gksyb.Core.Auth;

namespace Gksyb.Core.Filter
{
    public static class FilterParmMatch
    {
        /// <summary>
        /// 匹配当前用户信息，都是int类型
        /// 对于CurrentRoleID，只返回第一个角色
        /// </summary>
        public static readonly Dictionary<string, Func<object>> CurrentParmMatch = new()
        {
            { "{IsOurCompany}", () => User?.IsOurCompany.ToString() == "True" ? "1" : "0" },
            { "{CurrentRealName}", () => User?.RealName },
            { "{CurrentUserName}", () => User?.UserName },
            { "{CurrentUserID}", () => User?.UserID },
            { "{CurrentRoleID}", () => User?.Roles },
            { "{CurrentAllCorps}", () => User?.AllCorps.Select(c=>c.CorpID) },
            { "{CurrentCorp}", () => User?.Corp.CorpID },
            { "{CurrentStations}", () => User?.Corp.Station },
            { "{CurrentParentCompany}", () => User?.ParentCompany?.CorpID },
            { "{CurrentOpenid}", () => User?.Openid },
            { "{IsAdmin}", () => User?.IsAdmin.ToString() == "True" ? "1" : "0" },
        };

        /// <summary>
        /// 用户信息
        /// </summary>
        private static UserSession User
        {
            get
            {
                var user = HttpContext.Current.GetCurrentUserAsync().Result();
                if (user != null) return user;
                try
                {
                    HttpContext.Current.Response.StatusCode = 999;
                }
                catch
                {
                }
                return user;
            }
        }
    }
}