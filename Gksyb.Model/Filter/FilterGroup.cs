using Gksyb.Common;
using System.Collections.Generic;

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
    }
}