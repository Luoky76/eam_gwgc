using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Oracle;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;

namespace Gksyb.Common.Data
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        static OracleConnectionFactory()//初始化
        {
            OracleContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
        }

        private readonly string _connString = null;

        public OracleConnectionFactory(string connString)
        {
            _connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            var oracleConnection = new OracleConnection(_connString);
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

        public override ConnectionState State
        {
            get
            {
                try
                {
                    return _oracleConnection.State;//m_oracleConnectionImpl有可能为空
                }
                catch (Exception)
                {
                    return ConnectionState.Closed;
                }
            }
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

        public override int ExecuteNonQuery()
        {
            ExecuteNonQueryHandle();
            return _oracleCommand.ExecuteNonQuery();
        }

        public override async Task<int> ExecuteNonQueryAsync()
        {
            ExecuteNonQueryHandle();
            return await _oracleCommand.ExecuteNonQueryAsync();
        }

        public override IDataReader ExecuteReader()
        {
            DateTimeParamHandle();
            var reader = _oracleCommand.ExecuteReader();
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public override IDataReader ExecuteReader(CommandBehavior behavior)
        {
            DateTimeParamHandle();
            var reader = _oracleCommand.ExecuteReader(behavior);
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public override async Task<IDataReader> ExecuteReaderAsync()
        {
            DateTimeParamHandle();
            var reader = await _oracleCommand.ExecuteReaderAsync() as OracleDataReader;
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public override async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            DateTimeParamHandle();
            var reader = await _oracleCommand.ExecuteReaderAsync(behavior) as OracleDataReader;
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public override object ExecuteScalar()
        {
            DateTimeParamHandle();
            return _oracleCommand.ExecuteScalar();
        }

        public override async Task<object> ExecuteScalarAsync()
        {
            DateTimeParamHandle();
            return await _oracleCommand.ExecuteScalarAsync();
        }

        private static readonly OracleDbType[] DateTimeTypes = new OracleDbType[] { OracleDbType.Date, OracleDbType.TimeStamp };
        private static readonly ThreadLocal<Random> rnd = new(() => new Random());

        private void ExecuteNonQueryHandle()
        {
            if (Regex.IsMatch(_oracleCommand.CommandText, @"^\s+INSERT INTO ", RegexOptions.IgnoreCase)) return;
            DateTimeParamHandle();
        }

        /// <summary>
        /// 日期参数前加随机空格，防止Oracle缓存语句导致索引失效
        /// </summary>
        private void DateTimeParamHandle()
        {
            var paramNames = new List<string>();
            foreach (OracleParameter param in _oracleCommand.Parameters)
            {
                if (!DateTimeTypes.Contains(param.OracleDbType)) continue;
                paramNames.Add(param.ParameterName);
            }
            if (paramNames.Count < 1) return;
            var pattern = $"({string.Join('|', paramNames.Select(c => $"({c}\\b)"))})";
            var padding = string.Empty.PadLeft(rnd.Value.Next(50));
            _oracleCommand.CommandText = Regex.Replace(_oracleCommand.CommandText, pattern, $@"{padding}$1");
        }
    }
}