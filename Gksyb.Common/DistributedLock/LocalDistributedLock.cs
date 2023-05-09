using System.Runtime.CompilerServices;
using System.Threading;

namespace Gksyb.Common.DistributedLock
{
    /// <summary>
    /// 分布式锁本地实现
    /// </summary>
    public class LocalDistributedLock : IDistributedLock
    {
        /// <summary>
        /// 锁存放容器
        /// </summary>
        private static readonly Dictionary<string, LockValue> _lockPool = new();

        /// <summary>
        /// 随机数
        /// </summary>
        private static readonly Random _rnd = new();

        ~LocalDistributedLock()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public string Lock(string key, double expiry, int retry = 20, int delay = 10)
        {
            var value = GuidHelper.NewShortId();
            var min = delay * 2 / 3;
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    var isLock = LockTake(key, value, expiry);
                    if (isLock) return value;
                    if ((i + 1) == retry) break;
                    Thread.Sleep(_rnd.Next(min, delay));
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<string> LockAsync(string key, double expiry, int retry = 20, int delay = 10)
        {
            await Task.CompletedTask;
            return Lock(key, expiry, retry, delay);
        }

        /// <inheritdoc/>
        public void UnLock(string key, string value, int retry = 20, int delay = 10)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (string.IsNullOrWhiteSpace(value)) return;
            Exception exception = null;
            var min = delay * 2 / 3;
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    if (LockRelease(key, value)) return;
                    if ((i + 1) == retry) break;
                    Thread.Sleep(_rnd.Next(min, delay));
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            exception ??= new MessageException("释放锁超时");
            throw exception;
        }

        /// <inheritdoc/>
        public async Task UnLockAsync(string key, string value, int retry = 20, int delay = 10)
        {
            await Task.CompletedTask;
            UnLock(key, value, retry, delay);
        }

        /// <inheritdoc/>
        public string LockQuery(string key)
        {
            _lockPool.TryGetValue(key, out var lockValue);
            return lockValue?.Value;
        }

        /// <inheritdoc/>
        public async Task<string> LockQueryAsync(string key)
        {
            await Task.CompletedTask;
            return LockQuery(key);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool LockTake(string key, string value, double expiry)
        {
            if (_lockPool.TryGetValue(key, out var lockValue))
            {
                if (lockValue.Expiry <= DateTime.UtcNow) _lockPool.Remove(key);
            }
            if (_lockPool.ContainsKey(key)) return false;
            lockValue = new LockValue()
            {
                Key = key,
                Value = value,
                Expiry = DateTime.UtcNow.AddMilliseconds(expiry)
            };
            return _lockPool.TryAdd(key, lockValue);
        }

        /// <inheritdoc/>
        public async Task<bool> LockTakeAsync(string key, string value, double expiry)
        {
            await Task.CompletedTask;
            return LockTake(key, value, expiry);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool LockExtend(string key, string value, double expiry)
        {
            if (!_lockPool.TryGetValue(key, out var lockValue)) return false;
            var newValue = new LockValue()
            {
                Key = key,
                Value = value,
                Expiry = DateTime.UtcNow.AddMilliseconds(expiry)
            };
            if (lockValue.Expiry >= DateTime.UtcNow && lockValue.Value != value)
            {
                return false;
            }
            _lockPool[key] = newValue;
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> LockExtendAsync(string key, string value, double expiry)
        {
            await Task.CompletedTask;
            return LockExtend(key, value, expiry);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool LockRelease(string key, string value)
        {
            if (_lockPool.TryGetValue(key, out var lockValue))
            {
                if (lockValue.Expiry > DateTime.UtcNow && lockValue.Value != value)
                {
                    return false;
                }
                return _lockPool.Remove(key);
            }
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> LockReleaseAsync(string key, string value)
        {
            await Task.CompletedTask;
            return LockRelease(key, value);
        }

        private bool _disposed = false;

        private void Dispose(bool _)
        {
            if (_disposed) return;
            _disposed = true;
        }
    }

    internal class LockValue
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? Expiry { get; set; }
    }
}