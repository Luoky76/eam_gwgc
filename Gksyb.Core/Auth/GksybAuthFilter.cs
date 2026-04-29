using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class GksybAuthFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var description = (ControllerActionDescriptor)context.ActionDescriptor;
                var isValid = await description.MethodInfo.Valid(context.HttpContext);
                if (!isValid)
                {
                    var user = await context.HttpContext.GetCurrentUserAsync();
                    if (user == null)
                    {
                        if (!IsAuth(context.HttpContext)) return;
                        context.Result = new ObjectResult(AjaxResult.Error("登录超时请刷新后重新登录"))
                        {
                            StatusCode = 999
                        };
                        return;
                    }
                    context.Result = new OkObjectResult(AjaxResult.Error("您无权进行此操作"));
                }
            }
            catch (Exception ex)
            {
                context.Result = new OkObjectResult(AjaxResult.Error(ex.ToString()));
            }
        }

        private static string _authAppName = null;
        private static UserSession User = null;

        /// <summary>
        /// 是否验证
        /// </summary>
        private bool IsAuth(HttpContext source)
        {
            if (_authAppName == null)
            {
                var configuration = source.RequestServices.GetService<IConfiguration>();
                _authAppName = configuration.GetValue($"{OptionName.SysContext}:AuthAppName", defaultValue: string.Empty);
            }
            if (string.IsNullOrWhiteSpace(_authAppName)) return true;
            if (User == null)
            {
                var _options = source.RequestServices.GetService<IOptions<SysContextOptions>>()?.Value;
                User = new UserSession()
                {
                    Token = GuidHelper.NewShortId(),
                    UserID = -1,
                    UserName = "UserName",
                    RealName = "RealName",
                    Group = "Group",
                    AllRoles = new List<string>(),
                    IsAdmin = true,
                    IsOurCompany = true,
                    IsApi = true,
                    IP = source.Request.GetRealIP(),
                    UserAgent = source.Request.GetUserAgent(),
                    UserAppName = _options?.UserAppName,
                    RoleAppName = _options?.RoleAppName,
                    MenuAppname = _authAppName
                };
            }
            lock (source)//source.Items不是多线程安全
            {
                source.Items.Remove(nameof(UserSession));
                source.Items.Add(nameof(UserSession), User);
                source.User = User.ToClaimsPrincipal();
            }
            source.Response.StatusCode = StatusCodes.Status200OK;
            return false;
        }
    }
}