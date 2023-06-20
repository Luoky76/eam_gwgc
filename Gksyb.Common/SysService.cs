using Flurl.Http;
using Flurl.Http.Configuration;
using Gksyb.Common.Data;
using Gksyb.Common.Static;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using System.Net;

namespace Gksyb.Common
{
    public static class SysService
    {
        /// <summary>
        /// 系统通用配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddSysService(this IServiceCollection services, IConfiguration config)
        {
            //全局存储服务描述列表
            HttpContext.ServiceCollection = services;
            //加入gb2312编码支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.Configure<SysContextOptions>(config.GetSection(OptionName.SysContext));
            services.Configure<RedisCacheOptions>(config.GetSection(OptionName.RedisCache));
            JsonConvert.DefaultSettings = () =>//默认序列化
            {
                return new JsonSerializerSettings().Custom();
            };

            //flurl全局配置
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; };
            FlurlHttp.Configure(settings =>
            {
                settings.HttpClientFactory = new SSLHttpClientFactory();
                settings.Timeout = TimeSpan.FromSeconds(10);
                settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings().Custom());
            });

            //添加全局HttpContext
            services.AddStaticHttpContext();
            //数据库
            services.AddDbContext(config);

            //分布式锁
            services.AddDistributedLock(config);

            //任务调度
            services.AddQuartzServer();

            //事件总线
            services.AddEventBus();

            //微信
            services.AddWeixin(config.GetSection(OptionName.Weixin));
            return services;
        }

        /// <summary>
        /// 分布式缓存
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration config)
        {
            var cacheOptions = config.GetSection(OptionName.RedisCache).Get<RedisCacheOptions>();
            if (string.IsNullOrWhiteSpace(cacheOptions.Configuration))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheOptions.Configuration;
                    options.InstanceName = cacheOptions.InstanceName;
                    options.ConfigurationOptions = cacheOptions.ConfigurationOptions;
                });
            }
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