using Gksyb.Common.DistributedLock;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedLockServiceCollectionExtensions
    {
        /// <summary>
        /// 分布式锁调度
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddDistributedLock(this IServiceCollection services, IConfiguration config)
        {
            var cacheOptions = config.GetSection(OptionName.RedisCache).Get<RedisCacheOptions>();
            if (!string.IsNullOrWhiteSpace(cacheOptions.Configuration) && cacheOptions.ConfigurationOptions == null)
            {
                services.AddSingleton<IDistributedLock, RedisDistributedLock>();
                return services;
            }
            services.AddSingleton<IDistributedLock, LocalDistributedLock>();
            return services;
        }
    }
}