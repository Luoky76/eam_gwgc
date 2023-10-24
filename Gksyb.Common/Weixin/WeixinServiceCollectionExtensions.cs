using Gksyb.Common.Weixin;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WeixinServiceCollectionExtensions
    {
        public static IServiceCollection AddWeixin(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IAccessTokenHandle, AccessTokenHandle>();
            services.AddSingleton<IMiniProgramAccessTokenHandle, MiniProgramAccessTokenHandle>();
            WeixinSetting.InitFromConifg(config.GetSection(OptionName.Weixin));
            MiniProgramSetting.InitFromConifg(config.GetSection(OptionName.MiniProgram));
            return services;
        }
    }
}