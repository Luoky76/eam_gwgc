#pragma warning disable IDE0051,IDE0052 // 删除未使用的私有成员

using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
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
        private readonly UserSession _userSession;
        /// <summary>
        /// 下拉数据
        /// </summary>
        public ComboxDataService(IDbContext dbContext, IBCCodeService codeService, UserSession userSession)
        {
            _dbContext = dbContext;
            _codeService = codeService;
            _userSession = userSession;
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
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 设备运行状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RunStatus(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "run_status").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 维保部门
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> MaintDept(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "ship_dept").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }
        /// <summary>
        /// 维修类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepairType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "maint_type").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }
        /// <summary>
        /// 维修来源类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepSourceType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "source_type").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }
        /// <summary>
        /// 委外状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepOutType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "out_status").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }
        /// <summary>
        /// 维修处理方式
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepairDealType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "deal_type").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 设备卡片
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceInfo(Expression<Func<DEVICE_CARD, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            var qry = _dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID);
            return await qry.Where(predicate).Where(c => c.AUDITING=="1")
                .Select(c => new ComboxData()
                {
                    ID = c.DEVICE_ID,
                    TEXT = c.DEVICE_NAME,
                    VALUE = c.DEVICE_NO,
                    EXTEND =c.STATUS,
                    EXTEND1 =c.DEVICE_TYPE,
                    EXTEND2 =c.TYPE_NAME,
                })
               .ToListAsync();
        }
        /// <summary>
        /// 停机分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> StopSource(Expression<Func<RUN_STOP_TYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<RUN_STOP_TYPE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.STOP_TYPE_ID, TEXT = c.STOP_NAME, VALUE = c.STOP_NAME })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 故障分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> MalType(Expression<Func<RUN_MAL_TYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<RUN_MAL_TYPE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.MAL_TYPE_ID, TEXT = c.MAL_TYPE_NAME, VALUE = c.MAL_TYPE_NAME })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 记录状态下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> Auditing(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "auditing")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 供应商名下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ProviderName(Expression<Func<PROVIDER, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<PROVIDER>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.PROVIDER_ID, TEXT = c.PROVIDER_NAME, VALUE = c.PROVIDER_NAME })
                .Distinct()
                .ToListAsync();
        }


        /// <summary>
        /// 盘点状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ScanStatus(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "scan_status").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 部门
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeptData(Expression<Func<CF_CORP, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            var corpPath = _dbContext.Query<CF_CORP>().Where(predicate)
                .Select(c => c.CORP_PATH).ToList().Join();
            return await _dbContext.Query<CF_CORP>()
                .Where(a => (","+a.CORP_PATH).Contains(","+corpPath))
                .Select(c => new ComboxData() { ID = c.CORPID, TEXT = c.CNAME, VALUE = c.CNO })
                .Distinct()
                .ToListAsync();
        }


        /// <summary>
        /// 评估基础下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> AssessBaseContent(Expression<Func<PROVIDER_ASSESS_BASE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<PROVIDER_ASSESS_BASE>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.ASSESS_BASE_ID, TEXT = c.CONTENT, VALUE = c.CONTENT })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 用户ID、真实姓名下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> User(Expression<Func<CF_USER, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<CF_USER>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.USERID, TEXT = c.REALNAME, VALUE = c.REALNAME })
                .Distinct()
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