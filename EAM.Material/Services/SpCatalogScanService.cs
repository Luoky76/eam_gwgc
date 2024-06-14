using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Material.Services
{
    public class SpCatalogScanService : ISpCatalogScanService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;
        private string _rentID = string.Empty, errMsg = string.Empty;
        /// <summary>
        /// 获取数据库时间
        /// </summary>
        private DateTime? Sysdate
        {
            get
            {
                if (!_Sysdate.HasValue)
                {
                    _Sysdate = _dbContext.GetSysdate().Result();
                }
                return _Sysdate;
            }
        }

        public SpCatalogScanService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxService.Get(new Dictionary<string, object>(){
                { "Auditing",null}
            });
        }


        /// <summary>
        /// 导入物料列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportSpList(GridRequest request)
        {
            return await _dbContext.Query<BASE_SPCATALOG>().GetGridData(request);
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_CATALOG_SCAN>()
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.SCAN_DATE)
                .GetGridData(request);
        }

        /// <summary>
        /// 管理申请记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_CATALOG_SCAN> request, SaveRequest<SP_CATALOG_SCAN_DET> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                         c => new
                         {
                             c.SCAN_ID,
                             c.AUDITING,
                             c.SCAN_TYPE,
                             c.SCAN_USER,
                             c.SCAN_USERID,
                             c.DEPT_NAME,
                             c.DEPT_ID,
                             c.SCAN_DATE,
                             c.MEMO,
                             c.CREATE_USERID,
                             c.CREATEDATE,
                             c.MODIFY_USERID,
                             c.MODIFYDATE,
                             c.SCAN_CODE,
                             c.SEC_DEPTID,
                             c.SEC_DEPT
                         },
                         c => a => a.SCAN_ID == c.SCAN_ID, BeforeAdd, BeforUpdate, BeforDelete);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet ??= new SaveRequest<SP_CATALOG_SCAN_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                      c => new
                      {
                          c.AUDITING,
                          c.SP_CODE,
                          c.SP_NAME,
                          c.SP_SIZE,
                          c.OTHER_CODE,
                          c.PRODUCE,
                          c.UNIT,
                          c.FACTORY,
                          c.STORE_NUM,
                          c.SCAN_NUM,
                          c.HOUSE_CODE,
                          c.HOUSE_NAME,
                          c.HOUSE_ID,
                          c.STOCK_NAME,
                          c.STOCK_CODE,
                          c.STOCK_ID,
                          c.TYPE_CODE,
                          c.TYPE_NAME,
                          c.TYPE_ID,
                          c.MEMO,
                          c.SCAN_DET_ID,
                          c.SCAN_ID,
                          c.SP_ID,
                          c.STORE_ID,
                          c.CREATE_USERID,
                          c.CREATEDATE,
                          c.MODIFY_USERID,
                          c.MODIFYDATE
                      },
                      c => a => a.SCAN_DET_ID == c.SCAN_DET_ID, BeforeAddDet);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (mainSuccess && detSuccess)
                    trans.Commit();
                else
                {
                    trans.Rollback();
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    return AjaxResult.Error(errMsg);
                }
            }
            return AjaxResult.Success("保存成功");
        }

        private async Task BeforeAdd(SP_CATALOG_SCAN entity)
        {
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.SCAN_DATE = Sysdate;
            string aa = "PD" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<SP_CATALOG_SCAN>(x => x.SCAN_CODE.Contains(aa)).Select(x => Sql.Max(x.SCAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.SCAN_CODE = aa + index.ToString("D4");
            entity.SCAN_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
            entity.AUDITING = "0";
        }
        private async Task BeforeAddDet(SP_CATALOG_SCAN_DET entity)
        {
            entity.SCAN_ID = entity.SCAN_ID ?? _rentID;
            entity.SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.AUDITING = "0";
            await Task.CompletedTask;
        }

        private async Task BeforUpdate(SP_CATALOG_SCAN entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            if (entity.AUDITING.Equals("0"))
            {
                _rentID = entity.SCAN_ID;
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
                if (entity.AUDITING.Equals("1"))
                {
                    var detquery = await _dbContext.Query<SP_CATALOG_SCAN_DET>().Where(t => t.SCAN_ID == entity.SCAN_ID).ToListAsync();
                    foreach (var result in detquery)
                    {
                        if (string.IsNullOrEmpty(result.AUDITING) || result.AUDITING == "0")
                        {
                            var orderList = _dbContext.Query<SP_STORE>(x => x.SP_CODE == result.SP_CODE && x.DEPT_ID == entity.DEPT_ID).OrderByDesc(c => c.CREATEDATE).ToList();
                            //修改库存记录:第一条更正，后面的记录直接设置0;若没有记录添加一条数据
                            if (orderList.Count > 0)
                            {
                                var count = result.SCAN_NUM;
                                foreach (var detail in orderList)
                                {
                                    if (count > 0)
                                    {
                                        result.STORE_ID = detail.STORE_ID;
                                        detail.NUM = count;
                                        count = 0;
                                    }
                                    else
                                    {
                                        detail.NUM = 0;
                                    }

                                    await _dbContext.UpdateAsync(detail);
                                }
                            }
                            else
                            {
                                SP_STORE _STORE = new()
                                {
                                    SRC_TYPE = "5",
                                    IS_BACK = "0",
                                    SP_CODE = result.SP_CODE,
                                    SP_ID = result.SP_ID,
                                    SP_NAME = result.SP_NAME,
                                    SP_SIZE = result.SP_SIZE,
                                    PRODUCE = result.PRODUCE,
                                    TYPE_CODE = result.TYPE_CODE,
                                    TYPE_NAME = result.TYPE_NAME,
                                    TYPE_ID = result.TYPE_ID,
                                    STOCK_NAME = result.STOCK_NAME,
                                    UNIT = result.UNIT,
                                    NUM = result.SCAN_NUM,
                                    STORE_ID = GuidHelper.NewSnowflakeId().ToString(),
                                    DEPT_ID = entity.DEPT_ID,
                                    DEPT_NAME = entity.DEPT_NAME,
                                    CREATE_USERID = _userSession.UserID.ToString(),
                                    CREATEDATE = DateTime.Now,
                                    MODIFY_USERID = _userSession.UserID.ToString(),
                                    MODIFYDATE = DateTime.Now
                                };//库存表

                                string type = "PC" + DateTime.Now.ToString("yyyyMM");
                                string def = type + "0000";
                                var model = await _dbContext.Query<SP_STORE>(x => x.STORE_CODE.Contains(type)).Select(x => Sql.Max(x.STORE_CODE) ?? def).FirstOrDefaultAsync();
                                var index = model.SubStr(8, 4).CastTo<int>() + 1;
                                _STORE.STORE_CODE = type + index.ToString("D4");

                                result.STORE_ID = _STORE.STORE_ID;
                                await _dbContext.InsertAsync(_STORE);

                            }
                            result.AUDITING = "1"; //修正数据后确认处理
                            await _dbContext.UpdateAsync(result);
                        }

                    }

                }
            }
        }
        private async Task BeforDelete(SP_CATALOG_SCAN entity)
        {
            if (entity.AUDITING.Equals("0"))
                await _dbContext.DeleteAsync<SP_CATALOG_SCAN_DET>(x => x.SCAN_ID.Equals(entity.SCAN_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }

        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_CATALOG_SCAN_DET>().GetGridData(request);
        }

    }
}