using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Reflection;

namespace Gksyb.Common
{
    /// <summary>
    /// 基础服务
    /// </summary>
    public interface IBaseService
    {
    }

    public static class BaseServiceExtension
    {
        /// <summary>
        /// 批量注册IBaseService的派生类
        /// </summary>
        public static void AddIBaseService(this Assembly source)
        {
            var type = typeof(IBaseService);
            source.GetTypes().Where(t => type.IsAssignableFrom(t) && !t.IsAbstract).ForEach(c =>//动态注册
            {
                var lifeTime = c.GetAttribute<ServiceLifetimeAttribute>()?.Lifetime;
                switch (lifeTime)
                {
                    case ServiceLifetime.Singleton:
                        HttpContext.ServiceCollection.AddSingleton(c);
                        break;

                    case ServiceLifetime.Transient:
                        HttpContext.ServiceCollection.AddTransient(c);
                        break;

                    default:
                        HttpContext.ServiceCollection.AddScoped(c);
                        break;
                }
            });
        }
    }
}