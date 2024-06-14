using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Reflection;

namespace Gksyb.Common
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public interface IService
    {
    }

    public static class IServiceExtension
    {
        /// <summary>
        /// 批量注册IService的派生接口
        /// </summary>
        public static void AddIService(this Assembly source)
        {
            source.GetTypes().AddIService();
        }

        /// <summary>
        /// 批量注册IService的派生接口
        /// </summary>
        public static void AddIService(this Type[] types)
        {
            var type = typeof(IService);
            types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract).ForEach(c =>//动态注册
            {
                var lifeTime = c.GetAttribute<ServiceLifetimeAttribute>()?.Lifetime;
                c.GetInterfaces().ForEach(t =>
                {
                    if (type == t || !type.IsAssignableFrom(t) || t.GetAttribute<ServiceLifetimeAttribute>()?.SkipDependency == true) return;
                    switch (lifeTime)
                    {
                        case ServiceLifetime.Singleton:
                            HttpContext.ServiceCollection.AddSingleton(t, c);
                            break;

                        case ServiceLifetime.Transient:
                            HttpContext.ServiceCollection.AddTransient(t, c);
                            break;

                        default:
                            HttpContext.ServiceCollection.AddScoped(t, c);
                            break;
                    }
                });
            });
        }

        /// <summary>
        /// 注册所有的service
        /// </summary>
        /// <param name="source"></param>
        public static void AddAllService(this Assembly source)
        {
            var types = source.GetTypes();
            types.AddIService();
            types.AddIBaseService();
        }
    }
}