using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.CodeAnalysis;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using WkHtmlToPdfDotNet;

namespace EAM.Material.Services
{
    public class SpOutService : ISpOutService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;
        private string _rentID = string.Empty, errMsg = string.Empty;
        private string _outID = string.Empty, errMsg2 = string.Empty;
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
        /// 导入物料功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportSpList(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>(c => c.NUM>0)
                .GroupBy(t => new
                {
                    t.SP_ID,
                    t.STOCK_ID,
                    t.SP_SIZE,
                    t.UNIT,
                    t.SP_NAME,
                    t.STOCK_NAME,
                    t.SP_CODE,
                })
                .Select(c => new
                {
                    c.SP_NAME,
                    c.SP_ID,
                    c.SP_SIZE,
                    c.UNIT,
                    c.SP_CODE,
                    c.STOCK_NAME,
                    NUM = Sql.Sum(c.NUM),
                    MONEY = Sql.Sum(c.MONEY),
                    TAX_MONEY = Sql.Sum(c.TAX_MONEY),
                    NOTAX_MONEY = Sql.Sum(c.NOTAX_MONEY),
                })
                .GetGridData(request);
        }

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
                    requestdet ??= new SaveRequest<SP_OUTAPP_DET>();

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
                          c.NOTAX_MONEY,
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
                     c.NOTAX_MONEY,
                     c.MEMO,
                     c.STORE_ID,
                 }).ToList();
            if (qryoutappdets.Count == 0)
            {
                throw new MessageException("请导入明细数据！");
            }
            else
            {
                //往出库表插入数据
                var outst = qryoutapps.MapTo<SP_OUTSTORE>();

                outst.OUT_ID = GuidHelper.NewSnowflakeId().ToString();
                outst.OUT_CODE = aa + index.ToString("D4");
                outst.IS_RED = "0";
                outst.AUDITING_A = "0";
                outst.CREATE_USERID = _userSession.UserID.ToString();
                outst.CREATEDATE = Sysdate;
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
                        var outstdet2 = qryoutappdet.MapTo<SP_OUTSTORE_DET>();
                        var outstdet3 = qrypc.MapTo(outstdet2);

                        outstdet3.OUTDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        outstdet3.OUT_ID = outst.OUT_ID;
                        outstdet3.MONEY = qrypc.PRICE * applyNumToUse;
                        outstdet3.STORE_NUM = qrypc.NUM;
                        outstdet3.NOTAX_MONEY = qrypc.NOTAX_PRICE * qrypc.NOTAX_MONEY;
                        outstdet3.APPLY_NUM = applyNumToUse;
                        outstdet3.COUNT = applyNumToUse;
                        //outstdet2.IN_DATE = qryoutappdet.IN_DATE;
                        outstdet3.APPLY_MONEY = qrypc.PRICE * applyNumToUse;
                        outstdet3.CREATE_USERID = _userSession.UserID.ToString();
                        outstdet3.CREATEDATE = Sysdate;

                        spoutstoredet.Add(outstdet3);

                        if (applyNumToUse == res)
                        {
                            break;
                        }

                        res -= applyNumToUse;
                    }
                }
                await _dbContext.InsertRangeAsync(spoutstoredet);
                return await _dbContext.UpdateAsync<SP_OUT_APP>(x => sid == x.OUT_ID,
                      x => new SP_OUT_APP
                      {
                          AUDITING_A = "1",
                      });
            }
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

        #region 物料领用出库

        /// <summary>
        /// 获取物料领用出库记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutStoreList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE>()
                .OrderBy(c => c.AUDITING_A)
                .ThenByDesc(c => c.OUT_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条物料领用出库记录
        /// </summary>
        /// <returns></returns>

        public async Task<SP_OUTSTORE> GetSpOutStoreListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<SP_OUTSTORE>(ID);
            return qry;
        }

        /// <summary>
        /// 管理物料领用出库记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageSpOutStore(SaveRequest<SP_OUTSTORE> request, SaveRequest<SP_OUTSTORE_DET> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                         c => new
                         {
                             c.OUT_DATE,
                             c.M_USERID,
                             c.M_USER,
                             c.MEMO,
                             c.OUT_ID,
                         },
                         c => a => a.OUT_ID == c.OUT_ID, null, SpOutStoreBeforUpdate);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet ??= new SaveRequest<SP_OUTSTORE_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                      c => new
                      {
                          c.SP_CODE,
                          c.COUNT,
                          c.OUTDET_ID,
                      },
                      c => a => a.OUTDET_ID == c.OUTDET_ID, BeforeAddSpOutStoredet);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (mainSuccess && detSuccess)
                    trans.Commit();
                else
                {
                    trans.Rollback();
                    if (string.IsNullOrWhiteSpace(errMsg2)) errMsg2 = "保存失败";
                    return AjaxResult.Error(errMsg2);
                }
            }
            return AjaxResult.Success("保存成功");
        }

        private async Task BeforeAddSpOutStoredet(SP_OUTSTORE_DET entity)
        {
            entity.OUT_ID = _outID;
            entity.OUTDET_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        private async Task SpOutStoreBeforUpdate(SP_OUTSTORE entity)
        {
            if (entity.AUDITING_A.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                _outID = entity.OUT_ID;
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg2 = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }

        /// <summary>
        /// 提交物料领用出库
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitSpOutStore(string sid)
        {
            var qry = await _dbContext.Query<SP_OUTSTORE>()
                    .Where(c => c.OUT_ID==sid)
                    .Select(c => new
                    {
                        c.OUT_DATE,
                    }).FirstAsync();
            if (qry.OUT_DATE == null)
            {
                throw new MessageException("核对出库日期是否选择！");
            }
            //获取出库明细对应的数据
            var qryoutstdets = await _dbContext.Query<SP_OUTSTORE_DET>()
                 .Where(c => sid == c.OUT_ID)
                 .Select(c => new
                 {
                     c.COUNT,
                     c.STORE_CODE,
                     c.STORE_ID,
                 })
                 .ToListAsync();
            //获取出库明细对应的数据
            var qryoutst = await _dbContext.Query<SP_OUTSTORE>()
                 .Where(c => sid == c.OUT_ID)
                 .Select(c => new
                 {
                     c.OUT_CODE,
                 }).FirstOrDefaultAsync();
            //获取出库明细库存id
            var outstoreDetIds = qryoutstdets.Select(q => q.STORE_ID).ToList();
            //获取出库明细的库存数据
            var qryStores = await _dbContext.Query<SP_STORE>()
                .Where(s => outstoreDetIds.Contains(s.STORE_ID))
                .ToListAsync();
            var stwater = new List<STORE_WATER>();
            using var transaction = _dbContext.BeginTransaction();

            try
            {
                foreach (var qryoutstdet in qryoutstdets)
                {
                    //获取库存数据
                    var qrystore = qryStores.FirstOrDefault(s =>
                        s.STORE_ID == qryoutstdet.STORE_ID && qryoutstdet.STORE_CODE == s.STORE_CODE);
                    if (qrystore.NUM < qryoutstdet.COUNT)
                    {
                        throw new MessageException("当前批次库存已经没这么多，请重新选择出库数量！");
                    }
                    if (qrystore != null)
                    {
                        //期初库存
                        var bnum = qrystore.NUM;
                        //期初库存
                        var bmoney = qrystore.TAX_MONEY;
                        //剩余库存数量
                        var surnum = qrystore.NUM - qryoutstdet.COUNT;
                        //剩余库存金额
                        var surmoney = surnum * qrystore.PRICE;
                        //剩余不含税金额
                        var surnomoney = surnum * qrystore.NOTAX_PRICE;
                        //更新库存表
                        var updatedevice = await _dbContext.UpdateAsync<SP_STORE>(x => x.STORE_ID == qryoutstdet.STORE_ID,
                             x => new SP_STORE
                             {
                                 NUM = surnum,
                                 MONEY = surmoney,
                                 TAX_MONEY = surmoney,
                                 NOTAX_MONEY = surnomoney,
                             });

                        //往流水表插数据
                        var waterdata = qryoutstdet.MapTo<STORE_WATER>();
                        waterdata.SRC_CODE = qryoutst.OUT_CODE;
                        waterdata.SRC_TYPE = "3";
                        waterdata.INIT_NUM = bnum;
                        waterdata.INIT_MONEY = bmoney;
                        waterdata.IN_NUM = 0;
                        waterdata.IN_PRICE = 0;
                        waterdata.IN_MONEY = 0;
                        waterdata.OUT_NUM = qryoutstdet.COUNT;
                        waterdata.OUT_MONEY = qrystore.PRICE;
                        waterdata.IN_MONEY = qryoutstdet.COUNT * qrystore.PRICE;
                        waterdata.CUR_NUM = surnum;
                        waterdata.CUR_MONEY = surmoney;

                        waterdata.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                        waterdata.CREATE_USERID = _userSession.UserID.ToString();
                        waterdata.CREATEDATE = Sysdate;
                        waterdata.WATER_DATE = Sysdate;
                        stwater.Add(waterdata);
                    }
                }
                await _dbContext.InsertRangeAsync(stwater);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return await _dbContext.UpdateAsync<SP_OUTSTORE>(x => sid == x.OUT_ID,
                      x => new SP_OUTSTORE
                      {
                          AUDITING_A = "1",
                      });
        }

        /// <summary>
        /// 注销物料领用出库
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitSpOutStore(string sid)
        {
            return await _dbContext.UpdateAsync<SP_OUTSTORE>(x => sid == x.OUT_ID,
                      x => new SP_OUTSTORE
                      {
                          AUDITING_A = "7",
                      });
        }

        /// <summary>
        /// 获取物料领用出库明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutStoredetList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE_DET>()
                .GetGridData(request);
        }

        #endregion 物料领用出库


    }
}