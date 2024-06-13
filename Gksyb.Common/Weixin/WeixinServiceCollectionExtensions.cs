using Gksyb.Common.Weixin;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WeixinServiceCollectionExtensions
    {
        public static IServiceCollection AddWeixin(this IServiceCollection services, IConfigurationSection config)
        {
            services.AddSingleton<IAccessTokenHandle, AccessTokenHandle>();
            WeixinSetting.InitFromConifg(config);
            return services;
        }
    }
}