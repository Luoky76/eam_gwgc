using Gksyb.Common;
using Gksyb.Common.Mvc.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class WhitelistAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
        private readonly string _appid;
        private readonly string _ip;

        public int Order => int.MinValue;

        public WhitelistAuthorizeAttribute(string appid, string ip = null)
        {
            _appid = appid;
            _ip = ip;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var ips = _ip;
            if (!string.IsNullOrWhiteSpace(_appid))
            {
                var service = context.HttpContext.RequestServices.GetRequiredService<IWhitelistService>();
                ips = $"{(await service.GetAsync(_appid))},{ips}";
            }
            var ip = context.HttpContext.Request.GetRealIP();
            var list = (ips ?? "").Split(",").DistinctAndOrderBy().ToList();
            if (list.Any(pattern => Regex.IsMatch(ip, pattern))) return;
            context.Result = new OkObjectResult(AjaxResult.Error($"{ip}不在白名单"));
        }
    }
}