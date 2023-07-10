using Gksyb.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class JsTokenAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
    {
        public string Key { get; set; }

        public int Order => int.MinValue;

        public JsTokenAttribute()
        { }

        public JsTokenAttribute(string key) => Key = key;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (await context.HttpContext.ValidJsToken(Key)) return;
            context.Result = new OkObjectResult(AjaxResult.Error("您无权进行此操作"));
        }
    }
}