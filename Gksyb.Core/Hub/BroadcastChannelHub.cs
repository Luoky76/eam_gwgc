using Gksyb.Core.Auth;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR
{
    /// <summary>
    /// 消息通道
    /// </summary>
    [GksybAuthorize(true)]
    public class BroadcastChannelHub : Hub<IBroadcastChannelClient>
    {
        private static readonly Type _hubType = typeof(Hub<IBroadcastChannelClient>);
        private const string _assemblyName = "Microsoft.AspNetCore.SignalR.Channel";

        /// <summary>
        /// 方法缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> _cache = new();

        /// <summary>
        /// 多线程对象锁
        /// </summary>
        private static readonly object _lockObj = new();

        /// <summary>
        /// 获取调用方法
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static MethodInfo GetMethodInfo(string typeName, string methodName)
        {
            var name = $"{typeName}.{methodName}";
            if (!_cache.TryGetValue(name, out MethodInfo methodInfo))
            {
                lock (_lockObj)
                {
                    if (!_cache.TryGetValue(name, out methodInfo))
                    {
                        var type = ParsingConfig.Default.CustomTypeProvider.ResolveType(typeName);
                        if (!type.IsAssignableTo(_hubType)) throw new HubException("类型错误");
                        methodInfo = type.GetMethod(methodName);
                        _cache.GetOrAdd(name, methodInfo);
                    }
                }
            }
            return methodInfo;
        }

        /// <summary>
        /// 设置初始值
        /// </summary>
        /// <param name="hub"></param>
        private void InitValue(Hub<IBroadcastChannelClient> hub)
        {
            hub.Clients = Clients;
            hub.Context = Context;
            hub.Groups = Groups;
        }

        /// <summary>
        /// 动态调用方法
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        public async Task Excute(ActionData<JToken> actionData)
        {
            var actions = (actionData.Action ?? "").Split("/");
            if (actions.Length != 2) throw new HubException("参数错误");
            var typeName = actions[0] ?? "";
            var methodName = actions[1] ?? "";
            typeName = typeName.Contains('.') ? typeName : $"{_assemblyName}.{typeName}";
            var methodInfo = GetMethodInfo(typeName, methodName) ?? throw new HubException($"找不到方法{methodName}");
            var httpContext = Context.GetHttpContext();
            var isValid = await methodInfo.Valid(httpContext);
            if (!isValid)
            {
                await Clients.Error("您无权进行此操作");
                return;
            }
            object obj = null;
            if (!methodInfo.IsStatic)
            {
                obj = httpContext.RequestServices.GetService(methodInfo.ReflectedType);
                InitValue(obj as Hub<IBroadcastChannelClient>);
            }
            var values = methodInfo.GetParametersValue(actionData.Data);
            methodInfo.Invoke(obj, values);
        }
    }
}