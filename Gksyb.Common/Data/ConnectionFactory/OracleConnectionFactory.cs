using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Oracle;
using Chloe.RDBMS;
using Chloe.Reflection;
using Chloe.Reflection.Emit;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gksyb.Common.Data
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        static OracleConnectionFactory()//初始化
        {
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
        private readonly bool _isInit = false;

        public OracleConnectionFactory(string connString)
        {
            _connString = connString;
            _isInit = _connString.Contains("SessionInit");
            if (_isInit) _connString = _connString.Replace("SessionInit", "");
        }

        public IDbConnection CreateConnection()
        {
            var oracleConnection = new OracleConnection(_connString);
            IDbConnection conn = new OracleConnectionDecorator(oracleConnection, _isInit);
            return conn;
        }
    }

    /// <summary>
    /// 该装饰器主要修改参数绑定方式。
    /// </summary>
    internal class OracleConnectionDecorator : DbConnectionDecorator
    {
        private static readonly MemberGetter IsNewConGetter;

        static OracleConnectionDecorator()
        {
            IsNewConGetter = DelegateGenerator.CreateGetter(typeof(OracleConnection).GetProperty("m_bNewConCreated",
               BindingFlags.NonPublic | BindingFlags.Instance));
        }

        private readonly OracleConnection _oracleConnection;
        private readonly bool _isInit = false;

        public OracleConnectionDecorator(OracleConnection oracleConnection, bool isInit) : base(oracleConnection)
        {
            _oracleConnection = oracleConnection;
            _isInit = isInit;
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

        public override async Task OpenAsync()
        {
            await base.OpenAsync();
            SessionInit();
        }

        public override void Open()
        {
            base.Open();
            SessionInit();
        }

        /// <summary>
        /// 当前会话参数初始化
        /// </summary>
        private void SessionInit()
        {
            if (!_isInit) return;
            try
            {
                if (_oracleConnection.State != ConnectionState.Open) return;
                var isNew = IsNewConGetter(_oracleConnection).CastTo(true);
                if (!isNew) return;
                using var cmd = _oracleConnection.CreateCommand();
                cmd.CommandText = "ALTER SESSION SET \"_serial_direct_read\"=never";
            }
            catch (Exception)
            {
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
                if (!param.ParameterName.StartsWith(':')) continue;
                if (!DateTimeTypes.Contains(param.OracleDbType)) continue;
                paramNames.Add(param.ParameterName);
            }
            if (paramNames.Count < 1) return;
            var pattern = $"({string.Join('|', paramNames.Select(c => $"(\\s{c}\\b)"))})";
            var padding = string.Empty.PadLeft(rnd.Value.Next(50));
            _oracleCommand.CommandText = Regex.Replace(_oracleCommand.CommandText, pattern, $@"{padding}$1");
        }
    }
}