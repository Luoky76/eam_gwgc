using Gksyb.Common.EventBus;
using Gksyb.Common.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EAM.Device
{
    internal class WebPlugin : IPlugin
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services, IMvcBuilder builder, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();
            assembly.AddAllService();
            assembly.AddIEventSubscriber();
            builder.AddApplicationPart(assembly);
        }

    }
}