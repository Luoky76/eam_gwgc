using Gksyb.Common;
using Gksyb.Common.Quartz.Dtos;
using Gksyb.Common.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Quartz
{
    public abstract class BaseJob : IJob
    {
        protected LogPath _logPath = null;
        protected readonly ILogger _logger;
        protected QuartzTask _quartzTask = null;
        protected IJobExecutionContext _context = null;
        protected IDistributedCache distributedCache = null;
        protected bool _isDistributedLock = false;

        /// <summary>
        /// 锁定超时时间 单位秒 默认两分钟
        /// </summary>
        protected double Expiry = 2 * 60;

        public BaseJob(ILogger logger, string path)
        {
            _logger = logger;
            _logPath = new(path);
            distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string taskKey = null;
            try
            {
                _quartzTask = (context.JobDetail.JobDataMap["QuartzTask"] as QuartzTask) ?? new QuartzTask()
                {
                    TaskID = GuidHelper.NewSnowflakeId()
                };
                _quartzTask.IsExcuted = false;
                var key = _quartzTask.TaskID.ToString();
                var isExcute = await distributedCache.GetStringAsync($"{key}{nameof(Expiry)}");
                if (isExcute == "1") return;
                _isDistributedLock = (_quartzTask.TaskIP ?? "").Split(",").DistinctAndOrderBy().Count() != 1;
                if (_isDistributedLock)//不是单IP，分布式锁处理
                {
                    var address = HttpContext.AddressList.ToStr(",");
                    var value = await DistributedLockHelper.LockQueryAsync(key);
                    if (!string.IsNullOrWhiteSpace(value) && value != address) return;//已被其他服务器锁定
                    var nextFireTimeUtc = context.NextFireTimeUtc ?? context.FireTimeUtc.AddSeconds(Expiry);
                    var expiry = (nextFireTimeUtc - context.FireTimeUtc).TotalSeconds * 3;
                    if (expiry < Expiry) expiry = Expiry;
                    expiry *= 1000;
                    //首次调度，为了延续之前IP，先锁定之前IP
                    var isChange = context.PreviousFireTimeUtc == null && !string.IsNullOrWhiteSpace(_quartzTask.LastRunIP) && _quartzTask.LastRunIP != address;
                    address = isChange ? _quartzTask.LastRunIP : address;
                    if (!await DistributedLockHelper.LockExtendAsync(key, address, expiry) && !await DistributedLockHelper.LockTakeAsync(key, address, expiry))//加入锁
                    {
                        return;
                    }
                    if (isChange) return;
                }
                taskKey = $"{key}{nameof(Expiry)}";
                await distributedCache.SetStringAsync(taskKey, "1", new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Expiry)
                });
                _quartzTask.IsExcuted = true;
                _context = context;
                await Excute();
            }
            catch (Exception ex)
            {
                try
                {
                    await ErrorHandle(ex);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(_logPath, $"ErrorHandle:{ex2}");
                }
                _logger.LogError(_logPath, ex.ToString());
                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(taskKey))
                {
                    await distributedCache.RemoveAsync(taskKey);
                }
            }
        }

        /// <summary>
        /// 执行job
        /// </summary>
        /// <returns></returns>
        public abstract Task Excute();

        /// <summary>
        ///错误处理
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ErrorHandle(Exception ex)
        {
            if (!_isDistributedLock) return;
            if (ex is MessageException) return;
            if (_quartzTask == null) return;
            var key = _quartzTask.TaskID.ToString();
            var address = HttpContext.AddressList.ToStr(",");
            await DistributedLockHelper.LockReleaseAsync(key, address);
        }
    }
}