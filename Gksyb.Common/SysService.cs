using Flurl.Http;
using Flurl.Http.Configuration;
using Gksyb.Common.Data;
using Gksyb.Common.Static;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using System.Net;
using System.Text;

namespace Gksyb.Common
{
    public static class SysService
    {
        /// <summary>
        /// 系统基础配置
        /// </summary>
        public static IServiceCollection AddBaseService(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RedisCacheOptions>(config.GetSection(OptionName.RedisCache));
            services.Configure<SysContextOptions>(config.GetSection(OptionName.SysContext));
            //全局存储服务描述列表
            HttpContext.ServiceCollection = services;
            //加入gb2312编码支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //默认序列化
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings().Custom();
            //flurl全局配置
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; };
            FlurlHttp.Configure(settings =>
            {
                settings.HttpClientFactory = new SSLHttpClientFactory();
                settings.Timeout = TimeSpan.FromSeconds(10);
                settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings().Custom());
            });
            //添加全局HttpContext
            services.AddHttpContextAccessor();
            services.AddStaticHttpContext();
            //分布式缓存
            services.AddDistributedCache(config);
            //分布式锁
            services.AddDistributedLock(config);
            return services;
        }

        /// <summary>
        /// 系统通用配置
        /// </summary>
        public static IServiceCollection AddSysService(this IServiceCollection services, IConfiguration config)
        {
            services.AddBaseService(config);
            //数据库
            services.AddDbContext(config);
            //任务调度
            services.AddQuartzServer();
            //事件总线
            services.AddEventBus();
            //微信
            services.AddWeixin(config.GetSection(OptionName.Weixin));
            return services;
        }

        /// <summary>
        /// SignalR分布式
        /// </summary>
        /// <returns></returns>
        public static ISignalRServerBuilder AddRedis(this ISignalRServerBuilder signalrBuilder, IConfiguration config)
        {
            var cacheOptions = config.GetSection(OptionName.RedisCache).Get<RedisCacheOptions>();
            if (!string.IsNullOrWhiteSpace(cacheOptions.Configuration))
            {
                signalrBuilder.AddStackExchangeRedis(cacheOptions.Configuration, options =>
                {
                });
            }
            return signalrBuilder;
        }
    }
}