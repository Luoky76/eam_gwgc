using Gksyb.Common.EventBus;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusServiceCollectionExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventStorer, EventStorer>();
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddSingleton<IHostedService, EventBusHostedService>();
            return services;
        }
    }
}