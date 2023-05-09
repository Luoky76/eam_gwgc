using Chloe;
using Gksyb.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc
{
    public class AjaxResultFilter : IResultFilter, IExceptionFilter
    {
        private readonly ILogger<AjaxResultFilter> _logger;
        private readonly LogPath _logPath = new("Exception");

        public AjaxResultFilter(ILogger<AjaxResultFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            try
            {
                context.ExceptionHandled = true;
                if (context.Exception != null)
                {
                    if (context.Exception is not MessageException)//非消息异常，打印日志
                    {
                        var message = context.Exception.ToString();
                        try
                        {
                            var methodInfo = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo;
                            message = $"{methodInfo.ReflectedType.Name}.{methodInfo.Name}:{message}";
                        }
                        catch (Exception)
                        {
                        }
                        _logger.LogError(_logPath, message);
                    }
                    context.Result = new OkObjectResult(AjaxResult.Error(context.Exception.Message));
                    return;
                }
                if (context.Result != null && context.Result is ObjectResult objectResult)
                {
                    context.HttpContext.SetResponseBodyItem(objectResult.Value);
                }
            }
            catch (Exception)
            {
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.Result != null && context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is AjaxResult ajaxResult && !ajaxResult.IsError)
                {
                    var dbContext = context.HttpContext.RequestServices.GetResolvedServices<IDbContext>();
                    if (dbContext != null && dbContext.Session.IsInTransaction)
                    {
                        dbContext.Session.CommitTransaction();
                    }
                }
                context.HttpContext.SetResponseBodyItem(objectResult.Value);
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            switch (context.Result)
            {
                case StatusCodeResult result:
                    context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                    context.Result = result.StatusCode == StatusCodes.Status200OK
                        ? new OkObjectResult(AjaxResult.Success("请求成功"))
                        : new OkObjectResult(AjaxResult.Error($"请求失败，状态码{result.StatusCode}"));
                    return;

                case ObjectResult objectResult:
                    if (objectResult.Value is ValidationProblemDetails validationProblemDetails)
                    {
                        context.Result = new OkObjectResult(AjaxResult.Error($"{validationProblemDetails.Errors?.Values.ToJson()}"));
                    }
                    else if (objectResult.Value is ProblemDetails problemDetails)
                    {
                        context.Result = new OkObjectResult(AjaxResult.Error($"{problemDetails.Status} {problemDetails.Title}"));
                        return;
                    }
                    break;
            }
        }
    }
}