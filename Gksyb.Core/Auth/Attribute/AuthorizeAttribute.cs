using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 验证抽象类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// 基础验证
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> ValidAsync(HttpContext httpContext);

        public abstract int GetOrder();
    }

    public static class MethodInfoExtension
    {
        /// <summary>
        /// 验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> Valid(this MethodInfo source, HttpContext httpContext)
        {
            if (source == null) return true;
            var typeName = source.ReflectedType.Name;
            var methodName = source.Name;
            typeName = Regex.Replace(typeName, @"Controller$", "", RegexOptions.IgnoreCase);
            methodName = Regex.Replace(methodName, @"Async$", "", RegexOptions.IgnoreCase);
            bool isValid, isAuth = false;
            var typeAttributes = source.ReflectedType.GetCustomAttributes(true);
            var validMenuNo = typeName;
            foreach (var attribute in typeAttributes)//使用GksybAuthorizeAttribute指定名称 作为验证依据
            {
                if (attribute is GksybAuthorizeAttribute gksybAuthorizeAttribute && !string.IsNullOrWhiteSpace(gksybAuthorizeAttribute.MenuNo))
                {
                    validMenuNo = gksybAuthorizeAttribute.MenuNo;
                    break;
                }
            }

            //验证属性
            async Task<bool> ValidAsync(object[] attributes)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute is AuthorizeAttribute) break;
                    if (attribute is AllowAnonymousAttribute) return true;
                }
                var authorizeAttributes = attributes.Where(c => c is AuthorizeAttribute).Select(c => c as AuthorizeAttribute).OrderBy(c => c.GetOrder()).ToList();
                foreach (var authorizeAttribute in authorizeAttributes)
                {
                    if (authorizeAttribute is GksybAuthorizeAttribute gksybAuthorizeAttribute)
                    {
                        isAuth = true;
                        if (string.IsNullOrWhiteSpace(gksybAuthorizeAttribute.MenuNo)) gksybAuthorizeAttribute.MenuNo = validMenuNo;
                    }
                    if (!await authorizeAttribute.ValidAsync(httpContext)) return false;
                }
                return true;
            }

            //先验证方法属性
            var methodAttributes = source.GetCustomAttributes(true);
            isValid = await ValidAsync(methodAttributes);
            if (!isValid) return false;
            if (methodAttributes.Any(c => c is AllowAnonymousAttribute)) return true;

            //验证类型属性
            isValid = await ValidAsync(typeAttributes);
            if (!isValid) return false;
            if (typeAttributes.Any(c => c is AllowAnonymousAttribute)) return true;

            if (!isAuth)//方法和类型不存在GksybAuthorizeAttribute，调用默认验证
            {
                isValid = await new GksybAuthorizeAttribute(validMenuNo, methodName).ValidAsync(httpContext);
                if (!isValid) return false;
            }
            var user = await httpContext.GetCurrentUserAsync();
            return user != null;
        }
    }
}