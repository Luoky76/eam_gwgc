using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Gksyb.Common
{
    /// <summary>
    /// 扩展String的方法 <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        #region 正则表达式

        /// <summary>
        /// 指示所指定的正则表达式在指定的输入字符串中是否找到了匹配项
        /// </summary>
        /// <param name="value">要搜索匹配项的字符串</param>
        /// <param name="pattern">要匹配的正则表达式模式</param>
        /// <param name="isContains">是否包含，否则全匹配</param>
        /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false</returns>
        public static bool IsMatch(this string value, string pattern, bool isContains = true)
        {
            if (value == null)
            {
                return false;
            }
            return isContains
                ? Regex.IsMatch(value, pattern)
                : Regex.Match(value, pattern).Success;
        }

        /// <summary>
        /// 是否电子邮件
        /// </summary>
        public static bool IsEmail(this string value)
        {
            const string pattern = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否是IP地址
        /// </summary>
        public static bool IsIpAddress(this string value)
        {
            const string pattern = @"^((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否是整数
        /// </summary>
        public static bool IsNumeric(this string value)
        {
            const string pattern = @"^\-?[0-9]+$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否是Unicode字符串
        /// </summary>
        public static bool IsUnicode(this string value)
        {
            const string pattern = @"^[\u4E00-\u9FA5\uE815-\uFA29]+$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否Url字符串
        /// </summary>
        public static bool IsUrl(this string value)
        {
            const string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#!]*[\w\-\@?^=%&amp;/~\+#!])?$";
            return value.IsMatch(pattern);
        }

        /// <summary>
        /// 是否身份证号，验证如下3种情况：
        /// 1.身份证号码为15位数字；
        /// 2.身份证号码为18位数字；
        /// 3.身份证号码为17位数字+1个字母
        /// </summary>
        public static bool IsIdentityCard(this string value)
        {
            if (value.Length != 15 && value.Length != 18)
            {
                return false;
            }
            Regex regex;
            string[] array;
            DateTime time;
            if (value.Length == 15)
            {
                regex = new Regex(@"^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})_");
                if (!regex.Match(value).Success)
                {
                    return false;
                }
                array = regex.Split(value);
                return DateTime.TryParse(string.Format("{0}-{1}-{2}", "19" + array[2], array[3], array[4]), out time);
            }
            regex = new Regex(@"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9Xx])$");
            if (!regex.Match(value).Success)
            {
                return false;
            }
            array = regex.Split(value);
            if (!DateTime.TryParse(string.Format("{0}-{1}-{2}", array[2], array[3], array[4]), out time))
            {
                return false;
            }
            //校验最后一位
            string[] chars = value.ToCharArray().Select(m => m.ToString()).ToArray();
            int[] weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                int num = int.Parse(chars[i]);
                sum += num * weights[i];
            }
            int mod = sum % 11;
            string vCode = "10X98765432";//检验码字符串
            string last = vCode.ToCharArray().ElementAt(mod).ToString();
            return chars.Last().ToUpper() == last;
        }

        /// <summary>
        /// 是否手机号码
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isRestrict">是否按严格格式验证</param>
        public static bool IsMobileNumber(this string value, bool isRestrict = false)
        {
            string pattern = isRestrict ? @"^[1][3-8]\d{9}$" : @"^[1]\d{10}$";
            return value.IsMatch(pattern);
        }

        #endregion 正则表达式

        #region 其他操作

        /// <summary>
        /// 根据字符串进行分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitString"></param>
        /// <returns></returns>
        public static string[] Split(this string str, string splitString)
        {
            return str.Split(new[] { splitString }, StringSplitOptions.None);
        }

        /// <summary>
        /// 防止sql注入
        /// </summary>
        public static string SqlFilter(this string value, int? limit = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (Regex.IsMatch(value, @"(insert\s+|union\s+|update\s+|delete\s+|select\s+|\s+or\s+|\s+=\s+|\s+dual\s+|create\s+|declare\s+|exec\s+|sys\.|dbms_|extractvalue|dburitype)", RegexOptions.IgnoreCase))
                throw new MessageException("防注入系统");
            if (limit != null && value.Length > limit && Regex.Replace(value, $@"^((\w|\.|,){{{limit},}})|(\basc\b)|(\bdesc\b)|( )", "", RegexOptions.IgnoreCase).Length > limit)
                throw new MessageException("防注入系统");
            return value;
        }

        /// <summary>
        /// 为sql语句格式化字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatSql(this string value, params object[] args)
        {
            for (var l = args.Length - 1; l >= 0; l--)
            {
                if (args[l] is string)
                {
                    args[l] = (args[l] as string).SqlFilter();
                }
            }
            return string.Format(value, args);
        }

        /// <summary>
        /// 字符串格式化 去除前后空格和制表符,换行符,回车符
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Format(this string value)
        {
            if (value == null) return value;
            return value.Trim().Replace((char)9, (char)0).Replace((char)10, (char)0).Replace((char)13, (char)0);
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string value)
        {
            if (value.IsNullOrEmpty()) return value;
            if (value.Length == 1) return value.ToUpper();
            return value[..1].ToUpper() + value[1..];
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToVariableCase(this string value)
        {
            if (value.IsNullOrEmpty()) return value;
            if (value.Length == 1) return value.ToLower();
            return value[..1].ToLower() + value[1..];
        }

        /// <summary>
        /// 是否包括值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 指示指定的字符串是 null 还是 System.String.Empty 字符串
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 指示指定的字符串是 null、空还是仅由空白字符组成。
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 为指定格式的字符串填充相应对象来生成字符串
        /// </summary>
        /// <param name="format">字符串格式，占位符以{n}表示</param>
        /// <param name="args">用于填充占位符的参数</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            if (format.IsNullOrEmpty()) return format;
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// 以指定字符串作为分隔符将指定字符串分隔成数组
        /// </summary>
        /// <param name="value">要分割的字符串</param>
        /// <param name="strSplit">字符串类型的分隔符</param>
        /// <param name="removeEmptyEntries">是否移除数据中元素为空字符串的项</param>
        /// <returns>分割后的数据</returns>
        public static string[] Split(this string value, string strSplit, bool removeEmptyEntries = false)
        {
            return value.Split(new[] { strSplit }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        /// <summary>
        /// 支持汉字的字符串长度，汉字长度计为2
        /// </summary>
        /// <param name="value">参数字符串</param>
        /// <returns>当前字符串的长度，汉字长度为2</returns>
        public static int TextLength(this string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            return bytes.Length + bytes.Count(b => b == 63);
        }

        /// <summary>
        /// 获取子字符串 超过指定长度则获取剩余字符串
        /// </summary>
        /// <param name="value">参数字符串</param>
        /// <param name="startIndex">开始位置</param>
        /// <param name="length">长度 超过则获取剩余字符串</param>
        /// <param name="isASCII">是否获取ASCII 汉字算两个字符</param>
        /// <returns></returns>
        public static string SubStr(this string value, int startIndex, int length = 0, bool isASCII = false)
        {
            if (value.IsNullOrEmpty()) return value;
            if (startIndex >= value.Length) return "";
            if (length == 0) length = value.Length - startIndex;
            var subStr = value.Substring(startIndex, value.Length > (startIndex + length) ? length : (value.Length - startIndex));
            if (isASCII)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(subStr);
                int tempLen = 0;
                int i = 0;
                foreach (byte b in bytes)
                {
                    if (b == 63) tempLen += 2;
                    else tempLen += 1;
                    if (tempLen >= length)
                    {
                        if (tempLen == length) i += 1;
                        break;
                    }
                    i += 1;
                }
                subStr = subStr[..i];
            }
            return subStr;
        }

        /// <summary>
        /// 给URL添加查询参数
        /// </summary>
        /// <param name="url">URL字符串</param>
        /// <param name="queries">要添加的参数，形如："id=1,cid=2"</param>
        /// <returns></returns>
        public static string AddUrlQuery(this string url, params string[] queries)
        {
            foreach (string query in queries)
            {
                if (!url.Contains('?'))
                {
                    url += "?";
                }
                else if (!url.EndsWith("&"))
                {
                    url += "&";
                }

                url += query;
            }
            return url;
        }

        /// <summary>
        /// 获取URL中指定参数的值，不存在返回空字符串
        /// </summary>
        public static string GetUrlQuery(this string url, string key)
        {
            var uri = new Uri(url);
            string query = uri.Query;
            if (query.IsNullOrEmpty())
            {
                return string.Empty;
            }
            query = query.TrimStart('?');
            var dict = (from m in query.Split("&", true)
                        let strs = m.Split("=")
                        select new KeyValuePair<string, string>(strs[0], strs[1]))
                .ToDictionary(m => m.Key, m => m.Value);
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// 转全角的函数(SBC case)
        /// </summary>
        /// <param name="value">任意字符串</param>
        /// <returns>全角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public static string ToSBC(this string value)
        {
            if (value == null) return value;
            //半角转全角：
            char[] c = value.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)

                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 转半角的函数(DBC case)
        /// </summary>
        /// <param name="value">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public static string ToDBC(this string value)
        {
            if (value == null) return value;
            char[] c = value.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;

                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)

                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 是否包含中文和中文标点符号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasChinese(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Regex.IsMatch(value, @"[\u4e00-\u9fa5]") || Regex.IsMatch(value, @"[、，。；？~！：‘“”’【】（）]");
        }

        /// <summary>
        /// 获取中文字符串的首字母
        /// </summary>
        public static string GetChineseSpell(this string cnString)
        {
            if (!cnString.IsMatch(@"[\u4E00-\u9FA5]"))
            {
                throw new ArgumentException("参数不是中文字符串", nameof(cnString));
            }
            int length = cnString.Length;
            string result = null;
            for (int i = 0; i < length; i++)
            {
                result += GetChineseSpell(cnString[i]);
            }
            return result;
        }

        /// <summary>
        /// 获取单个中文字符的拼音首字母
        /// </summary>
        /// <param name="cnChar"></param>
        /// <returns></returns>
        public static string GetChineseSpell(this char cnChar)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(cnChar.ToString());
            if (bytes.Length > 1)
            {
                int area = (short)bytes[0];
                int pos = (short)bytes[1];
                int code = (area << 8) + pos;
                int[] areacode = { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481 };

                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25)
                    {
                        max = areacode[i + 1];
                    }
                    if (areacode[i] <= code && code < max)
                    {
                        return Encoding.UTF8.GetString(new byte[] { (byte)(97 + i) }).ToUpper();
                    }
                }
                return "*";
            }
            return cnChar.ToString();
        }

        /// <summary>
        /// 将字符串进行Unicode编码，变成形如“\u7f16\u7801”的形式
        /// </summary>
        /// <param name="source">要进行编号的字符串</param>
        public static string ToUnicodeString(this string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0:x2}{1:x2}", bytes[i + 1], bytes[i]);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 将形如“\u7f16\u7801”的Unicode字符串解码
        /// </summary>
        public static string FromUnicodeString(this string source)
        {
            var regex = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
            return regex.Replace(source,
                m =>
                {
                    if (short.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out short s))
                    {
                        return "" + (char)s;
                    }
                    return m.Value;
                });
        }

        #endregion 其他操作

        /// <summary>
        /// 增加String
        /// </summary>
        /// <param name="str"></param>
        /// <param name="addString"></param>
        /// <returns></returns>
        public static string Add(this string str, string addString)
        {
            return IsNullOrEmpty(str) ? "" : str + addString;
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="str"></param>
        /// <param name="to"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool EqualsTo(this string str, string to, bool ignoreCase)
        {
            return string.Compare(str, to, ignoreCase) == 0;
        }

        /// <summary>
        /// 是否相等，不区分大小写
        /// </summary>
        /// <param name="str"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool EqualsTo(this string str, string to)
        {
            return str.EqualsTo(to, false);
        }

        /// <summary>
        /// 查找第一个非空字符串
        /// </summary>
        /// <returns></returns>
        public static string FindFirstHasValue(this string source, params string[] others)
        {
            if (!string.IsNullOrWhiteSpace(source)) return source;
            foreach (var other in others)
            {
                if (!string.IsNullOrWhiteSpace(other)) return other;
            }
            return source;
        }
    }
}