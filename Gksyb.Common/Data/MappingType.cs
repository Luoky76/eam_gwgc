using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Chloe.Infrastructure
{
    public class ObjectMappingType : DbValueConverter<object>
    {
        public override object Convert(object value)
        {
            return value;
        }
    }

    public class DateTimeMappingType : DbParameterAssembler
    {
        public override void SetupParameter(IDbDataParameter parameter, DbParam param)
        {
            if (!param.DbType.HasValue || param.DbType == DbType.Object)
            {
                param.DbType = (parameter is OracleParameter) ? DbType.Date : DbType.DateTime;
            }
            base.SetupParameter(parameter, param);
        }
    }

    /// <summary>
    /// 处理 Guid
    /// </summary>
    public class Guid_MappingType : DbParameterAssembler
    {
        public override void SetupParameter(IDbDataParameter parameter, DbParam param)
        {
            if (param.Value is Guid)
            {
                try
                {
                    parameter.Value = param.Value;
                }
                catch
                {
                    param.DbType = parameter.DbType;
                    param.Value = param.Value.ToString();
                }
            }
            base.SetupParameter(parameter, param);
        }
    }
}