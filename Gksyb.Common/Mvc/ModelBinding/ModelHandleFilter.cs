using Gksyb.Common;
using Gksyb.Common.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 参数处理
    /// </summary>
    public class ModelHandleFilter : IAsyncActionFilter, IOrderedFilter
    {
        public int Order => -1000;
        private readonly ApiBehaviorOptions _apiBehaviorOptions;
        private readonly ILogger _logger;

        public ModelHandleFilter(IOptions<ApiBehaviorOptions> options, ILogger<ModelHandleFilter> logger)
        {
            _apiBehaviorOptions = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var description = (ControllerActionDescriptor)context.ActionDescriptor;
            var isHandle = await JsonModelBinding(context);
            isHandle = ParameterHandle(context) || isHandle;
            if (isHandle) ModelStateValid(context);
            context.CheckSecurity();
            var arguments = context.ActionArguments?.Where(c =>
            {
                return !description.Parameters.Any(a => a.Name == c.Key && a.BindingInfo != null &&
                (a.BindingInfo.BindingSource == BindingSource.Services || a.BindingInfo.BindingSource == BindingSource.FormFile));
            }).ToList();
            if (arguments?.Count > 0)
            {
                var body = arguments.ToMiniJson();
                context.HttpContext.SetRequestBodyItem(body);
                if (!description.MethodInfo.HasAttribute<SkipXssFilterAttribute>(false))
                    body.XssFilter();
            }
            if (context.Result == null && !context.ModelState.IsValid)
            {
                _logger.LogDebug(new EventId(1, "ModelEncryptFilterExecuting"), "The request has model state errors, returning an error response.");
                context.Result = _apiBehaviorOptions.InvalidModelStateResponseFactory(context);
                return;
            }
            await next();
        }

        /// <summary>
        /// json模型绑定
        /// </summary>
        /// <param name="context"></param>
        private static async Task<bool> JsonModelBinding(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.HasJsonContentType()) return false;
            var description = context.ActionDescriptor;
            var paramters = description.Parameters.Where(c => c.BindingInfo == null).ToList();
            if (paramters.Count < 1) return false;
            var json = await context.HttpContext.Request.GetContent();
            var dic = json.StartsWith("{") ? json.ToObject<Dictionary<string, JToken>>() : null;
            var form = (dic ?? new Dictionary<string, JToken>()).ToIgnoreCaseDictionary();
            var isHandle = false;
            var jsonType = new List<Type>();
            foreach (var param in paramters)
            {
                if (form.TryGetValue(param.Name, out JToken value))
                {
                    context.ActionArguments[param.Name] = value.ToObject(param.ParameterType);
                    isHandle = true;
                    continue;
                }
                if (param.ParameterType.IsSimpleType()) continue;//简单类型或者已经解析过的类型
                if (jsonType.Contains(param.ParameterType))
                {
                    context.ActionArguments[param.Name] = null;
                    continue;
                }
                try
                {
                    context.ActionArguments[param.Name] = JSONHelper.FromJson(json, param.ParameterType) ?? context.ActionArguments[param.Name];
                    jsonType.Add(param.ParameterType);
                    isHandle = true;
                }
                catch (Exception)
                {
                    if (paramters.Count == 1) throw;
                }
            }
            return isHandle;
        }

        /// <summary>
        /// 参数处理
        /// </summary>
        /// <param name="context"></param>
        private static bool ParameterHandle(ActionExecutingContext context)
        {
            using var handle = new ParameterHandle(context);
            return handle.Handle();
        }

        /// <summary>
        /// 模型验证
        /// </summary>
        /// <param name="context"></param>
        private static void ModelStateValid(ActionExecutingContext context)
        {
            var description = context.ActionDescriptor;
            var controller = context.Controller as ControllerBase;
            context.ModelState.Clear();//清除模型验证
            foreach (var param in description.Parameters.Cast<ControllerParameterDescriptor>())//重新验证
            {
                if (!context.ActionArguments.TryGetValue(param.Name, out object value)) continue;
                if (param.ParameterType.IsSimpleType())
                {
                    var validationAttributes = param.ParameterInfo.GetCustomAttributes<ValidationAttribute>().ToList();
                    if (validationAttributes.Count < 1) continue;
                    ICollection<ValidationResult> results = new List<ValidationResult>();
                    var validationContext = new ValidationContext(value ?? "")
                    {
                        DisplayName = param.Name
                    };
                    if (Validator.TryValidateValue(value, validationContext, results, validationAttributes)) continue;
                    results.ForEach(result =>
                    {
                        context.ModelState.AddModelError(param.Name, result.ErrorMessage);
                    });
                }
                else if (value != null)
                {
                    controller.TryValidateModel(value);
                }
            }
        }
    }
}