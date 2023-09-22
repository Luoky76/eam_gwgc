using Chloe;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gksyb.Common.Data
{
    public static class DbColumnInfoExtension
    {
        /// <summary>
        /// 获取表、视图信息
        /// </summary>
        public static async Task<List<DbColumnInfo>> GetTableColumns(this IDbContext source, string table, string schema)
        {
            try
            {
                if (table.Trim().StartsWith("select ", StringComparison.OrdinalIgnoreCase))
                {
                    return await HandleColumns(source, null, table, schema);
                }
                var method = $"Get{source.GetDbType()}Column";
                MessageException.ThrowIf(!_methodInfos.ContainsKey(method), $"不支持{method}");
                var columns = await (_methodInfos[method].Invoke(null, new object[] { source, table, schema }) as Task<List<DbColumnInfo>>);
                return await HandleColumns(source, columns, table, schema);
            }
            catch (Exception)
            {
                return await HandleColumns(source, null, table, schema);
            }
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> HandleColumns(IDbContext source, List<DbColumnInfo> columns, string table, string schema)
        {
            var tableName = table;
            if (!string.IsNullOrWhiteSpace(schema)) tableName = $"{schema}.{table}";
            if (tableName.Trim().StartsWith("select ", StringComparison.OrdinalIgnoreCase)) tableName = $"({tableName})";
            var sql = $"SELECT * FROM {tableName} tmptablequery WHERE 1 = 2";
            columns ??= new List<DbColumnInfo>();
            using var reader = await source.Session.ExecuteReaderAsync(sql);
            var list = reader.GetSchemaTable();
            foreach (DataRow row in list.Rows)
            {
                var name = row[SchemaTableColumn.ColumnName] as string;
                var column = columns.FirstOrDefault(c => c.Name == name);
                if (column == null)
                {
                    column = new DbColumnInfo()
                    {
                        Table = row[SchemaTableColumn.BaseTableName] as string ?? table,
                        Name = name,
                        DbType = row["DataTypeName"] as string,
                        MaxLength = row[SchemaTableColumn.ColumnSize].CastTo<int?>(),
                        Precision = row[SchemaTableColumn.NumericPrecision].CastTo<int?>(),
                        Scale = row[SchemaTableColumn.NumericScale].CastTo<int?>(),
                        IsPrimary = row[SchemaTableColumn.IsKey].CastTo<bool?>(),
                        IsIdentity = row["IsIdentity"].CastTo<bool?>(),
                        IsNullable = row[SchemaTableColumn.AllowDBNull].CastTo<bool?>(),
                        Position = row[SchemaTableColumn.ColumnOrdinal].CastTo<int?>()
                    };
                    columns.Add(column);
                }
                var fieldType = (row[SchemaTableColumn.DataType] as Type).Name.ToLower();
                if (column.DbType.Contains("date", StringComparison.OrdinalIgnoreCase) || column.DbType.Contains("time", StringComparison.OrdinalIgnoreCase))
                {
                    fieldType = "DateTime";
                }
                if (fieldType.Contains("byte")) fieldType = "string";
                if (fieldType == "string" && column.DbType.StartsWith("int", StringComparison.OrdinalIgnoreCase)) fieldType = "int";
                if (fieldType == "string" && column.DbType.StartsWith("real", StringComparison.OrdinalIgnoreCase)) fieldType = "decimal";
                if (fieldType == "int64") fieldType = "long";
                if (fieldType == "int32") fieldType = "int";
                column.CsType = fieldType;
            }
            columns.ForEach(c =>
            {
                c.CsType = string.IsNullOrWhiteSpace(c.CsType) ? "string" : c.CsType;
                c.MaxLength = c.MaxLength > 0 ? c.MaxLength : 0;
                c.Precision = c.Precision > 0 ? c.Precision : 0;
                c.Scale = c.Scale > 0 ? c.Scale : 0;
                c.IsPrimary = c.IsPrimary == true;
                c.IsIdentity = c.IsIdentity == true;
                c.IsNullable = c.IsNullable == true;
                c.Position ??= int.MaxValue;
                if (c.Scale == 0 && (c.CsType == "decimal" || c.CsType == "float"))
                {
                    c.CsType = c.Precision > 9 ? "long" : "int";
                }
            });
            return columns;
        }

        /// <summary>
        /// 获取oracle列信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetOracleColumn(IDbContext source, string table, string schema)
        {
            var defaultSchema = await source.Session.ExecuteScalarAsync("SELECT username FROM user_users") as string;
            if (string.IsNullOrWhiteSpace(schema)) schema = defaultSchema;
            var paramPrefix = source.GetParamPrefix();
            var sql = $@"SELECT a.owner ""Schema"",
                           a.table_name ""Table"",
                           a.column_name ""Name"",
                           a.data_type DbType,
                           (CASE
                             WHEN a.CHAR_LENGTH = 0 THEN
                              a.DATA_LENGTH
                             ELSE
                              a.CHAR_LENGTH
                           END) MaxLength,
                           a.data_precision ""Precision"",
                           a.data_scale Scale,
                           CASE
                             WHEN a.nullable = 'N' THEN
                              0
                             ELSE
                              1
                           END IsNullable,
                           to_char(b.comments) ""Comment"",
                           a.DATA_DEFAULT DefaultValue,
                           a.COLUMN_ID Position
                      FROM all_tab_cols a
                      LEFT JOIN all_col_comments b
                        ON b.owner = a.owner
                       AND b.table_name = a.table_name
                       AND b.column_name = a.column_name
                     WHERE a.TABLE_NAME = {paramPrefix}tableName
                       AND a.owner = {paramPrefix}owner
                       AND a.HIDDEN_COLUMN = 'NO'
                     ORDER BY a.COLUMN_ID ASC";
            var columns = await source.SqlQueryAsync<DbColumnInfo>(sql, new DbParam("tableName", table), new DbParam("owner", schema));
            sql = $@"SELECT c.COLUMN_NAME
                      FROM ALL_CONS_COLUMNS c
                     INNER JOIN all_constraints t
                        ON t.OWNER = c.owner
                       AND t.table_name = c.table_name
                       AND t.CONSTRAINT_NAME = c.constraint_name
                       AND t.CONSTRAINT_TYPE = 'P'
                     WHERE c.table_name = {paramPrefix}tableName
                       AND c.owner = {paramPrefix}owner";
            var names = await source.SqlQueryAsync<string>(sql, new DbParam("tableName", table), new DbParam("owner", schema));
            foreach (var column in columns)
            {
                if (names.Any(c => c == column.Name)) column.IsPrimary = true;
                if (column.Schema == defaultSchema) column.Schema = "";
                if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                {
                    column.DefaultValue = column.DefaultValue.TrimEnd('\n');
                    if (column.DefaultValue.StartsWith("null", StringComparison.OrdinalIgnoreCase)) column.DefaultValue = null;
                }
            }
            return columns;
        }

        /// <summary>
        /// 获取mysql列信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetMysqlColumn(IDbContext source, string table, string schema)
        {
            var defaultSchema = await source.Session.ExecuteScalarAsync("SELECT DATABASE()") as string;
            if (string.IsNullOrWhiteSpace(schema)) schema = defaultSchema;
            var paramPrefix = source.GetParamPrefix();
            var sql = $@"SELECT a.TABLE_SCHEMA ""Schema"",
                       a.TABLE_NAME ""Table"",
                       a.COLUMN_NAME ""Name"",
                       a.DATA_TYPE DbType,
                       A.CHARACTER_MAXIMUM_LENGTH MaxLength,
                       a.NUMERIC_PRECISION ""Precision"",
                       a.NUMERIC_SCALE Scale,
                       (CASE WHEN INSTR(a.COLUMN_KEY, 'PRI') > 0 THEN 1 ELSE 0 END) IsPrimary,
                       (CASE WHEN INSTR(a.EXTRA, 'auto_increment') > 0 THEN 1 ELSE 0 END) IsIdentity,
                       (CASE WHEN a.IS_NULLABLE = 'YES' THEN 1 ELSE 0 END) IsNullable,
                       A.COLUMN_COMMENT ""Comment"",
                       a.COLUMN_DEFAULT DefaultValue,
                       a.ORDINAL_POSITION Position
                  FROM information_schema.COLUMNS a
                  WHERE a.TABLE_NAME = {paramPrefix}tableName
                       AND a.TABLE_SCHEMA = {paramPrefix}owner
                     ORDER BY a.ORDINAL_POSITION ASC";
            var columns = await source.SqlQueryAsync<DbColumnInfo>(sql, new DbParam("tableName", table), new DbParam("owner", schema));
            foreach (var column in columns)
            {
                if (column.Schema == defaultSchema) column.Schema = "";
            }
            return columns;
        }

        /// <summary>
        /// 获取sqlite列信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetSqliteColumn(IDbContext source, string table, string schema)
        {
            var paramPrefix = source.GetParamPrefix();
            var sql = $@"SELECT sql FROM sqlite_master WHERE name = {paramPrefix}tableName";
            var sqls = (await source.Session.ExecuteScalarAsync(sql, new DbParam("tableName", table)) as string).Split('\n').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            sql = $@"PRAGMA table_info({table})";
            var list = await source.SqlQueryAsync<dynamic>(sql);
            var columns = new List<DbColumnInfo>();
            foreach (var item in list)
            {
                long? position = item.cid;
                var column = new DbColumnInfo()
                {
                    Schema = schema,
                    Table = table,
                    Name = item.name,
                    DbType = item.type,
                    IsPrimary = item.pk == 1,
                    IsNullable = item.pk == 0 && item.notnull == 0,
                    DefaultValue = item.dflt_value,
                    Position = position.CastTo<int?>()
                };
                column.IsIdentity = sqls.Any(c => c.Contains("AUTOINCREMENT") && c.Contains($"\"{column.Name}\""));
                if (!column.DbType.Contains('(')) column.DbType = $"{column.DbType}(0)";
                var match = Regex.Match(column.DbType, @"(\w+)\s*\(\s*(\d*)\s*\,*\s*(\d*)\s*\)");
                column.DbType = match.Groups[1].Value;
                column.MaxLength = match.Groups.Count > 2 && !string.IsNullOrWhiteSpace(match.Groups[2].Value) ? match.Groups[2].Value.CastTo(0) : 0;
                column.Precision = match.Groups.Count > 3 && !string.IsNullOrWhiteSpace(match.Groups[2].Value) ? match.Groups[2].Value.CastTo(0) : 0;
                column.Scale = match.Groups.Count > 3 && !string.IsNullOrWhiteSpace(match.Groups[3].Value) ? match.Groups[3].Value.CastTo(0) : 0;
                columns.Add(column);
            }
            return columns;
        }

        /// <summary>
        /// 获取pgsql列信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetPgsqlColumn(IDbContext source, string table, string schema)
        {
            var defaultSchema = "public";
            if (string.IsNullOrWhiteSpace(schema)) schema = defaultSchema;
            var paramPrefix = source.GetParamPrefix();
            var sql = $@"SELECT ns.nspname ""Schema"",
                           a.table_name ""Table"",
                           a.attname ""Name"",
                           t.typname DbType,
                           (CASE
                             WHEN a.atttypmod > 0 AND a.atttypmod < 32767 THEN
                              a.atttypmod - 4
                             ELSE
                              a.attlen
                           END) MaxLength,
                           (CASE
                             WHEN t.typelem = 0 THEN
                              t.typname
                             ELSE
                              t2.typname
                           END, CASE
                              WHEN a.attnotnull THEN
                               0
                              ELSE
                               1
                            END) IsNullable,
                           (SELECT pg_get_expr(adbin, adrelid)
                              FROM pg_attrdef
                             WHERE adrelid = e.adrelid
                               AND adnum = e.adnum LIMIT 1) IsIdentity,
                           d.description ""Comment""
                      FROM pg_class c
                     INNER JOIN pg_attribute a
                        ON a.attnum > 0
                       AND a.attrelid = c.oid
                     INNER JOIN pg_type t
                        ON t.oid = a.atttypid
                      LEFT JOIN pg_type t2
                        ON t2.oid = t.typelem
                      LEFT JOIN pg_description d
                        ON d.objoid = a.attrelid
                       AND d.objsubid = a.attnum
                      LEFT JOIN pg_attrdef e
                        ON e.adrelid = a.attrelid
                       AND e.adnum = a.attnum
                     INNER JOIN pg_namespace ns
                        ON ns.oid = c.relnamespace
                     INNER JOIN pg_namespace ns2
                        ON ns2.oid = t.typnamespace
                     WHERE a.table_name = {paramPrefix}tableName
                       AND ns.nspname = {paramPrefix}owner";
            var columns = await source.SqlQueryAsync<DbColumnInfo>(sql, new DbParam("tableName", table), new DbParam("owner", schema));
            foreach (var column in columns)
            {
                if (column.Schema == defaultSchema) column.Schema = "";
            }
            return columns;
        }

        /// <summary>
        /// 获取sqlserver表信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetSqlServerColumn(IDbContext source, string table, string schema)
        {
            var paramPrefix = source.GetParamPrefix();
            var sql = $@"SELECT
	                        '' ""Schema"",
	                        b.name ""Table"",
	                        a.name ""Name"",
	                        c.name ""DbType"",
	                        (CASE WHEN c.name IN ( 'text', 'ntext', 'image' ) THEN 0 WHEN c.name IN ( 'nchar', 'nvarchar' ) THEN a.max_length / 2 ELSE a.max_length END ) ""MaxLength"",
		                    a.precision ""Precision"",
		                    a.scale ""Scale"",
		                    e.is_primary_key IsPrimary,
		                    a.is_identity IsIdentity,
		                    a.is_nullable IsNullable,
		                    f.value ""Comment"",
		                    g.text ""DefaultValue"",
		                    a.column_id ""Position""
	                    FROM sys.columns a
		                    INNER JOIN sys.objects b ON b.object_id = a.object_id
		                    AND b.type IN ( 'U', 'V' )
		                    INNER JOIN sys.types c ON c.user_type_id = a.user_type_id
		                    LEFT JOIN sys.index_columns d ON d.object_id = a.object_id
		                    AND d.column_id = a.column_id
		                    LEFT JOIN sys.indexes e ON e.object_id = d.object_id
		                    AND e.index_id = d.index_id
		                    LEFT JOIN sys.extended_properties f ON f.major_id = a.object_id
		                    AND f.minor_id = a.column_id
		                    LEFT JOIN syscomments g ON g.id = a.default_object_id
	                    WHERE b.name = {paramPrefix}tableName ORDER BY a.column_id ASC";
            var columns = await source.SqlQueryAsync<DbColumnInfo>(sql, new DbParam("tableName", table), new DbParam("owner", schema));
            foreach (var column in columns)
            {
                if (column.DefaultValue == null) continue;
                var value = column.DefaultValue.ToString();
                column.DefaultValue = Regex.Replace(value.SubStr(1, value.Length - 2), @"\(+(?<f1>[^(\(|\))]+)\)+", "${f1}");
            }
            return columns;
        }

        /// <summary>
        /// 获取达梦表信息
        /// </summary>
        private static async Task<List<DbColumnInfo>> GetDamengColumn(IDbContext source, string table, string schema)
        {
            return await GetOracleColumn(source, table, schema);
        }

        private static readonly Dictionary<string, MethodInfo> _methodInfos = null;

        /// <summary>
        /// 初始化
        /// </summary>
        static DbColumnInfoExtension()
        {
            _methodInfos = typeof(DbColumnInfoExtension).GetDicMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
        }
    }
}