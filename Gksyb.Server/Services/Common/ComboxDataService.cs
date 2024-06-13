#pragma warning disable IDE0051,IDE0052 // 删除未使用的私有成员

using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Reflection;

namespace Gksyb.Server.Services.Common
{
    /// <summary>
    /// 通用服务
    /// </summary>

    public class ComboxDataService : IComboxDataService
    {
        private readonly IDbContext _dbContext;
        private readonly IBCCodeService _codeService;

        /// <summary>
        /// 下拉数据
        /// </summary>
        public ComboxDataService(IDbContext dbContext, IBCCodeService codeService)
        {
            _dbContext = dbContext;
            _codeService = codeService;
        }

        /// <summary>
        /// 获取下拉数据
        /// </summary>
        /// <param name="views"></param>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> Get(IDictionary<string, object> views)
        {
            var result = new ConcurrentDictionary<string, List<ComboxData>>();
            await Parallel.ForEachAsync(views, async (view, token) =>//开启多线程
            {
                var keys = view.Key.Split("@#");
                var key = keys.LastOrDefault();
                try
                {
                    var name = keys.FirstOrDefault();
                    if (_methodInfos.ContainsKey(name))
                    {
                        var invokeResult = _methodInfos[name].Invoke(this, new object[] { view.Value });
                        if (invokeResult is Task<List<ComboxData>> task)//返回值判断
                        {
                            result.TryAdd(key, await task);
                            return;
                        }
                        if (invokeResult is List<ComboxData> comboxData)
                        {
                            result.TryAdd(key, comboxData);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                result.TryAdd(key, new List<ComboxData>());
            });
            return result;
        }

        /// <summary>
        /// 字典下拉
        /// </summary>
        /// <param name="codeType">类型</param>
        /// <returns></returns>
        private async Task<List<ComboxData>> BCCode(string codeType)
        {
            return await _codeService.Get(codeType);
        }

        private static readonly Dictionary<string, MethodInfo> _methodInfos = null;

        /// <summary>
        /// 初始化
        /// </summary>
        static ComboxDataService()
        {
            _methodInfos = typeof(ComboxDataService).GetDicMethods();
        }
    }
}
#pragma warning restore IDE0051, IDE0052 // 删除未使用的私有成员