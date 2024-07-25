using Chloe.Descriptors;
using Gksyb.Model.Filter;
using System.Collections;
using System.Text.RegularExpressions;

namespace Gksyb.Core.Filter
{
    /// <summary>
    /// 将检索规则 翻译成 where sql 语句,并生成相应的参数列表
    /// 如果遇到{CurrentUserID}这种，翻译成对应的参数
    /// </summary>
    public class FilterTranslatorLinq : FilterTranslator
    {
        private readonly TypeDescriptor _typeDescriptor;
        private readonly Regex _start;

        public FilterTranslatorLinq(TypeDescriptor typeDescriptor)
            : this(typeDescriptor, null)
        {
        }

        public FilterTranslatorLinq(TypeDescriptor typeDescriptor, FilterGroup group) : base(group)
        {
            _typeDescriptor = typeDescriptor;
            _start = new Regex($@"^{_typeDescriptor.EntityType?.Name ?? _typeDescriptor.Table?.Name}\.", RegexOptions.IgnoreCase);
            leftToken = "";
            rightToken = "";
            paramPrefix = "";
        }

        public override string TranslateRule(FilterRule rule)
        {
            if (rule == null) return " 1=1 ";
            if ("NULLPARAM".Equals(rule.Op))
            {
                CreateFilterParam(null, rule.Type, rule);
                return " 1=1 ";
            }
            var filter = TranslateRuleInner(rule);
            var op = rule.Op.ToLower();
            if (op.StartsWith("not") && op != "notequal")
            {
                return $"(!({filter}))";
            }
            return filter;
        }

        private string TranslateRuleInner(FilterRule rule)
        {
            var bulider = new StringBuilder();
            var op = rule.Op.ToLower();
            if (!string.IsNullOrWhiteSpace(rule.Field)) rule.Field = _start.Replace(rule.Field, "");
            //如果字段名采用了 用户信息参数
            if (FilterParmMatch.CurrentParmMatch.ContainsKey(rule.Field))
            {
                var field = FilterParmMatch.CurrentParmMatch[rule.Field]();
                bulider.Append(paramPrefixToken + CreateFilterParam(field, field.GetType().Name, rule));
            }
            else
            {
                bulider.Append(leftToken + rule.Field + rightToken);
            }

            var operatorQuery = GetOperatorQueryText(op);
            bulider.Append(operatorQuery);

            if (rule.Type.EqualsTo("field")) return bulider.Append(rule.Value).ToString();
            if ("{CurrentSysdate}".Equals(rule.Value))
            {
                return bulider.Append(" {Sysdate} ").ToString();
            }

            if (op == "in" || op == "notin")
            {
                var split = string.IsNullOrWhiteSpace(rule.Split) ? "," : rule.Split;
                var values = rule.Value.ToString().Split(split);
                var appended = false;
                bulider.Append('(');
                foreach (var value in values)
                {
                    if (appended) bulider.Append(',');
                    appended = false;
                    //如果值使用了 用户信息参数 比如： in ({CurrentRoleID},4)
                    if (FilterParmMatch.CurrentParmMatch.ContainsKey(value))
                    {
                        var val = FilterParmMatch.CurrentParmMatch[value]();
                        if (val is IEnumerable)
                        {
                            foreach (var tempValue in (val as IEnumerable))
                            {
                                if (appended) bulider.Append(',');
                                bulider.Append(paramPrefixToken + CreateFilterParam(tempValue, tempValue.GetType().Name, rule));
                                appended = true;
                            }
                        }
                        else
                        {
                            bulider.Append(paramPrefixToken + CreateFilterParam(val, val.GetType().Name, rule));
                        }
                    }
                    else
                    {
                        bulider.Append(paramPrefixToken + CreateFilterParam(value, rule.Type, rule));
                    }
                    appended = true;
                }
                bulider.Append(')');
                return $"({bulider})";
            }
            //is null 和 is not null 不需要值
            else if (op != "isnull" && op != "isnotnull")
            {
                var isFunction = operatorQuery.StartsWith(".");
                var key = rule.Value.CastTo<string>() ?? "";
                if (FilterParmMatch.CurrentParmMatch.ContainsKey(key))
                {
                    rule.Value = FilterParmMatch.CurrentParmMatch[key]();
                    rule.Type = rule.GetType().Name;
                }
                var value = rule.Value.CastTo<string>() ?? "";
                if (value.StartsWith(likeToken.ToString()))
                {
                    value = value.Remove(0, 1);
                }
                if (value.EndsWith(likeToken.ToString()))
                {
                    value = value.Remove(value.Length - 1, 1);
                }
                rule.Value = value;
                if (isFunction) bulider.Append('(');
                bulider.Append(paramPrefixToken + CreateFilterParam(rule.Value, rule.Type, rule));
                if (isFunction) bulider.Append(')');
            }
            return bulider.ToString();
        }

        private string CreateFilterParam(object value, string type, FilterRule rule)
        {
            var property = _typeDescriptor.PrimitivePropertyDescriptors.FirstOrDefault(c => c.Property.Name.EqualsTo(rule.Field, true));
            if (property != null && (string.IsNullOrWhiteSpace(type) || property.PropertyType.GetUnNullableType().IsNumeric()))
            {
                type = property.PropertyType.GetUnNullableType().Name;
            }
            return CreateFilterParam(value, type, string.Empty);
        }

        /// <summary>
        /// 获取操作符的SQL Text
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public override string GetOperatorQueryText(string op)
        {
            return op switch
            {
                "equal" => " = ",
                "greater" => " > ",
                "greaterorequal" => " >= ",
                "isnull" => " == null ",
                "isnotnull" => " != null ",
                "less" => " < ",
                "lessorequal" => " <= ",
                "like" => ".Contains",
                "notlike" => ".Contains",
                "startwith" => ".StartsWith",
                "endwith" => ".EndsWith",
                "notstartwith" => ".StartsWith",
                "notendwith" => ".EndsWith",
                "notequal" => " <> ",
                "and" => " and ",
                "or" => " or ",
                "in" => " in ",
                "notin" => " in ",
                _ => " = ",
            };
        }
    }
}