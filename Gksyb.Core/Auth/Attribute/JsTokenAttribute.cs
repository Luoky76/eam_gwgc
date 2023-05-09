using Microsoft.AspNetCore.Http;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 权限验证
    /// </summary>
    public sealed class JsTokenAttribute : AuthorizeAttribute
    {
        public string Key { get; set; }

        public JsTokenAttribute()
        {
        }

        public JsTokenAttribute(string key)
        {
            Key = key;
        }

        public override async Task<bool> ValidAsync(HttpContext httpContext)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = httpContext.Request.Path.Value.TrimStart('/').TrimEnd('/');
                //Key = $"{typeName}/{methodName}";
            }
            return await httpContext.ValidJsToken(Key);
        }

        public override int GetOrder() => 10;
    }
}