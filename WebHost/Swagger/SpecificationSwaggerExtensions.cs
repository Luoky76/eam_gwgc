//using IGeekFan.AspNetCore.Knife4jUI;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Mvc.Controllers;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.OpenApi.Models;
//using System.Collections.Generic;
//using System.IO;

//namespace WebHost
//{
//    public static class SpecificationSwaggerExtensions
//    {
//        private static Dictionary<string, OpenApiInfo> _openApiInfos;

//        /// <summary>
//        /// 添加API文档服务
//        /// </summary>
//        public static IServiceCollection AddSpecificationSwagger(this IServiceCollection services, IConfiguration configuration)
//        {
//            _openApiInfos = configuration.GetSection("Swagger:Group").Get<Dictionary<string, OpenApiInfo>>() ?? new Dictionary<string, OpenApiInfo>();
//            if (_openApiInfos.Count < 1) return services;
//            services.AddSwaggerGen(options =>
//            {
//                foreach (var dic in _openApiInfos)
//                {
//                    options.SwaggerDoc(dic.Key, dic.Value);
//                }
//                var fileNames = Directory.GetFiles(StaticData.PluginDirectory, "*.xml", SearchOption.AllDirectories);
//                foreach (var name in fileNames)
//                {
//                    options.IncludeXmlComments(name, true);
//                }
//                options.AddServer(new OpenApiServer()
//                {
//                    Url = "",
//                    Description = "Api文档"
//                });
//                options.CustomOperationIds(apiDesc =>
//                {
//                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
//                    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
//                });
//                options.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
//                //c.AddSecurityDefinition("Bearer",
//                //    new OpenApiSecurityScheme
//                //    {
//                //        Description = "请输入OAuth接口返回的Token，前置Bearer。示例：Bearer {Roken}",
//                //        Name = "Authorization",
//                //        In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
//                //        Type = SecuritySchemeType.ApiKey
//                //    });
//                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
//                //{
//                //   {
//                //        new OpenApiSecurityScheme
//                //        {
//                //            Reference = new OpenApiReference()
//                //            {
//                //                Id = "Bearer",
//                //                Type = ReferenceType.SecurityScheme
//                //            }
//                //        }, Array.Empty<string>()
//                //    }
//                //});
//            });
//            return services;
//        }

//        /// <summary>
//        /// 添加API文档中间件
//        /// </summary>
//        public static IApplicationBuilder UseSpecificationSwagger(this IApplicationBuilder app)
//        {
//            if (_openApiInfos.Count < 1) return app;
//            //启用中间件服务生成SwaggerUI，指定Swagger JSON终结点
//            app.UseSwagger(c =>
//            {
//                //c.SerializeAsV2 = true;
//            }).UseKnife4UI(c =>//UseSwaggerUI  UseKnife4UI
//            {
//                c.DocumentTitle = "API文档";
//                foreach (var dic in _openApiInfos)
//                {
//                    c.SwaggerEndpoint($"/swagger/{dic.Key}/swagger.json", $"{dic.Value.Title}");
//                }
//                c.RoutePrefix = "docs";//设置根节点访问
//            });
//            return app;
//        }
//    }
//}