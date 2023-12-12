using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Threading.Tasks;
using Gksyb.Model.Core;

namespace Chloe.Infrastructure.Interception
{
    /// <summary>
    /// 数据库日志拦截器
    /// </summary>
    public class DbContextInterceptor : IDbContextInterceptor
    {
        /// <summary>
        /// 插入写入日志
        /// </summary>
        public void Insert(IDbContext dbContext, TypeDescriptor typeDescriptor, DbInsertExpression dbExpression)
        {
            if (dbContext.IsDisableDbLog()) return;
            if (typeDescriptor.Definition.NotDbLog()) return;
            var keys = new List<object>();
            var builder = new StringBuilder();
            var columns = dbExpression.InsertColumns;
            object value;
            foreach (PrimitivePropertyDescriptor propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
            {
                if (dbExpression.OutputColumns.ContainsKey(propertyDescriptor.Column))
                {
                    value = dbExpression.OutputColumns[propertyDescriptor.Column];
                }
                else
                {
                    var expression = columns.Where(c => c.Column == propertyDescriptor.Column).Select(c => c.Value).FirstOrDefault();
                    if (expression == null) continue;
                    value = dbContext.Evaluate(expression);
                }
                var key = propertyDescriptor.GetDescription();
                if (propertyDescriptor.IsPrimaryKey) keys.Add(value);
                builder.Append($"{key}:{value}{Environment.NewLine}");
            }
            var detail = builder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            dbContext.DBLog($"{typeDescriptor.Definition.GetDescription()} {Oper.Add}", keys.ToStr(","), detail).GetResult();
        }

        /// <summary>
        /// 更新写入日志
        /// </summary>
        public void Update(IDbContext dbContext, TypeDescriptor typeDescriptor, DbUpdateExpression dbExpression, IEntityState entityState)
        {
            if (dbContext.IsDisableDbLog()) return;
            if (typeDescriptor.Definition.NotDbLog()) return;
            var keys = new List<object>();
            var builder = new StringBuilder();
            var columns = dbExpression.UpdateColumns;
            var isTrack = entityState != null;
            foreach (PrimitivePropertyDescriptor propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
            {
                var expression = columns.Where(c => c.Column == propertyDescriptor.Column).Select(c => c.Value).FirstOrDefault();
                if (expression == null) continue;
                var key = propertyDescriptor.GetDescription();
                var value = dbContext.Evaluate(expression);
                object oldValue = null;
                if (isTrack && entityState.Fakes.TryGetValue(propertyDescriptor.Property, out oldValue))
                {
                    if (propertyDescriptor.IsPrimaryKey) keys.Add(oldValue ?? value);
                    if (oldValue != null) oldValue = $"{oldValue}→";
                }
                builder.Append($"{key}:{oldValue}{value}{Environment.NewLine}");
            }
            var primaryKey = keys.ToStr(",");
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                primaryKey = dbContext.Translate(dbExpression.Condition).ToSql(typeDescriptor.PrimaryKeys?.Select(c => c.Column.Name).ToStr(","));
            }
            var detail = builder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            dbContext.DBLog($"{typeDescriptor.Definition.GetDescription()} {Oper.Modify}", primaryKey, detail).GetResult();
        }

        /// <summary>
        /// 删除写入日志
        /// </summary>
        public void Delete(IDbContext dbContext, TypeDescriptor typeDescriptor, DbDeleteExpression dbExpression, object entity)
        {
            if (dbContext.IsDisableDbLog()) return;
            if (typeDescriptor.Definition.NotDbLog()) return;
            var keys = new List<object>();
            var builder = new StringBuilder();
            if (entity != null)
            {
                foreach (PrimitivePropertyDescriptor propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
                {
                    var key = propertyDescriptor.GetDescription();
                    var value = propertyDescriptor.GetValue(entity);
                    if (propertyDescriptor.IsPrimaryKey) keys.Add(value);
                    builder.Append($"{key}:{value}{Environment.NewLine}");
                }
            }
            var primaryKey = keys.ToStr(",");
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                primaryKey = dbContext.Translate(dbExpression.Condition).ToSql(typeDescriptor.PrimaryKeys?.Select(c => c.Column.Name).ToStr(","));
            }
            var detail = builder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            if (string.IsNullOrWhiteSpace(detail)) detail = dbContext.Translate(dbExpression.Condition).ToSql();
            dbContext.DBLog($"{typeDescriptor.Definition.GetDescription()} {Oper.Delete}", primaryKey, detail).GetResult();
        }
    }
}