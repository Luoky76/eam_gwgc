using Gksyb.Common;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        /// <summary>
        /// 从缓存获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this IDistributedCache source, string key)
        {
            return typeof(T).IsComplexType() ? source.GetString(key).ToObject<T>() : source.GetString(key).CastTo<T>();
        }

        /// <summary>
        /// 从缓存获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Get<T>(this IDistributedCache source, string key, T defaultValue)
        {
            if (typeof(T).IsComplexType())
            {
                try
                {
                    return source.GetString(key).ToObject<T>();
                }
                catch
                {
                    return defaultValue;
                }
            }
            return source.GetString(key).CastTo<T>(defaultValue);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache source, string key)
        {
            var value = await source.GetStringAsync(key);
            if (value == null) return default;
            return typeof(T).IsComplexType() ? value.ToObject<T>() : value.CastTo<T>();
        }

        /// <summary>
        /// 从缓存获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache source, string key, T defaultValue)
        {
            try
            {
                var value = await source.GetStringAsync(key);
                if (value == null) return defaultValue;
                return typeof(T).IsComplexType() ? value.ToObject<T>() : value.CastTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public static void Set<T>(this IDistributedCache source, string key, T value, DistributedCacheEntryOptions options = null)
        {
            source.SetString(key, typeof(T).IsComplexType() ? value.ToMiniJson() : value.ToString(), options ?? new DistributedCacheEntryOptions());
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Task SetAsync<T>(this IDistributedCache source, string key, T value, DistributedCacheEntryOptions options = null)
        {
            return source.SetStringAsync(key, typeof(T).IsComplexType() ? value.ToMiniJson() : value.ToString(), options ?? new DistributedCacheEntryOptions());
        }

        /// <summary>
        /// 限制重试次数
        /// </summary>
        /// <returns></returns>
        public static async Task<AjaxResult> LimitRetry(this IDistributedCache source, string key, string error, Func<Task<AjaxResult>> func, int limit = 3, int minitues = 3)
        {
            var retryCount = await source.GetAsync<int?>(key) ?? 0;
            if (retryCount >= limit) return AjaxResult.Error(error);
            var result = await func();
            if (result.IsError)
            {
                await source.SetAsync(key, (++retryCount), new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minitues)
                });
                return result;
            }
            if (retryCount > 0) await source.RemoveAsync(key);
            return result;
        }

        /// <summary>
        /// 是否复杂类型
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static bool IsComplexType(this Type source) => !TypeDescriptor.GetConverter(source).CanConvertFrom(typeof(string));

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
    }
}