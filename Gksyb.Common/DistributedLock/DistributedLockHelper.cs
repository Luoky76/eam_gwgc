using Gksyb.Common.DistributedLock;
using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Common
{
    /// <summary>
    /// 分布式锁接口
    /// </summary>
    public static class DistributedLockHelper
    {
        private static IDistributedLock _distributedLock;

        private static IDistributedLock DistributedLock
        {
            get
            {
                if (_distributedLock != null) return _distributedLock;
                _distributedLock = HttpContext.RequestServices.GetService<IDistributedLock>();
                return _distributedLock;
            }
        }

        /// <summary>
        /// 获取分布式锁
        /// </summary>
        public static IDistributedLock GetDistributedLock()
        {
            return DistributedLock;
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns>返回空代表加锁失败</returns>
        public static string GetLock(string key, double expiry, int retry = 20, int delay = 10)
        {
            return DistributedLock.Lock(key, expiry, retry, delay);
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns>返回空代表加锁失败</returns>
        public static async Task<string> GetLockAsync(string key, double expiry, int retry = 20, int delay = 10)
        {
            return await DistributedLock.LockAsync(key, expiry, retry, delay);
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <param name="action">动作(isFail)</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns>返回空代表加锁失败</returns>
        public static void Lock(string key, double expiry, Action<bool> action, int retry = 20, int delay = 10)
        {
            string value = null;
            try
            {
                value = DistributedLock.Lock(key, expiry, retry, delay);
                action(string.IsNullOrWhiteSpace(value));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                DistributedLock.UnLock(key, value, retry, delay);
            }
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <param name="func">动作(isFail)</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns></returns>
        public static async Task LockAsync(string key, double expiry, Func<bool, Task> func, int retry = 20, int delay = 10)
        {
            string value = null;
            var isFail = true;
            try
            {
                value = await DistributedLock.LockAsync(key, expiry, retry, delay);
                isFail = string.IsNullOrWhiteSpace(value);
                await func(isFail);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!isFail)
                {
                    await DistributedLock.UnLockAsync(key, value, retry, delay);
                }
            }
        }

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <returns>锁对应的值</returns>
        public static string LockQuery(string key)
        {
            return DistributedLock.LockQuery(key);
        }

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <returns>锁对应的值</returns>
        public static async Task<string> LockQueryAsync(string key)
        {
            return await DistributedLock.LockQueryAsync(key);
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        public static bool LockTake(string key, string value, double expiry)
        {
            return DistributedLock.LockTake(key, value, expiry);
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        public static async Task<bool> LockTakeAsync(string key, string value, double expiry)
        {
            return await DistributedLock.LockTakeAsync(key, value, expiry);
        }

        /// <summary>
        /// 更新锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        public static bool LockExtend(string key, string value, double expiry)
        {
            return DistributedLock.LockExtend(key, value, expiry);
        }

        /// <summary>
        /// 更新锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        public static async Task<bool> LockExtendAsync(string key, string value, double expiry)
        {
            return await DistributedLock.LockExtendAsync(key, value, expiry);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <returns></returns>
        public static bool LockRelease(string key, string value)
        {
            return DistributedLock.LockRelease(key, value);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <returns></returns>
        public static async Task<bool> LockReleaseAsync(string key, string value)
        {
            return await DistributedLock.LockReleaseAsync(key, value);
        }
    }
}