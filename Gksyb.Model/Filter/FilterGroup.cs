using Gksyb.Common;

namespace Gksyb.Model.Filter
{
    /// <summary>
    /// 过滤组
    /// </summary>
    public class FilterGroup
    {
        public IList<FilterRule> Rules { get; set; }
        public string Op { get; set; }
        public IList<FilterGroup> Groups { get; set; }

        public void Check()
        {
            if (Rules != null && Rules.Count > 0)
            {
                foreach (var rule in Rules)
                {
                    rule.Field.SqlFilter(30);
                }
            }
            if (Groups != null && Groups.Count > 0)
            {
                foreach (var group in Groups)
                {
                    group.Check();
                }
            }
        }

        /// <summary>
        /// 根据名称获取规则
        /// </summary>
        public FilterRule GetRule(string name)
        {
            var rule = Rules?.FirstOrDefault(c => c.Field == name);
            if (rule != null) return rule;
            if (Groups == null) return null;
            foreach (var group in Groups)
            {
                rule = group.GetRule(name);
                if (rule != null) return rule;
            }
            return rule;
        }
    }
}