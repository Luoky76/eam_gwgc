using Flurl.Http;
using Flurl.Http.Content;
using Gksyb.Common.Quartz;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.RegularExpressions;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// API调用
    /// </summary>
    public class ApiJob : BaseJob
    {
        private readonly IDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private ApiInvoke _apiInvoke;

        public ApiJob(IDbContext dbContext, IServiceProvider serviceProvider, ILogger<ApiJob> logger) : base(logger, nameof(ApiJob))
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public override async Task Excute()
        {
            if (_quartzTask == null || string.IsNullOrWhiteSpace(_quartzTask.TaskData)) return;
            _apiInvoke = _quartzTask.TaskData.ToObject<ApiInvoke>();
            if (string.IsNullOrWhiteSpace(_apiInvoke.Url)) return;
            var response = await _apiInvoke.Url.SendAsync(_apiInvoke.Verb, _apiInvoke.HttpContent).ReceiveString();
            if (!string.IsNullOrWhiteSpace(_quartzTask.TaskErrorMatch) && Regex.IsMatch(response, _quartzTask.TaskErrorMatch))//符合错误匹配
            {
                ErrorHandle(response);
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
            if (_apiInvoke == null)
            {
                return;
            }
            var _methods = (_quartzTask.TaskErrorMethod ?? "").Split(";").Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var type = string.IsNullOrWhiteSpace(_apiInvoke.ErrorType) ? "API" : _apiInvoke.ErrorType;
            foreach (var method in _methods)
            {
                try
                {
                    var serviceType = Type.GetType(method, false);
                    var noticeHandle = _serviceProvider.GetService(serviceType) as INoticeHandle;
                    _ = noticeHandle?.Excute(type, content);
                }
                catch
                {
                }
            }
        }
    }

    public class ApiInvoke
    {
        /// <summary>
        /// 调用地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// HTTP 动作 GET;POST
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 内容体
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 内容体
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        public string ErrorType { get; set; }

        public HttpMethod Verb
        {
            get
            {
                var action = (Action ?? "").ToLower();
                return action switch
                {
                    "" or "post" => HttpMethod.Post,
                    "get" => HttpMethod.Get,
                    "put" => HttpMethod.Put,
                    "delete" => HttpMethod.Delete,
                    "head" => HttpMethod.Head,
                    "options" => HttpMethod.Options,
                    "patch" => HttpMethod.Patch,
                    "trace" => HttpMethod.Trace,
                    _ => HttpMethod.Post,
                };
            }
        }

        public HttpContent HttpContent
        {
            get
            {
                var contentType = (ContentType ?? "").ToLower();
                var content = Content ?? "";
                return contentType switch
                {
                    "" or "json" => new CapturedJsonContent(content),
                    "urlencoded" => new CapturedUrlEncodedContent(content),
                    "string" => new CapturedStringContent(content),
                    "file" => new FileContent(content),
                    _ => new CapturedStringContent(content)
                };
            }
        }
    }
}