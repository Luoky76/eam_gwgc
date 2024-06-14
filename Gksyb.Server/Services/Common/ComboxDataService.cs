#pragma warning disable IDE0051,IDE0052 // 删除未使用的私有成员

using Gksyb.Core.Auth;
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
        /// 设备分类编码下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceTypeCode(Expression<Func<BASE_DEVICETYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICETYPE>().Where(predicate)
                .Select(c => new ComboxData() { ID = c.TYPE_CODE, TEXT = c.TYPE_CODE, VALUE = c.PRE_TYPEID })
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
            var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DEVICETYPE>(c => c.STATUS!="0").Where(predicate)
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
                .Select(c => new ComboxData() { ID = c.COMPOSE_ID, TEXT = c.COMPOSE_NAME, VALUE = c.TYPE_ID, EXTEND =c.TYPE_NAME, EXTEND1=c.TYPE_CODE })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 物资分类名称下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpTypeName(Expression<Func<BASE_SPTYPE, bool>> predicate)
        {
            var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_SPTYPE>(c => c.IS_CANCEL!="1").Where(predicate)
                .Select(c => new ComboxData() { ID = c.TYPE_ID, TEXT = c.TYPE_NAME, VALUE = c.TYPE_CODE })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 物资目录名称下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpCatalogName(Expression<Func<BASE_SPCATALOG, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_SPCATALOG>(c => c.IS_CANCEL != "1"&&c.IS_RECOVERY != "1").Where(predicate)
                .Select(c => new ComboxData() { ID = c.SP_ID, TEXT = c.SP_NAME, VALUE = c.SP_NAME })
                .Distinct()
               .ToListAsync();
        }

        /// <summary>
        /// 船舶常规物料下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> LaborMaterialCard(Expression<Func<BASE_SPCATALOG, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_SPCATALOG>(c => c.IS_CANCEL != "1")
                .Where(predicate)
                .OrderBy(c => c.SP_CODE)
                .Select(c => new ComboxData()
                {
                    ID = c.SP_ID,
                    TEXT = c.SP_NAME,
                    VALUE = c.SP_CODE,
                    EXTEND = c.SP_SIZE,
                    EXTEND1 = c.TYPE_ID,
                    EXTEND2 = c.TYPE_NAME,
                    EXTEND3 = c.TYPE_CODE,
                    EXTEND4 = c.UNIT
                })
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
        /// 当前管理状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> AssetStatus(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "assetStatus").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 无形资产产品类型下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> AssetProductType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "assetProductType")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 固定资产设备类型下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> AssetDeviceType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "assetDeviceType")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.SID, TEXT = c.CODE_CN, VALUE = c.CODE_EN })
                .ToListAsync();
        }

        /// <summary>
        /// IT固定资产设备信息下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> AssetCard(Expression<Func<ASSET_CARD, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<ASSET_CARD>()
                .Where(predicate)
                .Where(c => c.AUDITING == "1" && c.IS_TANGIBLE == "1")
                .OrderBy(c => c.ASSET_CODE)
                .Select(c => new ComboxData()
                {
                    ID = c.ASSET_ID,
                    TEXT = c.ASSET_CODE,
                    VALUE = c.ASSET_NAME,
                    EXTEND = c.ASSETNO,
                    EXTEND1 = c.TYPE_NAME,
                    EXTEND2 = c.DEPT_NAME,
                    EXTEND3 = c.CARD_USER,
                    EXTEND4 = c.PERSON,
                    EXTEND5 = c.ASSET_SIZE,
                    EXTEND6 = c.PRODUCE
                })
                .ToListAsync();
        }

        /// <summary>
        /// IT资产修复状态下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ReportState(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "reportState")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 评价下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> Appraise(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "appraise")
                .Where(predicate)
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

        private async Task<List<ComboxData>> PlanState(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "plan_state").Where(predicate)
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
        /// 维修项目分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepitemType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "repitemtype").Where(predicate)
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
        /// 设备变动类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> VaryType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "vary_type").Where(predicate)
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
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var qry = dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID);
            return await qry.Where(predicate).Where(c => c.AUDITING=="1")
                .Select(c => new ComboxData()
                {
                    ID = c.DEVICE_ID,
                    TEXT = c.DEVICE_NAME,
                    VALUE = c.DEVICE_NO,
                    EXTEND =c.STATUS,
                    EXTEND1 =c.DEVICE_TYPE,
                    EXTEND2 =c.TYPE_NAME,
                    EXTEND3 =c.DEPT_NAME,
                    EXTEND4 =c.ASSET_CODE,
                    EXTEND5 =c.INSTALL_SITE,
                    EXTEND6 =c.WDEPT_NAME,
                })
               .ToListAsync();
        }
        /// <summary>
        /// 获取船舶数据
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ShipInfo(Expression<Func<DEVICE_CARD, bool>> predicate)
        {
            //公司换成部门过滤
            using var dbContext = _dbContext.Clone();
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var qry = dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID);
            return await qry.Where(predicate).Where(c => c.AUDITING=="1"&&c.TYPE_ID=="1")
                .Select(c => new ComboxData()
                {
                    ID = c.DEVICE_ID,
                    TEXT = c.DEVICE_NAME,
                    VALUE = c.DEVICE_NO,
                    EXTEND =c.DEPT_NAME,
                    EXTEND1 =c.WDEPT_NAME,
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
        /// 企业性质下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> EnterNature(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "enterNature")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 供应商来源下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ProviderSrc(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "providerSrc")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 供应商分类下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> ProviderType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "providerType")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
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
            var corpPath = dbContext.Query<CF_CORP>().Where(predicate)
                .Select(c => c.CORP_PATH).ToList().Join();
            return await dbContext.Query<CF_CORP>()
                .Where(a => (","+a.CORP_PATH).Contains(","+corpPath))
                .Select(c => new ComboxData() { ID = c.CORPID, TEXT = c.CNAME, VALUE = c.CNO })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 仓库货位
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpHouseName(Expression<Func<SP_HOUSE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<SP_HOUSE>(c => c.AUDITING =="1").Where(predicate)
                .Select(c => new ComboxData() { ID = c.HOUSE_CODE, TEXT = c.HOUSE_NAME, VALUE = c.HOUSE_ID })
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
        /// 用户及部门下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> UserDept(Expression<Func<CF_USER, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<CF_USER>()
                .LeftJoin<CF_DEPT>((a, c) => a.DEPARTCODE==c.DEPT_CODE)
                .Select((a, c) => new ComboxData() { ID = a.USERID, TEXT = a.REALNAME, VALUE = c.DEPT_ID, EXTEND=c.DEPT_NAME })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 药品需求类别下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RequestType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "requestType")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 药品采购方式下拉框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DrugCollectMethod(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "drugCollectMethod")
                .Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .ToListAsync();
        }

        /// <summary>
        /// 处理方式
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DisposeType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "disposetype").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 设备状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DeviceStatus(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "devicestatus").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> FaultDispose(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "faultdispose").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 异常来源
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> FaultSrc(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "faultsrc").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 故障程度
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> FrdbLevel(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "frdblevel").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 故障状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> FaultStatus(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "faultstatus").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 故障分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RepType(Expression<Func<REP_TYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<REP_TYPE>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.REP_TYPE_ID, TEXT = c.REP_TYPE_NAME, VALUE = c.REP_TYPE_NAME })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 计量单位
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpUnit(Expression<Func<SP_UNIT, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<SP_UNIT>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.UNIT, TEXT = c.UNIT, VALUE = c.UNIT })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 物资分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> BaseSpType(Expression<Func<BASE_SPTYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_SPTYPE>()
            .Where(predicate)
                .Select(c => new ComboxData() { ID = c.TYPE_ID, TEXT = c.TYPE_NAME, VALUE = c.TYPE_CODE, FLAG = c.IS_CANCEL })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 采购分类
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> BasePurtype(Expression<Func<BASE_PURTYPE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_PURTYPE>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.PURTYPE_ID, TEXT = c.PURTYPE_NAME, VALUE = c.PURTYPE_CODE })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 物资信息框
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpcatalogCard(Expression<Func<BASE_SPCATALOG, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_SPCATALOG>()
                .Where(predicate)
                .OrderBy(c => c.SP_CODE)
                .Select(c => new ComboxData()
                {
                    ID = c.SP_ID,
                    TEXT = c.SP_CODE,
                    VALUE = c.SP_NAME,
                    EXTEND = c.SP_SIZE,
                    EXTEND1 = c.TYPE_NAME,
                    EXTEND2 = c.PRODUCE,
                    EXTEND3 = c.UNIT,
                    EXTEND4 =  c.STORE_NUM
                })
                .ToListAsync();
        }

        /// <summary>
        /// 低值类别
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> LowType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "LowType").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 维护类别
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> PmType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "pm_type").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 保养来源
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> BySource(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "bysource").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 保养周期
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> MaintCycle(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "maint_cycle").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 周期
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> PmcycleUnit(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "pmcycleunit").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 检查人，执行人
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> PmShippost(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "pmshippost").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 作业时设备状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> WorkState(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "work_state").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 码头信息
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> DockInfo(Expression<Func<BASE_DOCK, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BASE_DOCK>()
                .Where(predicate)
                .Select(c => new ComboxData() { ID = c.DOCK_ID, TEXT = c.DOCK_CODE, VALUE = c.DOCK_NAME, EXTEND1 = c.DOCK_ADDRESS })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 申请类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> SpapplyType(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "spapply_type").Where(predicate)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
               .ToListAsync();
        }

        /// <summary>
        /// 劳保租借状态
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private async Task<List<ComboxData>> RentState(Expression<Func<BC_CODE, bool>> predicate)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>()
                .Where(a => a.CODE_TYPE == "laborRentState")
                .Where(predicate)
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