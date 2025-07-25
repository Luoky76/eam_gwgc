namespace Gksyb.Model.Grid
{
    public class OrderRequest
    {

        public OrderRequest()
        {
        }

        public OrderRequest(string field, string orderType)
        {
            Field = field;
            OrderType = orderType;
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 排序方式 asc desc
        /// </summary>
        public string OrderType { get; set; }

        public string GetOrderBy()
        {
            return $"{Field} {("asc".Equals(Field, StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC")}";
        }
    }
}
