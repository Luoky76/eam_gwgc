using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 自定义模型行为 加入默认http谓词
    /// </summary>
    public class ApplicationModelConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                ConfigureController(controller);

                var actions = controller.Actions;
                // 查找所有重复的方法签名
                var repeats = actions.GroupBy(c => c.ActionName).Where(c => c.Count() > 1).Select(c => c.Last()).ToList();
                foreach (var action in actions)
                {
                    ConfigureAction(action);
                    // 跳过相同方法签名
                    if (repeats.Contains(action)) action.ApiExplorer.IsVisible = false;
                }
            }
        }

        /// <summary>
        /// 配置控制器
        /// </summary>
        private static void ConfigureController(ControllerModel controller)
        {
            ConfigureControllerApiExplorer(controller);
        }

        /// <summary>
        /// 配置控制器ApiExplorer
        /// </summary>
        private static void ConfigureControllerApiExplorer(ControllerModel controller)
        {
            if (controller.ApiExplorer.IsVisible == false) return;
            if (controller.ApiExplorer.GroupName == null)
            {
                if (!controller.ApiExplorer.IsVisible.HasValue)
                {
                    controller.ApiExplorer.IsVisible = false;
                    return;
                }
                var selectorModel = controller.Selectors[0];
                if (selectorModel.AttributeRouteModel != null && !selectorModel.AttributeRouteModel.Template.StartsWith("[controller]"))
                {
                    controller.ApiExplorer.GroupName = selectorModel.AttributeRouteModel.Template.Split("/")[0];
                }
            }
        }

        /// <summary>
        /// 配置动作方法
        /// </summary>
        private static void ConfigureAction(ActionModel action)
        {
            ConfigureActionHttpMethodAttribute(action);
            ConfigureActionRouteAttribute(action);
            ConfigureActionApiExplorer(action);
        }

        /// <summary>
        /// 配置动作方法请求谓词特性
        /// </summary>
        /// <param name="action">动作方法模型</param>
        private static void ConfigureActionHttpMethodAttribute(ActionModel action)
        {
            var selectorModel = action.Selectors[0];
            if (action.Parameters.Any(c => typeof(IFormFile).IsAssignableFrom(c.ParameterType)))//文件上传默认长度无限制
            {
                if (!action.Filters.Any(c => c is DisableRequestSizeLimitAttribute))
                {
                    action.Filters.Add(new DisableRequestSizeLimitAttribute());
                }
                if (!action.Filters.Any(c => c is RequestFormLimitsAttribute))
                {
                    action.Filters.Add(new RequestFormLimitsAttribute()
                    {
                        MultipartBodyLengthLimit = long.MaxValue
                    });
                }
            }
            // 跳过已配置请求谓词特性的配置
            if (selectorModel.ActionConstraints.Count > 0) return;
            // 添加默认POST请求约束
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { "POST" }));
            selectorModel.EndpointMetadata.Add(new HttpPostAttribute());
        }

        /// <summary>
        /// 配置动作方法请求谓词特性
        /// </summary>
        /// <param name="action">动作方法模型</param>
        private static void ConfigureActionRouteAttribute(ActionModel action)
        {
            var selectorModel = action.Selectors[0];
            // 跳过已配置路由特性的配置
            if (selectorModel.AttributeRouteModel != null) return;
            // 添加默认路由规则
            var template = action.Controller.Selectors[0].AttributeRouteModel?.Template ?? "";//Controller的Route
            if (template.Contains("[action]")) return;
            if (template.Contains("[controller]")) template = "[action]";
            else template = "[controller]/[action]";
            //设置action的Route
            selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(template));
        }

        /// <summary>
        /// 配置动作方法ApiExplorer
        /// </summary>
        private static void ConfigureActionApiExplorer(ActionModel action)
        {
            if (action.ApiExplorer.IsVisible == false) return;
            action.ApiExplorer.IsVisible = action.Controller.ApiExplorer.IsVisible;
            action.ApiExplorer.GroupName ??= action.Controller.ApiExplorer.GroupName;
        }
    }
}