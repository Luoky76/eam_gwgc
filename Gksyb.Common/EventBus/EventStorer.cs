using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Threading.Channels;

namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件存储器
    /// </summary>
    internal sealed partial class EventStorer : IEventStorer, IDisposable
    {
        /// <summary>
        /// 消息队列Group
        /// </summary>
        private const string MessageQueueGroup = "StreamGroup";

        private readonly LogPath _logPath = new("EventBus");
        private bool _disposed;
        private readonly RedisCacheOptions _options;
        private readonly ILogger<EventStorer> _logger;
        private readonly Channel<ActionData<string>> _channel;
        private static string _keyStream;
        private static string _groupName;
        private static string _consumerName;
        private static RedisChannel _channelName;
        private volatile IConnectionMultiplexer _connection;
        private IDatabase _redis;
        private ISubscriber _bus;
        private readonly SemaphoreSlim _connectionLock = new(initialCount: 1, maxCount: 1);
        private readonly bool _isLocal = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public EventStorer(ILogger<EventStorer> logger, IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null) throw new ArgumentNullException(nameof(optionsAccessor));
            _logger = logger;
            // 创建有限容量通道
            _channel = Channel.CreateBounded<ActionData<string>>(new BoundedChannelOptions(int.MaxValue)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            _options = optionsAccessor.Value;
            if (!string.IsNullOrWhiteSpace(_options.Configuration) && _options.ConfigurationOptions == null)
            {
                _isLocal = false;
                _keyStream = $"{_options.InstanceName ?? string.Empty}{nameof(EventStorer)}";
                _groupName = $"{_keyStream}_Group";
                _consumerName = $"{_keyStream}_Consumer";
                _channelName = new RedisChannel($"{_keyStream}_Channel", RedisChannel.PatternMode.Auto);
            }
            _logger.LogInformation(_logPath, $"启用{(_isLocal ? "本地" : "reids")}事件存储器");
        }

        /// <summary>
        /// 事件写入
        /// </summary>
        public async ValueTask WriteAsync(ActionData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            await _channel.Writer.WriteAsync(data.ToActionString());
        }

        /// <summary>
        /// 事件广播
        /// </summary>
        /// <returns></returns>
        public async ValueTask BroadcastAsync(ActionData data)
        {
            if (_isLocal)
            {
                await WriteAsync(data);
                return;
            }
            if (data == default) throw new ArgumentNullException(nameof(data));
            await ConnectAsync();
            await _bus!.PublishAsync(_channelName, data.ToActionString().ToJson(), CommandFlags.None);
        }

        /// <summary>
        /// 消息队列
        /// </summary>
        /// <returns></returns>
        public async ValueTask MessageQueueAsync(ActionData data)
        {
            if (_isLocal)
            {
                await WriteAsync(data);
                return;
            }
            await ConnectAsync();
            await _redis.StreamAddAsync(_keyStream, data.Action, data.ToActionString().ToJson(), null, int.MaxValue, false, CommandFlags.None);
            await _bus!.PublishAsync(_channelName, MessageQueueGroup, CommandFlags.None);
        }

        /// <summary>
        /// 事件读取
        /// </summary>
        public async ValueTask<ActionData<string>> ReadAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!_isLocal)
                {
                    await ConnectAsync(cancellationToken);
                }
                var data = await _channel.Reader.ReadAsync(cancellationToken);
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 从stream读取
        /// </summary>
        /// <returns></returns>
        private async ValueTask StreamReadGroupAsync(bool retry = true, bool isAll = false)
        {
            try
            {
                var count = isAll ? int.MaxValue : 1;
                var entrys = await _redis.StreamReadGroupAsync(_keyStream, _groupName, _consumerName,
                    StreamPosition.NewMessages, count, false, CommandFlags.None);
                if (entrys.Length < 1)
                {
                    if (!retry) return;
                    await Task.Delay(1000);
                    await StreamReadGroupAsync(false);
                    return;
                }
                foreach (var entry in entrys)
                {
                    foreach (var value in entry.Values)
                    {
                        var message = value.Value.ToString();
                        _logger.LogInformation(_logPath, $"接到来自{value.Name}的数据{message}");
                        await _channel.Writer.WriteAsync(message.ToObject<ActionData<string>>());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
        }

        /// <summary>
        /// 广播
        /// </summary>
        /// <returns></returns>
        private async Task SubscribeToAll()
        {
            var channel = await _bus!.SubscribeAsync(_channelName);
            channel.OnMessage(async channelMessage =>
            {
                try
                {
                    var message = channelMessage.Message.ToString();
                    if (message == MessageQueueGroup)
                    {
                        await ConnectAsync();
                        await StreamReadGroupAsync();
                        return;
                    }
                    _logger.LogInformation(_logPath, $"接到来自{channelMessage.Channel}的数据{message}");
                    await _channel.Writer.WriteAsync(message.ToObject<ActionData<string>>());
                }
                catch (Exception ex)
                {
                    _logger.LogError(_logPath, ex.ToString());
                }
            });
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
                    _bus = _connection.GetSubscriber();
                    await SubscribeToAll();
                    var isExists = false;
                    try
                    {
                        var group = await _redis.StreamGroupInfoAsync(_keyStream);
                        isExists = group.Length > 0;
                    }
                    catch (Exception)
                    {
                    }
                    if (!isExists)
                    {
                        await _redis.StreamCreateConsumerGroupAsync(_keyStream, _groupName, StreamPosition.NewMessages);
                    }
                    await StreamReadGroupAsync(false, true);
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _bus?.UnsubscribeAll();
            _connection?.Dispose();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}