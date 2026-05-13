using Gksyb.Common.EventBus;
using Gksyb.Common.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EAM.Third
{
    internal class WebPlugin : IPlugin
    {
        /// <summary>
        /// 配置应用管道
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        /// 配置服务注册
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IMvcBuilder builder, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            assembly.AddAllService();
            assembly.AddIEventSubscriber();
            builder.AddApplicationPart(assembly);
        }

    }
}
