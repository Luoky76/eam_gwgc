namespace Gksyb.Common.DistributedLock
{
    /// <summary>
    /// 分布式锁接口
    /// </summary>
    public interface IDistributedLock : IDisposable
    {
        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns>返回空代表加锁失败</returns>
        string Lock(string key, double expiry, int retry = 20, int delay = 10);

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="expiry">锁过期时间</param>
        /// <param name="retry">重试次数</param>
        /// <param name="delay">每次等待时间（毫秒）</param>
        /// <returns>返回空代表加锁失败</returns>
        Task<string> LockAsync(string key, double expiry, int retry = 20, int delay = 10);

        /// <summary>
        /// 释放锁
        /// </summary>
        void UnLock(string key, string value, int retry = 20, int delay = 10);

        /// <summary>
        /// 释放锁
        /// </summary>
        Task UnLockAsync(string key, string value, int retry = 20, int delay = 10);

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <returns>锁对应的值</returns>
        string LockQuery(string key);

        /// <summary>
        /// 查询锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <returns>锁对应的值</returns>
        Task<string> LockQueryAsync(string key);

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        bool LockTake(string key, string value, double expiry);

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        Task<bool> LockTakeAsync(string key, string value, double expiry);

        /// <summary>
        /// 更新锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        bool LockExtend(string key, string value, double expiry);

        /// <summary>
        /// 更新锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <param name="expiry">锁过期时间（毫秒）</param>
        /// <returns></returns>
        Task<bool> LockExtendAsync(string key, string value, double expiry);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <returns></returns>
        bool LockRelease(string key, string value);

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁主键</param>
        /// <param name="value">锁值</param>
        /// <returns></returns>
        Task<bool> LockReleaseAsync(string key, string value);
    }
}