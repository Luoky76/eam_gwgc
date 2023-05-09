using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Common.Interface
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 配置http请求管道之前
        /// </summary>
        void PreConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        /// 配置http请求管道
        /// </summary>
        void Configure(IApplicationBuilder app, IWebHostEnvironment env);

        /// <summary>
        /// 向容器加入服务
        /// </summary>
        void ConfigureServices(IServiceCollection services, IMvcBuilder builder, IConfiguration configuration);

        /// <summary>
        /// 排序
        /// </summary>
        /// <returns></returns>
        int GetOrder() => 10000;
    }
}