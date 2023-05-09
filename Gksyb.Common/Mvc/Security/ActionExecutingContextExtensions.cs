using Gksyb.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public static class ActionExecutingContextExtensions
    {
        /// <summary>
        /// 安全检查
        /// </summary>
        public static void CheckSecurity(this ActionExecutingContext source)
        {
            foreach (var param in source.ActionDescriptor.Parameters.Cast<ControllerParameterDescriptor>())
            {
                if (!source.ActionArguments.TryGetValue(param.Name, out object obj) || obj == null) continue;
                if (typeof(IFormFile).IsAssignableFrom(param.ParameterType)) (obj as IFormFile).CheckSecurity(param.ParameterInfo);
                if (typeof(IEnumerable<IFormFile>).IsAssignableFrom(param.ParameterType)) (obj as IEnumerable<IFormFile>).CheckSecurity(param.ParameterInfo);
            }
        }

        /// <summary>
        /// 安全检查
        /// </summary>
        public static void CheckSecurity(this IEnumerable<IFormFile> source, ParameterInfo parameterInfo = null)
        {
            source.ForEach(formFile => formFile.CheckSecurity(parameterInfo));
        }
    }
}