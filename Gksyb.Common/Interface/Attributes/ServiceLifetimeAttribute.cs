using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Common
{
    /// <summary>
    /// 生命周期特性
    /// </summary>
    /// <remarks>
    /// <para>作用于 <see cref="IService"/>  <see cref="IBaseService"/> 实现类或接口</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public sealed class ServiceLifetimeAttribute : Attribute
    {
        /// <summary>
        /// 生命周期
        /// </summary>
        public readonly ServiceLifetime Lifetime;

        /// <summary>
        /// 跳过依赖注入
        /// </summary>
        public readonly bool SkipDependency;

        /// <summary>
        /// 生命周期
        /// </summary>
        /// <param name="skipDependency">忽略依赖注入</param>
        public ServiceLifetimeAttribute(bool skipDependency)
        {
            SkipDependency = skipDependency;
        }

        public ServiceLifetimeAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }
    }
}