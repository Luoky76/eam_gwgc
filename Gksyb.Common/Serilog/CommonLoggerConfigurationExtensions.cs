using Gksyb.Common;
using Gksyb.Common.Static;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Serilog
{
    /// <summary>
    /// 日志通用设置
    /// </summary>
    public static class CommonLoggerConfigurationExtensions
    {
        private const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}:{HostIP}] {Message:lj}{NewLine}{Exception}";
        private const string KeyPropertyName = nameof(LogPath);
        private const string PathPropertyName = "MessagePath";
        private static readonly string HostIP = HttpContext.AddressList.LastOrDefault().Split('.').LastOrDefault();

        /// <summary>
        /// 日志通用设置
        /// </summary>
        public static LoggerConfiguration CommonLoggerConfiguration(this LoggerConfiguration loggerConfiguration, IConfiguration configuration = null, string directory = null)
        {
            configuration ??= new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{Environments.Production}.json", true)
                .Build();
            directory ??= Path.Combine(AppContext.BaseDirectory, configuration.GetValue("Serilog:File:FileName", defaultValue: "logs"));
            var isWriteToFile = configuration.GetValue("Serilog:WriteToFile", defaultValue: true);
            var isShare = isWriteToFile && configuration.GetValue<string>($"{OptionName.RedisCache}:Configuration").HasValue();
            var retainedFileCountLimit = configuration.GetValue("Serilog:File:CountLimit", defaultValue: 24 * 31);
            var fileSizeLimitBytes = configuration.GetValue("Serilog:File:SizeLimitBytes", defaultValue: 20) * 1024 * 1024;
            loggerConfiguration
        .WriteTo.Map(le =>//支持分路径写入文件 必须在最前面 因为会更新LogPath
        {
            string path = null;
            if (le.Properties.TryGetValue(KeyPropertyName, out var v) && v is ScalarValue sv)
            {
                path = sv.Value?.ToString();
            }
            path = string.IsNullOrWhiteSpace(path) ? "Application" : path;
            le.AddOrUpdateProperty(new LogEventProperty(KeyPropertyName, new ScalarValue("")));
            le.AddPropertyIfAbsent(new LogEventProperty(PathPropertyName, new ScalarValue(path)));
            le.AddOrUpdateProperty(new LogEventProperty(nameof(HostIP), new ScalarValue(HostIP)));
            return path;
        }, (path, configuration) =>
        {
            if (!isWriteToFile) return;
            path = Path.Combine(directory, $@"{path}/log-.txt");
            configuration.Async(configure =>//异步写入
            {
                configure.File(path, retainedFileCountLimit: retainedFileCountLimit, fileSizeLimitBytes: fileSizeLimitBytes, rollOnFileSizeLimit: true, shared: isShare, rollingInterval: RollingInterval.Day, outputTemplate: DefaultOutputTemplate);
            }, bufferSize: 10000);
        });
            return loggerConfiguration;
        }
    }
}