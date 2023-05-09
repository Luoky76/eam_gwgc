using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Threading;

namespace Gksyb.Common.DistributedLock
{
    /// <summary>
    /// 分布式锁Redis实现
    /// </summary>
    public class RedisDistributedLock : IDistributedLock
    {
        /// <summary>
        /// 随机数
        /// </summary>
        private static readonly Random _rnd = new();

        private readonly LogPath _logPath = new("RedisDistributedLock");
        private bool _disposed;
        private readonly RedisCacheOptions _options;
        private readonly ILogger<RedisDistributedLock> _logger;
        private volatile IConnectionMultiplexer _connection;
        private IDatabase _redis;
        private readonly SemaphoreSlim _connectionLock = new(initialCount: 1, maxCount: 1);
        private readonly string _instance;

        public RedisDistributedLock(ILogger<RedisDistributedLock> logger, IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null) throw new ArgumentNullException(nameof(optionsAccessor));
            _logger = logger;
            _options = optionsAccessor.Value;
            _instance = $"{_options.InstanceName ?? string.Empty}Lock_";
        }

        ~RedisDistributedLock()
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
            var value = GuidHelper.NewShortId();
            var min = delay * 2 / 3;
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    var isLock = await LockTakeAsync(key, value, expiry);
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
        public void UnLock(string key, string value, int retry = 20, int delay = 10)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (string.IsNullOrWhiteSpace(value)) return;
            Exception exception = null;
            ConnectAsync().Result();
            key = _instance + key;
            var min = delay * 2 / 3;
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    if (_redis.LockRelease(key, value)) return;
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
            if (string.IsNullOrWhiteSpace(key)) return;
            if (string.IsNullOrWhiteSpace(value)) return;
            Exception exception = null;
            await ConnectAsync();
            key = _instance + key;
            var min = delay * 2 / 3;
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    if (await _redis.LockReleaseAsync(key, value)) return;
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
        public string LockQuery(string key)
        {
            ConnectAsync().Result();
            key = _instance + key;
            return _redis.LockQuery(key);
        }

        /// <inheritdoc/>
        public async Task<string> LockQueryAsync(string key)
        {
            await ConnectAsync();
            key = _instance + key;
            return await _redis.LockQueryAsync(key);
        }

        /// <inheritdoc/>
        public bool LockTake(string key, string value, double expiry)
        {
            ConnectAsync().Result();
            key = _instance + key;
            return _redis.LockTake(key, value, TimeSpan.FromMilliseconds(expiry));
        }

        /// <inheritdoc/>
        public async Task<bool> LockTakeAsync(string key, string value, double expiry)
        {
            await ConnectAsync();
            key = _instance + key;
            return await _redis.LockTakeAsync(key, value, TimeSpan.FromMilliseconds(expiry));
        }

        /// <inheritdoc/>
        public bool LockExtend(string key, string value, double expiry)
        {
            ConnectAsync().Result();
            key = _instance + key;
            return _redis.LockExtend(key, value, TimeSpan.FromMilliseconds(expiry));
        }

        /// <inheritdoc/>
        public async Task<bool> LockExtendAsync(string key, string value, double expiry)
        {
            await ConnectAsync();
            key = _instance + key;
            return await _redis.LockExtendAsync(key, value, TimeSpan.FromMilliseconds(expiry));
        }

        /// <inheritdoc/>
        public bool LockRelease(string key, string value)
        {
            ConnectAsync().Result();
            key = _instance + key;
            return _redis.LockRelease(key, value);
        }

        /// <inheritdoc/>
        public async Task<bool> LockReleaseAsync(string key, string value)
        {
            await ConnectAsync();
            key = _instance + key;
            return await _redis.LockReleaseAsync(key, value);
        }

        private async Task ConnectAsync(CancellationToken token = default)
        {
            CheckDisposed();
            token.ThrowIfCancellationRequested();
            if (_redis != null) return;
            await _connectionLock.WaitAsync(token);
            try
            {
                if (_redis == null)
                {
                    if (_options.ConnectionMultiplexerFactory is null)
                    {
                        if (_options.ConfigurationOptions is not null)
                        {
                            _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions);
                        }
                        else
                        {
                            _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                        }
                    }
                    else
                    {
                        _connection = await _options.ConnectionMultiplexerFactory();
                    }
                    TryRegisterProfiler();
                    _redis = _connection.GetDatabase();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void TryRegisterProfiler()
        {
            _ = _connection ?? throw new InvalidOperationException($"{nameof(_connection)} cannot be null.");

            if (_options.ProfilingSession != null)
            {
                _connection.RegisterProfiler(_options.ProfilingSession);
            }
        }

        private void Dispose(bool _)
        {
            if (_disposed) return;
            _connection?.Dispose();
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}