using Gksyb.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HeadAuthorizeAttribute : Attribute, IAuthorizationFilter, IOrderedFilter
    {
        public string Key { get; set; } = "passport";

        public string Passport { get; set; } = "FZG!K#N3WDPUzcmLq8g@aVRhQ20Y7xlX";

        public int Order => int.MinValue;

        public HeadAuthorizeAttribute()
        { }

        public HeadAuthorizeAttribute(string passport, string key = null)
        {
            Key = key;
            Passport = passport;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var value = context.HttpContext.GetAuthToken(Key);
            if (value == Passport) return;
            context.Result = new OkObjectResult(AjaxResult.Error("您无权进行此操作"));
        }
    }
}