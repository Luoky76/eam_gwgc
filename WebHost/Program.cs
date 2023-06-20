using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            try
            {
                Log.Information("主机启动");
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);//设置当前路径
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "发生未处理的异常");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .UseSerilog((context, services, configuration) => configuration//配置Serilog
                    .CommonLoggerConfiguration(context.Configuration)
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services))
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = false;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    options.AddServerHeader = false;
                });
                webBuilder.UseStartup<Startup>();
            });

        /// <summary>
        /// 全局异常捕获
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal($"{{{nameof(LogPath)}}} {{message}}", _logPath, e.ExceptionObject?.ToString());
        }

        /// <summary>
        /// 记录任何未观察到的任务异常并防止进程终止
        /// </summary>
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Fatal($"{{{nameof(LogPath)}}} {{message}}", _logPath, e.Exception?.ToString());
            e.SetObserved();//防止进程终止
        }

        private static readonly LogPath _logPath = new("Exception");
    }
}