using Flurl.Http;
using Magicodes.IE.EPPlus;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gksyb.Common.Office.Core
{
    internal static class OfficeObjectExtensions
    {
        private static readonly Regex Base64Regex = new(@"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.Compiled);

        /// <summary>
        /// 将 DateTimeOffset 转换成本地 DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return dateTime.ToLocalTime().DateTime;
            else
                return dateTime.DateTime;
        }

        internal static void ValueMapping(this PropertyInfo propertyInfo, ref Dictionary<string, dynamic> directory)
        {
            var valueMappings = propertyInfo.GetAttributes<ValueMappingsBaseAttribute>(true).FirstOrDefault()?.GetMappings(propertyInfo);
            if (valueMappings != null)
            {
                foreach (var valueMapping in valueMappings)
                {
                    if (!directory.ContainsKey(valueMapping.Key)) directory.Add(valueMapping.Key, valueMapping.Value);
                }
                if (valueMappings.Count > 0) return;
            }
            var mappings = propertyInfo.GetAttributes<ValueMappingAttribute>().ToList();
            var objects = directory;
            foreach (var mappingAttribute in mappings.Where(mappingAttribute =>
                !objects.ContainsKey(mappingAttribute.Text)))
                directory.Add(mappingAttribute.Text, mappingAttribute.Value);

            //如果存在自定义映射，则不会生成默认映射
            if (mappings.Any()) return;

            var type = propertyInfo.PropertyType.GetUnNullableType();
            if (type == typeof(bool))
            {
                if (!directory.ContainsKey("是")) directory.Add("是", true);
                if (!directory.ContainsKey("否")) directory.Add("否", false);
            }
            if (!type.IsEnum) return;
            //为枚举类型生成默认映射
            if (propertyInfo.PropertyType.IsNullableType() && !directory.ContainsKey(string.Empty))
            {
                directory.Add(string.Empty, null);
            }
            var values = type.GetEnumTextAndValues();
            var dictionary = directory;
            foreach (var value in values.Where(value => !dictionary.ContainsKey(value.Key)))
                directory.Add(value.Key, value.Value);
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// key :返回显示名称或者描述
        /// value：值
        /// </returns>
        public static IDictionary<string, int> GetEnumTextAndValues(this Type type)
        {
            if (!type.IsEnum) throw new InvalidOperationException();
            var items = type.GetEnumDefinitionList();
            var dic = new Dictionary<string, int>();
            //枚举名 值 显示名称 描述
            foreach (var tuple in items)
            {
                //如果描述、显示名不存在，则返回枚举名称
                dic.Add(tuple.Item4 ?? tuple.Item3 ?? tuple.Item1, tuple.Item2);
            }
            return dic;
        }

        /// <summary>
        /// 获取枚举定义列表
        /// </summary>
        /// <returns>返回枚举列表元组（名称、值、显示名、描述）</returns>
        public static IEnumerable<Tuple<string, int, string, string>> GetEnumDefinitionList(this Type type)
        {
            var list = new List<Tuple<string, int, string, string>>();
            var attrType = type;
            if (!attrType.IsEnum) return null;
            var names = Enum.GetNames(attrType);
            var values = Enum.GetValues(attrType);
            var index = 0;
            foreach (var value in values)
            {
                var name = names[index];
                var field = value.GetType().GetField(value.ToString());
                var displayName = field.GetDisplayName();
                var des = field.GetAttribute<DescriptionAttribute>()?.Description;
                var item = new Tuple<string, int, string, string>(
                    name,
                    Convert.ToInt32(value),
                    displayName.IsNullOrWhiteSpace() ? null : displayName,
                    des.IsNullOrWhiteSpace() ? null : des
                );
                list.Add(item);
                index++;
            }

            return list;
        }

        public static bool IsBase64String(this string source)
        {
            if (string.IsNullOrEmpty(source) || source.Length % 4 != 0)
                return false;
            return Base64Regex.IsMatch(source.Replace("\r", "").Replace("\n", ""));
        }

        public static ExcelImage Base64StringToImage(this string base64String)
        {
            var content = Convert.FromBase64String(Regex.Replace(base64String, @"\s+", string.Empty));
            return ExcelImage.Decode(content);
        }

        public static ExcelImage GetImageByUrl(this string url)
        {
            var content = url.GetBytesAsync().Result();
            return ExcelImage.Decode(content);
        }

        /// <summary>
        /// 分解数据表
        /// </summary>
        /// <param name="originalTab">需要分解的表</param>
        /// <param name="rowsNum">每个表包含的数据量</param>
        /// <returns></returns>
        public static DataSet SplitDataTable(this DataTable originalTab, int rowsNum = 1000000)
        {
            //获取所需创建的表数量
            var tableNum = originalTab.Rows.Count / rowsNum;

            //获取数据余数
            var remainder = originalTab.Rows.Count % rowsNum;

            if (remainder != 0) tableNum += 1;

            var ds = new DataSet();

            //如果只需要创建1个表，直接将原始表存入DataSet
            if (tableNum == 1)
            {
                ds.Tables.Add(originalTab);
            }
            else
            {
                var tableSlice = new DataTable[tableNum];

                //Save orginal columns into new table.
                for (var c = 0; c < tableNum; c++)
                {
                    tableSlice[c] = new DataTable();
                    foreach (DataColumn dc in originalTab.Columns)
                        tableSlice[c].Columns.Add(dc.ColumnName, dc.DataType);
                }

                //Import Rows
                for (var i = 0; i < tableNum; i++)
                    if (remainder == 0)
                    {
                        for (var j = i * rowsNum; j < (i + 1) * rowsNum; j++)
                            tableSlice[i].ImportRow(originalTab.Rows[j]);
                    }
                    else
                    {
                        // if the current table is not the last one
                        if (i != tableNum - 1)
                            for (var j = i * rowsNum; j < (i + 1) * rowsNum; j++)
                                tableSlice[i].ImportRow(originalTab.Rows[j]);
                        else
                            for (var k = i * rowsNum; k < i * rowsNum + remainder; k++)
                                tableSlice[i].ImportRow(originalTab.Rows[k]);
                    }

                //add all tables into a dataset
                foreach (var dt in tableSlice) ds.Tables.Add(dt);
            }

            return ds;
        }

        /// <summary>
        /// 获取私有属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyname"></param>
        /// <returns></returns>
        public static T GetPrivateProperty<T>(this object instance, string propertyname)
        {
            Type type = instance.GetType().BaseType;
            FieldInfo[] finfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var field = finfos.FirstOrDefault(f => f.Name == propertyname);
            return (T)field.GetValue(instance);
        }

        public static PropertyInfo[] GetSortedPropertyInfos(this Type t)
        {
            var props = t.GetProperties();
            var noIndex = new List<PropertyInfo>();
            var hasIndex = new Dictionary<int, PropertyInfo>();
            var result = new PropertyInfo[props.Length];
            foreach (var propertyInfo in props)
            {
                var index = propertyInfo.GetAttribute<ImporterHeaderAttribute>()?.ColumnIndex;
                if (index != null && index != 0)
                {
                    hasIndex.Add(index.Value, propertyInfo);
                }
                else
                {
                    noIndex.Add(propertyInfo);
                }
            }

            for (var i = 0; i < props.Length; i++)
            {
                if (hasIndex.ContainsKey(i + 1))
                {
                    result[i] = hasIndex[i + 1];
                    hasIndex.Remove(i + 1);
                }
                else
                {
                    var firstNoIndex = noIndex.FirstOrDefault();
                    if (firstNoIndex == null)
                    {
                        var minIndex = hasIndex.Keys.Min();
                        result[i] = hasIndex[minIndex];
                        hasIndex.Remove(minIndex);
                    }
                    else
                    {
                        result[i] = firstNoIndex;
                        noIndex.Remove(firstNoIndex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取集合连续数据中最大的
        /// </summary>
        /// <param name="numList"></param>
        /// <returns></returns>
        public static int GetLargestContinuous(this List<int> numList)
        {
            for (int i = 0; i < numList.Count;)
            {
                if (numList.Count > i + 1 && numList[i] - numList[i + 1] == 1)
                {
                    //忽略
                }

                return numList[i];
            }

            return 0;
        }

        /// <summary>
        /// 是否必填
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsRequired(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetAttribute<RequiredAttribute>(true) != null) return true;
            //Boolean、Byte、SByte、Int16、UInt16、Int32、UInt32、Int64、UInt64、Char、Double、Single
            if (propertyInfo.PropertyType.IsPrimitive) return true;
            return propertyInfo.PropertyType.Name switch
            {
                "DateTime" or "Decimal" => true,
                _ => false,
            };
        }

        public static string SaveTo(this ExcelImage image, string path)
        {
            using var stream = File.Create(path);
            image.Encode(stream);
            return path;
        }

        public static string ToBase64String(this ExcelImage image)
        {
            return Convert.ToBase64String(image.GetContent());
        }

        /// <summary>
        /// 检查文件名
        /// </summary>
        /// <param name="fileName"></param>
        public static void CheckExcelFileName(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName), "文件名不能为空！");
            if (!Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("仅支持导出“.xlsx”，即不支持Excel97-2003!", nameof(fileName));
            }
        }
    }
}