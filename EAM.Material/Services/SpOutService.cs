using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using System.Collections.Concurrent;

namespace EAM.Material.Services
{
    public class SpOutService : ISpOutService
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

        public SpOutService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "SpapplyType",null},
                { "Auditing",null},
            });
        }

        #region 物料领用申请

        /// <summary>
        /// 获取物料领用申请记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutAppList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUT_APP>()
                .OrderBy(c => c.AUDITING_A)
                .ThenByDesc(c => c.APPLY_DATE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条物料领用申请记录
        /// </summary>
        /// <returns></returns>

        public async Task<SP_OUT_APP> GetSpOutAppListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<SP_OUT_APP>(ID);
            return qry;
        }

        /// <summary>
        /// 管理物料领用申请记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageSpOutApp(SaveRequest<SP_OUT_APP> request, SaveRequest<SP_OUTAPP_DET> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                         c => new
                         {
                             c.AUDITING_A,
                             c.APPLY_CODE,
                             c.APPLY_DATE,
                             c.PROJECT_CODE,
                             c.PROJECT_NAME,
                             c.USER_ID,
                             c.USER_NAME,
                             c.APPLY_TYPE,
                             c.SEC_DEPTID,
                             c.PURTYPE_ID,
                             c.SEC_DEPT,
                             c.PURTYPE_NAME,
                             c.SUM_MONEY,
                             c.MEMO,
                             c.OUT_ID,
                             c.DEVICE_NO,
                             c.DEVICE_NAME,
                             c.DEPT_ID,
                             c.DEPT_NAME
                         },
                         c => a => a.OUT_ID == c.OUT_ID, BeforeAdd, SpOutAppBeforUpdate, SpOutAppBeforDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_OUTAPP_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                      c => new
                      {
                          c.SP_CODE,
                          c.SP_NAME,
                          c.SP_SIZE,
                          c.DEVICE_ID,
                          c.DEVICE_NO,
                          c.APPLY_NUM,
                          c.DEVICE_NAME,
                          c.MEMO,
                          c.STOCK_ID,
                          c.STOCK_NAME,
                          c.TYPE_ID,
                          c.TYPE_NAME,
                          c.OUTDET_ID,
                          c.OUT_ID,
                          c.PRODUCE,
                          c.UNIT,
                          c.PRICE,
                          c.APPLY_MONEY,
                          c.STORE_CODE,
                          c.IN_DATE,
                          c.DEPT_NAME,
                          c.DEPT_ID,
                          c.TAX_RATE,
                          c.NOTAX_PRICE,
                          c.UNTAX_MONEY,
                      },
                      c => a => a.OUTDET_ID == c.OUTDET_ID, BeforeAddSpOutAppdet);

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

        private async Task BeforeAdd(SP_OUT_APP entity)
        {
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.APPLY_DATE = Sysdate;
            string aa = "LY" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<SP_OUT_APP>(x => x.APPLY_CODE.Contains(aa)).Select(x => Sql.Max(x.APPLY_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            entity.APPLY_CODE = aa + index.ToString("D4");
            entity.OUT_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
        }
        private async Task BeforeAddSpOutAppdet(SP_OUTAPP_DET entity)
        {
            entity.OUT_ID = _rentID;
            entity.OUTDET_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        private async Task SpOutAppBeforUpdate(SP_OUT_APP entity)
        {
            if (entity.AUDITING_A.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                _rentID = entity.OUT_ID;
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }
        private async Task SpOutAppBeforDelete(SP_OUT_APP entity)
        {
            if (entity.AUDITING_A.Equals("0"))
                await _dbContext.DeleteAsync<SP_OUTAPP_DET>(x => x.OUT_ID.Equals(entity.OUT_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }

        /// <summary>
        /// 提交物料领用申请
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitSpOutApp(string sid)
        {
            //出库单号
            string aa = "CK" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<SP_OUTSTORE>(x => x.OUT_CODE.Contains(aa)).Select(x => Sql.Max(x.OUT_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            //取物资申请表数据
            var qryoutapps = await _dbContext.Query<SP_OUT_APP>()
                 .Where(c => sid == c.OUT_ID)
                 .Select(c => new
                 {
                     c.APPLY_CODE,
                     c.APPLY_DATE,
                     c.PROJECT_CODE,
                     c.PROJECT_NAME,
                     c.USER_ID,
                     c.USER_NAME,
                     c.APPLY_TYPE,
                     c.SEC_DEPTID,
                     c.PURTYPE_ID,
                     c.SEC_DEPT,
                     c.PURTYPE_NAME,
                     c.SUM_MONEY,
                     c.MEMO,
                     c.OUT_ID,
                     c.DEVICE_NO,
                     c.DEVICE_NAME,
                     c.DEPT_ID,
                     c.DEPT_NAME
                 }).FirstOrDefaultAsync();
            //取明细表数据
            var qryoutappdets = _dbContext.Query<SP_OUTAPP_DET>()
                 .Where(c => sid == c.OUT_ID).Select(c => new
                 {
                     c.OUT_ID,
                     c.APPLY_NUM,
                     c.SP_ID,
                     c.SP_CODE,
                     c.SP_NAME,
                     c.SP_SIZE,
                     c.DEVICE_ID,
                     c.PRODUCE,
                     c.DEVICE_NO,
                     c.UNIT,
                     c.PRICE,
                     c.APPLY_MONEY,
                     c.STOCK_NAME,
                     c.STORE_CODE,
                     c.DEVICE_NAME,
                     c.IN_DATE,
                     c.DEPT_NAME,
                     c.DEPT_ID,
                     c.STOCK_ID,
                     c.TYPE_ID,
                     c.TYPE_NAME,
                     c.TAX_RATE,
                     c.NOTAX_PRICE,
                     c.UNTAX_MONEY,
                     c.MEMO,
                 }).ToList();
            if (qryoutappdets.Count == 0)
            {
                throw new MessageException("请导入明细数据！");
            }
            else
            {
                //往出库表插入数据
                var outst = new SP_OUTSTORE()
                {
                    OUT_ID = GuidHelper.NewSnowflakeId().ToString(),
                    OUT_CODE = aa + index.ToString("D4"),
                    APPLY_CODE = qryoutapps.APPLY_CODE,
                    DEPT_NAME = qryoutapps.DEPT_NAME,
                    DEPT_ID = qryoutapps.DEPT_ID,
                    SEC_DEPTID = qryoutapps.SEC_DEPTID,
                    SEC_DEPT = qryoutapps.SEC_DEPT,
                    USER_NAME = qryoutapps.USER_NAME,
                    DEVICE_NO = qryoutapps.DEVICE_NO,
                    DEVICE_NAME = qryoutapps.DEVICE_NAME,
                    SUM_MONEY = qryoutapps.SUM_MONEY,
                    MEMO = qryoutapps.MEMO,
                    IS_RED = "0",
                    CREATE_USERID = _userSession.UserID.ToString(),
                    CREATEDATE = Sysdate,
                };
                await _dbContext.InsertAsync(outst);
                var spoutstoredet = new List<SP_OUTSTORE_DET>();
                foreach (var qryoutappdet in qryoutappdets)
                {
                    var qrypcs = await _dbContext.Query<SP_STORE>(c => c.SP_ID == qryoutappdet.SP_ID)
                        .OrderBy(c => c.IN_DATE)
                        .ToListAsync();
                    //取总数量
                    var res = qryoutappdet.APPLY_NUM;
                    foreach (var qrypc in qrypcs)
                    {
                        var applyNumToUse = res < qrypc.NUM ? res : qrypc.NUM;
                        var outstdet2 = new SP_OUTSTORE_DET
                        {
                            OUTDET_ID = GuidHelper.NewSnowflakeId().ToString(),
                            OUT_ID = outst.OUT_ID,
                            SP_CODE = qryoutappdet.SP_CODE,
                            SP_NAME = qryoutappdet.SP_NAME,
                            SP_SIZE = qryoutappdet.SP_SIZE,
                            DEVICE_ID = qryoutappdet.DEVICE_ID,
                            APPLY_NUM = applyNumToUse,
                            DEVICE_NO = qryoutappdet.DEVICE_NO,
                            DEVICE_NAME = qryoutappdet.DEVICE_NAME,
                            PRODUCE = qryoutappdet.PRODUCE,
                            UNIT = qryoutappdet.UNIT,
                            PRICE = qryoutappdet.PRICE,
                            APPLY_MONEY = qryoutappdet.APPLY_MONEY,
                            STOCK_NAME = qryoutappdet.STOCK_NAME,
                            COUNT = applyNumToUse,
                            STORE_CODE = qrypc.STORE_CODE,
                            IN_DATE = qryoutappdet.IN_DATE,
                            DEPT_NAME = qryoutappdet.DEPT_NAME,
                            DEPT_ID = qryoutappdet.DEPT_ID,
                            MONEY = qryoutappdet.APPLY_MONEY * applyNumToUse,
                            STOCK_ID = qryoutappdet.STOCK_ID,
                            TYPE_ID = qryoutappdet.TYPE_ID,
                            TYPE_NAME = qryoutappdet.TYPE_NAME,
                            TAX_RATE = qryoutappdet.TAX_RATE,
                            NOTAX_PRICE = qryoutappdet.NOTAX_PRICE,
                            UNTAX_MONEY = qryoutappdet.UNTAX_MONEY,
                            MEMO = qryoutappdet.MEMO,
                            CREATE_USERID = _userSession.UserID.ToString(),
                            CREATEDATE = Sysdate,
                        };

                        spoutstoredet.Add(outstdet2);

                        if (applyNumToUse == res)
                        {
                            break;
                        }

                        res -= applyNumToUse;
                    }
                }
                await _dbContext.InsertRangeAsync(spoutstoredet);
                await _dbContext.UpdateAsync<SP_OUT_APP>(x => sid == x.OUT_ID,
                      x => new SP_OUT_APP
                      {
                          AUDITING_A = "1",
                      });
            }
            return 1;
        }

        /// <summary>
        /// 获取物料领用申请明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutAppdetList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTAPP_DET>()
                .GetGridData(request);
        }

        #endregion 物料领用申请
    }
}