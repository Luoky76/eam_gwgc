using Chloe;
using Chloe.Reflection;
using Chloe.Reflection.Emit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections;
using System.Reflection;

namespace Gksyb.Common.Mvc.ModelBinding
{
    /// <summary>
    /// 参数处理
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public abstract class ParameterHandleAttribute : Attribute
    {
        /// <summary>
        /// 数据处理
        /// </summary>
        public abstract object Handle(object value);

        /// <summary>
        /// 序号
        /// </summary>
        public abstract int GetOrder();
    }

    /// <summary>
    /// 参数处理
    /// </summary>
    public class ParameterHandle : IDisposable
    {
        private readonly Dictionary<Type, List<MemberInfo>> _memberInfos = new();
        private readonly Dictionary<MemberInfo, IEnumerable<ParameterHandleAttribute>> _parameterHandles = new();
        private readonly Dictionary<MemberInfo, MemberGetter> _getterCache = new();
        private readonly Dictionary<MemberInfo, MemberSetter> _setterCache = new();
        private readonly ActionExecutingContext _context;
        private int times = 0;
        private const int MaxTimes = 20;

        public ParameterHandle(ActionExecutingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 参数二次加工
        /// </summary>
        public bool Handle()
        {
            var description = (ControllerActionDescriptor)_context.ActionDescriptor;
            var methodHandles = GetAttributes(description.MethodInfo);
            var isHandle = false;
            foreach (var param in _context.ActionDescriptor.Parameters.Cast<ControllerParameterDescriptor>())
            {
                if (typeof(IFormFile).IsAssignableFrom(param.ParameterType)) continue;
                if (param.BindingInfo != null && param.BindingInfo.BindingSource == BindingSource.Services) continue;
                if (!_context.ActionArguments.TryGetValue(param.Name, out object value) || value == null) continue;//空值不处理
                var paramHandles = GetAttributes(param.ParameterInfo, methodHandles);
                isHandle = HandleValue(param.ParameterType, paramHandles, () => value, newValue =>
                {
                    _context.ActionArguments[param.Name] = newValue;
                }) || isHandle;
            }
            return isHandle;
        }

        /// <summary>
        /// 遍历处理对象，处理数据
        /// </summary>
        private bool HandleValue(Type type, IEnumerable<ParameterHandleAttribute> handles, Func<object> getter, Action<object> setter, bool isAppend = true, bool isEnumerable = false)
        {
            var paramValue = getter();
            if (paramValue == null) return false;
            if (type == typeof(object)) type = paramValue.GetType();
            if (type.IsInterface) return false;//接口类型不需要处理
            var isSimpleType = type.IsSimpleType();
            if (!isAppend && isSimpleType && (handles == null || !handles.Any())) return false;//无处理对象的简单类型
            if (isSimpleType)//简单类型
            {
                handles = isAppend ? AddBaseFilter(handles) : handles;
                var newParamValue = HandleValueInner(paramValue, handles);
                if (newParamValue == paramValue) return false;
                setter(newParamValue);
                return true;
            }
            var isHandle = false;
            if (type.IsEnumerable())//集合处理每行数据
            {
                var listValue = paramValue as IEnumerable;
                var listSetter = GetEnumerableSetter(listValue);
                if (listSetter == null) return isHandle;//找不到设置对象不处理
                var index = 0;
                var setterValues = new Dictionary<int, object>();
                var memberType = type.GenericTypeArguments.Length == 1 ? type.GenericTypeArguments[0] : null;
                var listGetter = memberType == null ? GetEnumerableGetter(listValue) : null;
                foreach (var model in listValue)
                {
                    var modelValue = listGetter == null ? model : listGetter(index, model);
                    var modelType = memberType ?? modelValue?.GetType();
                    modelType ??= type.GenericTypeArguments.Length == 2 ? type.GenericTypeArguments[1] : typeof(object);
                    isHandle = HandleValue(modelType, handles, () => modelValue, newValue =>
                    {
                        setterValues.Add(index, newValue);
                    }, isAppend && modelType.IsSimpleType(), true) || isHandle;
                    index++;
                    if (memberType != null && !isHandle) return false;//无处理的泛型列表直接返回，加快效率
                }
                setterValues.ForEach(c =>
                {
                    listSetter(c.Value, c.Key);
                });
                return isHandle;
            }
            if (!isEnumerable && (++times) > MaxTimes) return false;//集合不算次数
            var memberInfos = GetMemberInfos(type);
            foreach (var memberInfo in memberInfos)//复杂类型
            {
                var memberType = memberInfo.GetMemberType();
                var memberHandles = GetAttributes(memberInfo, handles);
                isHandle = HandleValue(memberType, memberHandles, () => GetGetter(memberInfo)(paramValue), newValue =>
                {
                    GetSetter(memberInfo)(paramValue, newValue);
                }, isAppend) || isHandle;
            }
            return isHandle;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private static object HandleValueInner(object value, IEnumerable<ParameterHandleAttribute> handles)
        {
            if (handles == null) return value;
            foreach (var memberHandle in handles)
            {
                value = memberHandle.Handle(value);
            }
            return value;
        }

        /// <summary>
        /// 获取可枚举对象的设值
        /// </summary>
        private static Action<object, int> GetEnumerableSetter(IEnumerable source)
        {
            if (source is IList list)
            {
                return (newValue, index) => list[index] = newValue;
            }
            if (source is IDictionary dictionary)
            {
                return (newValue, index) =>
                {
                    var pos = 0;
                    foreach (var key in dictionary.Keys)
                    {
                        if (pos++ == index)
                        {
                            dictionary[key] = newValue;
                            break;
                        }
                    }
                };
            }
            return null;
        }

        /// <summary>
        /// 获取可枚举对象的设值
        /// </summary>
        private static Func<int, object, object> GetEnumerableGetter(IEnumerable source)
        {
            if (source is IDictionary dictionary)
            {
                return (index, value) =>
                {
                    var pos = 0;
                    foreach (var key in dictionary.Keys)
                    {
                        if (pos++ == index) return dictionary[key];
                    }
                    return value;
                };
            }
            return null;
        }

        /// <summary>
        /// 缓存memberInfo对象，加快list遍历效率
        /// </summary>
        private List<MemberInfo> GetMemberInfos(Type type)
        {
            if (!_memberInfos.TryGetValue(type, out var value))
            {
                value = new List<MemberInfo>(type.GetProperties().Where(c => c.CanRead && c.CanWrite).Cast<MemberInfo>().Concat(type.GetFields()));
                _memberInfos.Add(type, value);
            }
            return value;
        }

        /// <summary>
        /// 缓存ParameterHandleAttribute对象，加快list遍历效率
        /// </summary>
        private IEnumerable<ParameterHandleAttribute> GetParameterHandles(MemberInfo memberInfo)
        {
            if (!_parameterHandles.TryGetValue(memberInfo, out var value))
            {
                value = memberInfo.GetCustomAttributes<ParameterHandleAttribute>(false);
                _parameterHandles.Add(memberInfo, value);
            }
            return value;
        }

        /// <summary>
        /// 缓存取值对象，加快list遍历效率
        /// </summary>
        private MemberGetter GetGetter(MemberInfo memberInfo)
        {
            if (!_getterCache.TryGetValue(memberInfo, out var value))
            {
                value = DelegateGenerator.CreateGetter(memberInfo);
                _getterCache.Add(memberInfo, value);
            }
            return value;
        }

        /// <summary>
        /// 缓存设值对象，加快list遍历效率
        /// </summary>
        private MemberSetter GetSetter(MemberInfo memberInfo)
        {
            if (!_setterCache.TryGetValue(memberInfo, out var value))
            {
                value = DelegateGenerator.CreateSetter(memberInfo);
                _setterCache.Add(memberInfo, value);
            }
            return value;
        }

        /// <summary>
        /// 获取参数处理
        /// </summary>
        private IEnumerable<ParameterHandleAttribute> GetAttributes(MemberInfo methodInfo, IEnumerable<ParameterHandleAttribute> parentHandles = null)
        {
            var parameterHandles = GetParameterHandles(methodInfo);
            parameterHandles = parameterHandles.Any() ? parameterHandles : null;
            return CombineAndOrder(parameterHandles, parentHandles);
        }

        /// <summary>
        /// 获取参数处理
        /// </summary>
        private static IEnumerable<ParameterHandleAttribute> GetAttributes(ParameterInfo parameterInfo, IEnumerable<ParameterHandleAttribute> parentHandles)
        {
            var parameterHandles = parameterInfo.GetCustomAttributes<ParameterHandleAttribute>(false);
            parameterHandles = parameterHandles.Any() ? parameterHandles : null;
            return CombineAndOrder(parameterHandles, parentHandles);
        }

        /// <summary>
        /// 合并并排序
        /// </summary>
        private static IEnumerable<ParameterHandleAttribute> CombineAndOrder(IEnumerable<ParameterHandleAttribute> parameterHandles, IEnumerable<ParameterHandleAttribute> parentHandles = null)
        {
            if (parameterHandles == null) return parentHandles;
            if (parentHandles == null) return parameterHandles;
            return parameterHandles.Concat(parentHandles.Where(c => !parameterHandles.Any(a => a.GetType() == c.GetType()))).OrderBy(c => c.GetOrder()).ToList();
        }

        /// <summary>
        /// 加入sql过滤
        /// </summary>
        private static IEnumerable<ParameterHandleAttribute> AddBaseFilter(IEnumerable<ParameterHandleAttribute> handles)
        {
            handles ??= new List<ParameterHandleAttribute>();
            if (!handles.Any(c => c is SqlFilterAttribute))
                handles = handles.Append(new SqlFilterAttribute());
            return handles;
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        public void Dispose()
        {
            _memberInfos?.Clear();
            _parameterHandles?.Clear();
            _getterCache?.Clear();
            _setterCache?.Clear();
            GC.SuppressFinalize(this);
        }
    }
}