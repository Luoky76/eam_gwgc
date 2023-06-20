namespace Gksyb.Model.Filter
{
    /// <summary>
    /// 过滤规则
    /// </summary>
    public class FilterRule
    {
        public FilterRule()
        {
        }

        public FilterRule(string field, object value)
            : this(field, value, "equal")
        {
        }

        public FilterRule(string field, object value, string op)
        {
            this.Field = field;
            this.Value = value;
            this.Op = op;
        }

        public string Field { get; set; }
        public object Value { get; set; }
        public string Op { get; set; }
        public string Type { get; set; }
        public string ParamName { get; set; }
    }
}