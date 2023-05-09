using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Gksyb.Common.Data
{
    public static class DbContextServiceCollectionExtensions
    {
        /// <summary>
        /// 数据库
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration config)
        {
            var dbConfiguration = config.GetSection(OptionName.DataBase);
            var isDbLog = (dbConfiguration["SQLLog"] ?? "").ToLower() == "true";
            _ = int.TryParse(dbConfiguration["SlowQuery"] ?? "0", out int slowQUery);

            //映射处理
            var mappingTypeBuilder = DbConfiguration.ConfigureMappingType<object>();
            mappingTypeBuilder.HasDbValueConverter<ObjectMappingType>();
            mappingTypeBuilder = DbConfiguration.ConfigureMappingType<string>();
            mappingTypeBuilder.HasDbType(DbType.AnsiString);
            mappingTypeBuilder = DbConfiguration.ConfigureMappingType<DateTime>();
            mappingTypeBuilder.HasDbParameterAssembler<DateTimeMappingType>();
            mappingTypeBuilder = DbConfiguration.ConfigureMappingType<Guid>();
            mappingTypeBuilder.HasDbParameterAssembler<Guid_MappingType>();

            //全局拦截器
            DbInterception.GetInterceptors().ForEach(c => DbInterception.Remove(c));
            DbContextInterception.GetInterceptors().ForEach(c => DbContextInterception.Remove(c));
            DbInterception.Add(new DbCommandInterceptor(isDbLog, slowQUery));
            DbContextFactory.SetDefault(dbConfiguration["DbType"], dbConfiguration["ConnectionString"]);

            services.AddScoped(c => DbContextFactory.CreateContext());
            return services;
        }
    }
}