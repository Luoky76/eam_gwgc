using Azure;
using Gksyb.Common.Quartz;
using Gksyb.Core.Common;
using Gksyb.Core.Interfaces.Common;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.RegularExpressions;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// 数据库调用
    /// </summary>
    public class DbJob : BaseJob
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonService _commonService;
        private readonly IServiceProvider _serviceProvider;

        public DbJob(IDbContext dbContext, ICommonService commonService, IServiceProvider serviceProvider, ILogger<DbJob> logger) : base(logger, nameof(DbJob))
        {
            _dbContext = dbContext;
            _commonService = commonService;
            _serviceProvider = serviceProvider;
        }

        public override async Task Excute()
        {
            if (_quartzTask == null || string.IsNullOrWhiteSpace(_quartzTask.TaskView)) return;
            Dictionary<string, object> param = null;
            try
            {
                param = string.IsNullOrWhiteSpace(_quartzTask.TaskData) ? null : _quartzTask.TaskData.ToObject<Dictionary<string, object>>();
            }
            catch (Exception)
            {
            }
            var list = await _commonService.JsonValueAsync<dynamic>(new QueryViewRequest()
            {
                ViewName = _quartzTask.TaskView,
                Param = param
            });
            var json = list?.Count == 1 ? list[0].ToJson() : list.ToJson();
            if (!string.IsNullOrWhiteSpace(_quartzTask.TaskErrorMatch) && Regex.IsMatch(json, _quartzTask.TaskErrorMatch))//符合错误匹配
            {
                await ErrorHandle(json);
            }
        }

        protected override async Task ErrorHandle(Exception ex)
        {
            await ErrorHandle(ex.ToString());
            await base.ErrorHandle(ex);
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        private async Task ErrorHandle(string content)
        {
            var _methods = (_quartzTask.TaskErrorMethod ?? "").Split(";").Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            await Parallel.ForEachAsync(_methods, async (method, token) =>
            {
                try
                {
                    var serviceType = Type.GetType(method, false);
                    if (_serviceProvider.GetService(serviceType) is INoticeHandle noticeHandle)
                        await noticeHandle.Excute("DbJob", content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(_logPath, $"{ex}");
                }
            });
        }
    }
}