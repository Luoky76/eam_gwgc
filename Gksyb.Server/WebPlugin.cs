using Chloe.Infrastructure.Interception;
using Gksyb.Common.EventBus;
using Gksyb.Common.Interface;
using Gksyb.Common.Quartz;
using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gksyb.Server
{
    internal class WebPlugin : IPlugin
    {
        public void PreConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseUploadDirectory();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BroadcastChannelHub>("/broadcast-channel");
            });
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder builder, IConfiguration configuration)
        {
            builder.AddMvcOptions(configure =>
            {
                configure.Filters.Add<GksybAuthFilter>();
            });

            services.AddSignalR(options =>//signalR处理
            {
                options.AddFilter<AuthHubFilter>();
            }).AddHubOptions<BroadcastChannelHub>(options =>
            {
            });
            services.AddSingleton<IUserIdProvider, AuthUserIdProvider>();

            services.AddScoped(c => HttpContext.Current?.GetCurrentUserAsync().Result());
            services.AddScoped(c => HttpContext.Current?.GetCurrentUserOrDefault().MapTo<ScopeUser>());

            var assembly = Assembly.GetExecutingAssembly();

            assembly.AddAllService();

            assembly.AddIEventSubscriber();//注册IEventSubscriber

            builder.AddApplicationPart(assembly);
            //数据库日志拦截
            DbContextInterception.Add(new DbContextInterceptor());
            assembly.AddEntityTypeBuilder();

            //任务调度实现
            services.AddScoped<IQuartzStore, QuartzStore>();
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <returns></returns>
        public int GetOrder() => 100;
    }
}