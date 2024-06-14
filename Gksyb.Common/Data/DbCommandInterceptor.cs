using Chloe;
using Chloe.Infrastructure.Interception;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Serilog.Extensions.Logging;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Gksyb.Common.Data
{
    /// <summary>
    /// 数据库日志拦截器
    /// </summary>
    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly LogPath _logPath = new("SQL");
        private readonly LogPath _SlowQuerylogPath = new("SlowQuery");
        private ILogger _logger;
        private readonly bool _isLog = false;
        private readonly int _slowQuery = 10 * 1000;
        private const string _contextName = nameof(IDbContext);
        private const string _timeBag = "startTime";

        private ILogger Logger
        {
            get
            {
                if (_logger != null) return _logger;
                _logger = new SerilogLoggerProvider(Serilog.Log.Logger, false).CreateLogger("");
                return _logger;
            }
        }

        public DbCommandInterceptor(bool isLog, int slowQuery)
        {
            _isLog = isLog;
            if (slowQuery > 0)
            {
                _slowQuery = slowQuery * 1000;
            }
        }

        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            command.SqlTranslate();
            interceptionContext.DataBag[_timeBag] = DateTime.Now;
        }

        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            var elapsed = DateTime.Now.Subtract((DateTime)(interceptionContext.DataBag[_timeBag])).TotalMilliseconds;
            if (elapsed > _slowQuery)//慢查询
            {
                Logger.LogWarning(_SlowQuerylogPath, $"耗时:{{Elapsed}}毫秒 {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            if (!_isLog) return;
            var context = interceptionContext.DataBag[_contextName];
            if (context == null || context is not IDbContext dbContext) return;
            if (dbContext.IsDisableSqlLog()) return;
            if (interceptionContext.Exception == null)
            {
                var fieldCount = interceptionContext.Result.FieldCount;
                Logger.LogInformation(_logPath, $"耗时:{{Elapsed}}毫秒,{fieldCount}列 {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            else
            {
                ExceptionHandle(command, interceptionContext.Exception);
            }
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            command.SqlTranslate();
            interceptionContext.DataBag[_timeBag] = DateTime.Now;
        }

        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            var elapsed = DateTime.Now.Subtract((DateTime)(interceptionContext.DataBag[_timeBag])).TotalMilliseconds;
            if (elapsed > _slowQuery)//慢查询
            {
                Logger.LogWarning(_SlowQuerylogPath, $"耗时:{{Elapsed}}毫秒 {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            if (!_isLog) return;
            var context = interceptionContext.DataBag[_contextName];
            if (context == null || context is not IDbContext dbContext) return;
            if (dbContext.IsDisableSqlLog()) return;
            if (interceptionContext.Exception == null)
            {
                var result = interceptionContext.Result;
                Logger.LogInformation(_logPath, $"耗时:{{Elapsed}}毫秒,影响行数{result} {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            else
            {
                ExceptionHandle(command, interceptionContext.Exception);
            }
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            command.SqlTranslate();
            interceptionContext.DataBag[_timeBag] = DateTime.Now;
        }

        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            var elapsed = DateTime.Now.Subtract((DateTime)(interceptionContext.DataBag[_timeBag])).TotalMilliseconds;
            if (elapsed > _slowQuery)//慢查询
            {
                Logger.LogWarning(_SlowQuerylogPath, $"耗时:{{Elapsed}}毫秒 {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            if (!_isLog) return;
            var context = interceptionContext.DataBag[_contextName];
            if (context == null || context is not IDbContext dbContext) return;
            if (dbContext.IsDisableSqlLog()) return;
            if (interceptionContext.Exception == null)
            {
                var result = interceptionContext.Result;
                Logger.LogInformation(_logPath, $"耗时:{{Elapsed}}毫秒,值{result} {Environment.NewLine}{AppendDbCommandInfo(command)}", elapsed);
            }
            else
            {
                ExceptionHandle(command, interceptionContext.Exception);
            }
        }

        private void ExceptionHandle(IDbCommand command, Exception exception)
        {
            Logger.LogError(_logPath, $"数据库异常{exception} {Environment.NewLine}{AppendDbCommandInfo(command)}");
            if (command.Connection is OracleConnection connection)
            {
                OracleConnection.ClearPool(connection);//数据库异常，从连接池清除连接
            }
        }

        public static string AppendDbCommandInfo(IDbCommand command)
        {
            var sb = new StringBuilder();
            sb.AppendLine(command.CommandText);
            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null) continue;
                object value;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;
                    if (param.DbType == DbType.String || param.DbType == DbType.AnsiString || param.DbType == DbType.DateTime)
                    {
                        var v = ("'" + value + "'");
                        value = Regex.Replace(v, @"(?<f1>(\{|\}))", "${f1}${f1}", RegexOptions.IgnoreCase);
                    }
                }
                var dbTypeName = Enum.GetName(typeof(DbType), param.DbType);
                var direction = Enum.GetName(typeof(ParameterDirection), param.Direction);
                sb.Append($"{direction} {dbTypeName} {param.ParameterName} = {value};");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}