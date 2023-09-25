using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Gksyb.Common
{
    public static class ServiceProviderServiceExtensions
    {
        /// <summary>
        /// 根据名称获取服务
        /// </summary>
        public static object GetService(this IServiceProvider source, string serviceType)
        {
            return source.GetService(c => c.ServiceType.FullName == serviceType);
        }

        /// <summary>
        /// 根据条件获取服务
        /// </summary>
        public static object GetService(this IServiceProvider source, Func<ServiceDescriptor, bool> func)
        {
            var collection = HttpContext.ServiceCollection;
            var descriptor = collection.Where(a => func(a)).LastOrDefault();
            if (descriptor == null) return null;
            var services = source.GetServices(descriptor.ServiceType).ToList();
            if (services.Count == 1) return services[0];
            return services.FirstOrDefault(c => c.GetType() == descriptor.ImplementationType);
        }

        /// <summary>
        /// 获取已映射的服务
        /// </summary>
        public static T GetResolvedServices<T>(this IServiceProvider source)
        {
            var services = HttpContext.ResolvedServicesGetter(source) as IDictionary;
            foreach (var item in services.Values)
            {
                if (item is T service) return service;
            }
            return default;
        }
    }
}