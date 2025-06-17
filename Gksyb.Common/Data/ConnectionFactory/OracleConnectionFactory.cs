using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Oracle;
using Chloe.RDBMS;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Gksyb.Common.Data
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        static OracleConnectionFactory()//初始化
        {
            OracleConfiguration.SuppressGetDecimalInvalidCastException = true;
            OracleConfiguration.SuppressErrorURL = true;
            OracleConfiguration.SqlNetAllowedLogonVersionClient = OracleAllowedLogonVersionClient.Version8;
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler("WM_CONCAT",null)},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()}
            };
            foreach (var item in methodHandlerDic)
            {
                OracleContext.SetMethodHandler(item.Key, item.Value);
            }
        }

        private readonly string _connString = null;
        private readonly bool _hasTAFMod = false;

        public OracleConnectionFactory(string connString)
        {
            _connString = connString;
            _hasTAFMod = _connString.Contains("TAFMode");
            if (_hasTAFMod) _connString = _connString.Replace("TAFMode", "");
        }

        public IDbConnection CreateConnection()
        {
            var oracleConnection = new OracleConnection(_connString);
            if (_hasTAFMod)
            {
                oracleConnection.TAFMode = new OracleTAFMode(OracleFailoverType.None, OracleFailoverRestore.NONE);
            }
            IDbConnection conn = new OracleConnectionDecorator(oracleConnection);
            return conn;
        }
    }

    /// <summary>
    /// 该装饰器主要修改参数绑定方式。
    /// </summary>
    internal class OracleConnectionDecorator : DbConnectionDecorator
    {
        private readonly OracleConnection _oracleConnection;

        public OracleConnectionDecorator(OracleConnection oracleConnection) : base(oracleConnection)
        {
            _oracleConnection = oracleConnection;
        }

        public override IDbCommand CreateCommand()
        {
            return new OracleCommandDecorator(_oracleConnection.CreateCommand());
        }
    }

    internal class OracleCommandDecorator : DbCommandDecorator
    {
        private readonly OracleCommand _oracleCommand;

        public OracleCommandDecorator(OracleCommand oracleCommand) : base(oracleCommand)
        {
            _oracleCommand = oracleCommand;
            _oracleCommand.BindByName = true;
            _oracleCommand.InitialLONGFetchSize = -1;//立即查询LONG和LONG RAW
            _oracleCommand.InitialLOBFetchSize = -1;//立即查询CLOB
        }
    }
}