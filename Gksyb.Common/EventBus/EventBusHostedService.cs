using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件总线后台主机服务
    /// </summary>
    internal sealed class EventBusHostedService : BackgroundService
    {
        private readonly LogPath _logPath = new("EventBus");

        /// <summary>
        /// 日志对象
        /// </summary>
        private readonly ILogger<EventBusHostedService> _logger;

        /// <summary>
        /// 事件源存储器
        /// </summary>
        private readonly IEventStorer _eventSourceStorer;

        /// <summary>
        /// 事件源存储器
        /// </summary>
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EventBusHostedService(ILogger<EventBusHostedService> logger, IServiceScopeFactory serviceScopeFactory, IEventStorer eventSourceStorer)
        {
            _logger = logger;
            _eventSourceStorer = eventSourceStorer;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// 执行后台任务
        /// </summary>
        /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
        /// <returns><see cref="Task"/> 实例</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(_logPath, "开始运行事件总线服务");

            // 注册后台主机服务停止监听
            stoppingToken.Register(() =>
                _logger.LogDebug(_logPath, "正在退出事件总线"));

            // 监听服务是否取消
            while (!stoppingToken.IsCancellationRequested)
            {
                // 执行具体任务
                await ReadAsync(stoppingToken);
            }

            _logger.LogCritical(_logPath, "退出事件总线");
        }

        /// <summary>
        /// 读取事件进行处理
        /// </summary>
        /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
        /// <returns><see cref="Task"/> 实例</returns>
        private async Task ReadAsync(CancellationToken stoppingToken)
        {
            try
            {
                // 从事件存储器中读取一条
                var message = await _eventSourceStorer.ReadAsync(stoppingToken);
                if (message == null) return;
                _logger.LogInformation(_logPath, $"接到广播:{message}");
                var actionData = message.ToObject<ActionData<JToken>>();
                var eventHandlers = EventBusStore.EventHandlers.Where(c => c.EventId == actionData.Action).ToList();
                using var scope = _serviceScopeFactory.CreateAsyncScope();
                foreach (var eventHandler in eventHandlers)
                {
                    try
                    {
                        var values = eventHandler.Handler.GetParametersValue(actionData.Data);
                        object obj = null;
                        if (!eventHandler.Handler.IsStatic)
                        {
                            obj = scope.ServiceProvider.GetService(eventHandler.Handler.DeclaringType);
                        }
                        _logger.LogInformation(_logPath, $"触发事件:{eventHandler.Handler.DeclaringType}:{eventHandler.Handler.Name}");
                        var invokeResult =  eventHandler.Handler!.Invoke(obj, values);
                        if (invokeResult is Task task) await task;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(_logPath, ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
        }
    }
}