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
                ErrorHandle(json);
            }
        }

        protected override async Task ErrorHandle(Exception ex)
        {
            ErrorHandle(ex.ToString());
            await base.ErrorHandle(ex);
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        private void ErrorHandle(string content)
        {
            var _methods = (_quartzTask.TaskErrorMethod ?? "").Split(";").Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            foreach (var method in _methods)
            {
                try
                {
                    var serviceType = Type.GetType(method, false);
                    var noticeHandle = _serviceProvider.GetService(serviceType) as INoticeHandle;
                    _ = noticeHandle?.Excute("DbJob", content);
                }
                catch
                {
                }
            }
        }
    }
}