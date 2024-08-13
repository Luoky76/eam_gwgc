using Chloe.SQLite;
using Dm;
using Kdbndp;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;

namespace Gksyb.Common.Data
{
    /// <summary>
    /// 数据库差异化处理
    /// </summary>
    public static class SqlTranslateExtension
    {
        public static void SqlTranslate(this IDbCommand source)
        {
            var sql = source.CommandText;
            switch (source.Connection)
            {
                case OracleConnection _:
                    sql = OracleTranslate(sql);
                    break;

                case MySqlConnection _:
                    sql = MySqlTranslate(sql);
                    break;

                case SqlConnection _:
                    sql = SqlServerTranslate(sql);
                    break;

                case ChloeSQLiteConcurrentConnection _:
                case SqliteConnection _:
                    sql = SqliteTranslate(sql);
                    break;

                case NpgsqlConnection _:
                    sql = NpgsqlTranslate(sql);
                    break;

                case DmConnection _:
                    sql = DamengTranslate(sql);
                    break;

                case KdbndpConnection _:
                    sql = KdbndpTranslate(sql);
                    break;
            }
            source.CommandText = sql;
        }

        /// <summary>
        /// oracle差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string OracleTranslate(string sql)
        {
            sql = Regex.Replace(sql, @"(?<!@)@(\w+)", ":$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "SYSDATE", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(group_concat)\b", "wm_concat", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(len)\b", "length", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(substring)\b", "substr", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(left)\b\s*\((?<f1>[^(\(|,|\))]*),(?<f2>[^(\(|\))]*)\)", "substr(${f1},1,${f2})", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(right)\b\s*\((?<f1>[^(\(|,|\))]*),(?<f2>[^(\(|\))]*)\)", "substr(${f1},-${f2})", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(isnull)\b", "nvl", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(getdate)\b\s*\(\s*\)", "sysdate", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(datepart)\b\s*\(([^(\(|,|\))]*),([^(\(|\))]*)\)", match =>
            {
                var type = match.Groups[2].Value.Trim();
                switch (type)
                {
                    case "year":
                        type = "'yyyy'";
                        break;

                    case "month":
                        type = "'mm'";
                        break;

                    case "day":
                        type = "'dd'";
                        break;
                }
                return string.Format("to_char({0},{1})", match.Groups[3].Value, type);
            }, RegexOptions.IgnoreCase);
            return sql;
        }

        /// <summary>
        /// mysql差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string MySqlTranslate(string sql)
        {
            sql = Regex.Replace(sql, @"(?<!@)@(\w+)", "?$1");
            sql = Regex.Replace(sql, @":(\w+)", "?$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "now()", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(from)\b\s*\b(dual)\b[\s\S]*", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\s*\(", "(", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(varchar)\b", "char", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(wm_concat)\b", "group_concat", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(len)\b", "length", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(nvl)\b", "IFNULL", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(isnull)\b\s*\((?<f1>[^(\(|,)]*),", "IFNULL(${f1},", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(getdate)\b", "now", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(sysdate)\b", "now()", RegexOptions.IgnoreCase);
            //sql = Regex.Replace(sql, @"\b(int)\b", "signed", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(datepart)\b\s*\((?<f1>[^(\(|,|\))]*),(?<f2>[^(\(|\))]*)\)", "${f1}(${f2})", RegexOptions.IgnoreCase);
            //|| 规则处理 目前最多只支持三个
            var reg = new Regex(@"(?<f1>(\w|')+)\s*(\|\|)\s*(?<f2>(\w|')+)\s*(\|\|)\s*(?<f3>(\w|')+)", RegexOptions.IgnoreCase);
            while (reg.IsMatch(sql))
            {
                sql = reg.Replace(sql, "concat(${f1},${f2},${f3})");
            }
            var reg2 = new Regex(@"(?<f1>(\w|')+)\s*(\|\|)\s*(?<f2>(\w|')+)", RegexOptions.IgnoreCase);
            while (reg2.IsMatch(sql))
            {
                sql = reg2.Replace(sql, "concat(${f1},${f2})");
            }
            return sql;
        }

        /// <summary>
        /// mysql差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string SqlServerTranslate(string sql)
        {
            sql = Regex.Replace(sql, @":(\w+)", "@$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "getdate()", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(from)\b\s*\b(dual)\b", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(sysdate)\b", "getdate()", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(nvl)\b", "isnull", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(substr)\b", "substring", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"(?<f1>(\w|')+)\s*(\|\|)\s*(?<f2>(\w|')+)", "${f1} + ${f2}", RegexOptions.IgnoreCase);
            return sql;
        }

        /// <summary>
        /// sqlite差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string SqliteTranslate(string sql)
        {
            sql = Regex.Replace(sql, @":(\w+)", "@$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "DATETIME('now','localtime')", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(from)\b\s*\b(dual)\b", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(nvl)\b", "IFNULL", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(isnull)\b\s*\((?<f1>[^(\(|,)]*),", "IFNULL(${f1},", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(wm_concat)\b", "group_concat", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(len)\b", "LENGTH", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(substring)\b", "substr", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(left)\b\s*\((?<f1>[^(\(|,|\))]*),(?<f2>[^(\(|\))]*)\)", "SUBSTR(${f1},1,${f2})", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(right)\b\s*\((?<f1>[^(\(|,|\))]*),(?<f2>[^(\(|\))]*)\)", "SUBSTR(${f1},-${f2})", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(getdate)\b\s*\(\s*\)", "DATETIME('now','localtime')", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(sysdate)\b", "DATETIME('now','localtime')", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(datepart)\b\s*\(([^(\(|,|\))]*),([^(\(|\))]*)\)", match =>
            {
                var type = match.Groups[2].Value.Trim();
                switch (type)
                {
                    case "year":
                        type = "%Y";
                        break;

                    case "month":
                        type = "%m";
                        break;

                    case "day":
                        type = "%d";
                        break;
                }
                return string.Format("CAST(STRFTIME('{1}',{0}) AS INT)", match.Groups[3].Value, type);
            }, RegexOptions.IgnoreCase);
            var reg = new Regex(@"\b(concat)\b\s*\((?<f1>[^(,)]*),(?<f2>[^(\(|\))]*)\)", RegexOptions.IgnoreCase);
            while (reg.IsMatch(sql))
            {
                sql = reg.Replace(sql, "${f1} || ${f2}");
            }
            return sql;
        }

        /// <summary>
        /// Npgsql差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string NpgsqlTranslate(string sql)
        {
            sql = Regex.Replace(sql, @":(\w+)", "@$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "now()", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(sysdate)\b", "now()", RegexOptions.IgnoreCase);
            return sql;
        }

        /// <summary>
        /// 达梦差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string DamengTranslate(string sql)
        {
            sql = Regex.Replace(sql, @"(?<!@)@(\w+)", ":$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "now()", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(group_concat)\b", "wm_concat", RegexOptions.IgnoreCase);
            return sql;
        }

        /// <summary>
        /// 人大金仓差异化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string KdbndpTranslate(string sql)
        {
            sql = Regex.Replace(sql, @":(\w+)", "@$1");
            sql = Regex.Replace(sql, @"{Sysdate}", "systimestamp", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\b(sysdate)\b", "systimestamp", RegexOptions.IgnoreCase);
            return sql;
        }
    }
}