#pragma warning disable IDE0051,IDE0052 // 删除未使用的私有成员

using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;
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
                try
                {
                    var key = view.Key.Split("@#").FirstOrDefault();
                    if (_methodInfos.ContainsKey(key))
                    {
                        var invokeResult = _methodInfos[key].Invoke(this, new object[] { view.Value });
                        if (invokeResult is Task<List<ComboxData>> task)//返回值判断
                        {
                            result.TryAdd(view.Key, await task);
                            return;
                        }
                        if (invokeResult is List<ComboxData> comboxData)
                        {
                            result.TryAdd(view.Key, comboxData);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                result.TryAdd(view.Key, new List<ComboxData>());
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
        /// 设备分类编码下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceTypeCode(Expression<Func<BASE_DEVICETYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICETYPE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.TYPE_CODE, TEXT = c.TYPE_CODE, VALUE = c.TYPE_CODE })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 设备分类名称下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceTypeName(Expression<Func<BASE_DEVICETYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICETYPE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.TYPE_ID, TEXT = c.TYPE_NAME, VALUE = c.TYPE_CODE })
                .Distinct()
               .ToListAsync();
        }
        /// <summary>
        /// 设备构造树编码下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceComposeCode(Expression<Func<BASE_DEVICE_COMPOSE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICE_COMPOSE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.COMPOSE_CODE, TEXT = c.COMPOSE_CODE, VALUE = c.COMPOSE_CODE })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 设备构造树名称下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceComposeName(Expression<Func<BASE_DEVICE_COMPOSE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICE_COMPOSE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.COMPOSE_ID, TEXT = c.COMPOSE_NAME, VALUE = c.COMPOSE_NAME })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 构造类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ConsType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "constype").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }
        
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