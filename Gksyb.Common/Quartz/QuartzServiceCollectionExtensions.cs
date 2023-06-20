using Gksyb.Common.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quartz
{
    public static class QuartzServiceCollectionExtensions
    {
        /// <summary>
        /// 任务调度
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddQuartzServer(this IServiceCollection services)
        {
            services.AddQuartz(c =>
            {
                c.AddJobListener<JobListener>();
                c.UseJobFactory<JobFactory>();
            });
            services.AddScoped<IQuartzStore, QuartzStoreFromFile>();
            services.AddSingleton<IHostedService, GksybTaskHostedService>();
            return services;
        }
    }
}