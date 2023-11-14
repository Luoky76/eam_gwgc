using Gksyb.Model.Filter;
using System.Collections;

namespace Gksyb.Core.Filter
{
    /// <summary>
    /// 将检索规则 翻译成 where sql 语句,并生成相应的参数列表
    /// 如果遇到{CurrentUserID}这种，翻译成对应的参数
    /// </summary>
    public class FilterTranslator
    {
        //几个前缀/后缀
        protected string leftToken = " ";

        protected string paramPrefixToken = "@";
        protected string rightToken = " ";
        protected string groupLeftToken = "(";
        protected string groupRightToken = ")";
        protected string likeToken = "%";
        protected string paramPrefix = "p";

        /// <summary>
        /// 参数计数器
        /// </summary>
        private int paramCounter = 0;

        //几个主要的属性
        public FilterGroup Group { get; set; }

        public string CommandText { get; protected set; }
        public IList<DbParam> Parms { get; protected set; }

        public FilterTranslator()
            : this(null)
        {
        }

        public FilterTranslator(FilterGroup group)
        {
            Group = group;
            Parms = new List<DbParam>();
        }

        public void Translate()
        {
            CommandText = TranslateGroup(Group);
            if (string.IsNullOrEmpty(CommandText))
            {
                CommandText = " 1=1 ";
            }
        }

        public string TranslateGroup(FilterGroup group)
        {
            var bulider = new StringBuilder();
            if (group == null) return " 1=1 ";
            var appended = false;
            bulider.Append(groupLeftToken);
            if (group.Rules != null)
            {
                foreach (var rule in group.Rules)
                {
                    if (appended)
                        bulider.Append(GetOperatorQueryText(group.Op));
                    bulider.Append(TranslateRule(rule));
                    appended = true;
                }
            }
            if (group.Groups != null)
            {
                foreach (var subgroup in group.Groups)
                {
                    if (appended)
                        bulider.Append(GetOperatorQueryText(group.Op));
                    bulider.Append(TranslateGroup(subgroup));
                    appended = true;
                }
            }
            bulider.Append(groupRightToken);
            if (appended == false) return " 1=1 ";
            return bulider.ToString();
        }

        public virtual string TranslateRule(FilterRule rule)
        {
            var bulider = new StringBuilder();
            if (rule == null) return " 1=1 ";
            if ("NULLPARAM".Equals(rule.Op))
            {
                CreateFilterParam(null, rule.Type, rule.ParamName);
                return " 1=1 ";
            }
            var op = rule.Op.ToLower();
            //如果字段名采用了 用户信息参数
            if (FilterParmMatch.CurrentParmMatch.ContainsKey(rule.Field))
            {
                var field = FilterParmMatch.CurrentParmMatch[rule.Field]();
                bulider.Append(paramPrefixToken + CreateFilterParam(field, field.GetType().Name, rule.ParamName));
            }
            else
            {
                bulider.Append(leftToken + rule.Field + rightToken);
            }
            var operatorQuery = GetOperatorQueryText(op);
            //操作符
            bulider.Append(operatorQuery);

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
                                bulider.Append(paramPrefixToken + CreateFilterParam(tempValue, tempValue.GetType().Name, rule.ParamName));
                                appended = true;
                            }
                        }
                        else
                        {
                            bulider.Append(paramPrefixToken + CreateFilterParam(val, val.GetType().Name, rule.ParamName));
                        }
                    }
                    else
                    {
                        bulider.Append(paramPrefixToken + CreateFilterParam(value, rule.Type, rule.ParamName));
                    }
                    appended = true;
                }
                bulider.Append(')');
            }
            //is null 和 is not null 不需要值
            else if (op != "isnull" && op != "isnotnull")
            {
                var key = rule.Value.CastTo<string>() ?? "";
                if (FilterParmMatch.CurrentParmMatch.ContainsKey(key))
                {
                    rule.Value = FilterParmMatch.CurrentParmMatch[key]();
                    rule.Type = rule.GetType().Name;
                }
                if (op == "like" || op == "notlike" || op == "endwith" || op == "notendwith")
                {
                    var value = rule.Value.CastTo<string>() ?? "";
                    if (!value.StartsWith(likeToken.ToString()))
                    {
                        rule.Value = likeToken + value;
                    }
                }
                if (op == "like" || op == "notlike" || op == "startwith" || op == "notstartwith")
                {
                    var value = rule.Value.CastTo<string>() ?? "";
                    if (!value.EndsWith(likeToken.ToString()))
                    {
                        rule.Value = value + likeToken;
                    }
                }
                bulider.Append(paramPrefixToken + CreateFilterParam(rule.Value, rule.Type, rule.ParamName));
            }
            return bulider.ToString();
        }

        protected string CreateFilterParam(object value, string type, string pname)
        {
            string paramName = string.IsNullOrEmpty(pname) ? (paramPrefix + (paramCounter++)) : pname;
            type = (type ?? "").ToLower();
            DbParam param;
            switch (type)
            {
                case "int32":
                case "int16":
                case "int":
                case "digits":
                    var compare = value.CastTo<long?>();
                    param = compare > int.MaxValue ? new DbParam(paramName, compare, typeof(long)) : new DbParam(paramName, value.CastTo<int?>(), typeof(int));
                    break;

                case "int64":
                case "long":
                    param = new DbParam(paramName, value.CastTo<long?>(), typeof(long));
                    break;

                case "single":
                case "double":
                case "decimal":
                case "float":
                case "number":
                    param = new DbParam(paramName, value.CastTo<decimal?>(), typeof(decimal));
                    break;

                case "date":
                case "datetime":
                    param = new DbParam(paramName, value.CastTo<DateTime?>(), typeof(DateTime));
                    break;

                default:
                    param = value == null ? new DbParam(paramName, value, typeof(string)) : new DbParam(paramName, value);
                    break;
            }
            Parms.Add(param);
            return paramName;
        }

        public override string ToString()
        {
            var bulider = new StringBuilder();
            bulider.Append("CommandText:");
            bulider.Append(CommandText);
            bulider.AppendLine();
            bulider.AppendLine("Parms:");
            foreach (var parm in Parms)
            {
                bulider.AppendLine(string.Format("{0}:{1}", parm.Name, parm.Value));
            }
            return bulider.ToString();
        }

        /// <summary>
        /// 获取操作符的SQL Text
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public virtual string GetOperatorQueryText(string op)
        {
            return op switch
            {
                "equal" => " = ",
                "greater" => " > ",
                "greaterorequal" => " >= ",
                "isnull" => " is null ",
                "isnotnull" => " is not null ",
                "less" => " < ",
                "lessorequal" => " <= ",
                "like" => " like ",
                "notlike" => " not like ",
                "startwith" => " like ",
                "endwith" => " like ",
                "notstartwith" => " not like ",
                "notendwith" => " not like ",
                "notequal" => " <> ",
                "and" => " and ",
                "or" => " or ",
                "in" => " in ",
                "notin" => " not in ",
                _ => " = ",
            };
        }
    }
}