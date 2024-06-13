using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Gksyb.Common
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// 获取方法调用参数
        /// </summary>
        /// <returns></returns>
        public static object[] GetParametersValue(this MethodInfo methodInfo, JToken jToken)
        {
            var parameters = new List<object>();
            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length < 1) return parameters.ToArray();
            var dic = jToken.Type == JTokenType.Object ? jToken.ToObject<Dictionary<string, JToken>>() : null;//转字典
            var form = (dic ?? new Dictionary<string, JToken>()).ToIgnoreCaseDictionary();
            foreach (var param in parameterInfos)
            {
                if (form.TryGetValue(param.Name, out JToken value))
                {
                    parameters.Add(value.ToObject(param.ParameterType));
                }
                else
                {
                    try
                    {
                        parameters.Add(jToken.ToObject(param.ParameterType));
                    }
                    catch
                    {
                        parameters.Add(param.HasDefaultValue ? param.DefaultValue : null);
                    }
                }
            }
            return parameters.ToArray();
        }

        /// <summary>
        /// 获取方法调用参数
        /// </summary>
        /// <returns></returns>
        public static object[] GetParametersValue(this MethodInfo methodInfo, string json)
        {
            var parameters = new List<object>();
            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length < 1) return parameters.ToArray();
            var dic = json.StartsWith("{") ? json.ToObject<Dictionary<string, JToken>>() : null;
            var form = (dic ?? new Dictionary<string, JToken>()).ToIgnoreCaseDictionary();
            foreach (var param in parameterInfos)
            {
                if (form.TryGetValue(param.Name, out JToken value))
                {
                    parameters.Add(value.ToObject(param.ParameterType));
                    continue;
                }
                if (param.ParameterType.IsSimpleType())//简单类型
                {
                    if (param.Name == "json" && param.ParameterType == typeof(string))//参数名json特殊处理
                    {
                        parameters.Add(json);
                    }
                    continue;
                }
                try
                {
                    parameters.Add(JSONHelper.FromJson(json, param.ParameterType));
                }
                catch (Exception)
                {
                    parameters.Add(param.HasDefaultValue ? param.DefaultValue : null);
                }
            }
            return parameters.ToArray();
        }
    }
}