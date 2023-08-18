using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;
using System.Linq;

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
        private Dictionary<string, string> outDic = new();
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
                { "BCCode", "purtypeName" },
            });
        }

        #region 物料领用申请

        /// <summary>
        /// 导入物料功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportSpList(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                .Where(c => c.NUM > 0 && c.SP_NAME != null && c.SP_CODE != null)
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
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
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
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
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
                outst.OUT_DATE = Sysdate;
                outst.IS_RED = "0";
                outst.AUDITING_A = "0";
                outst.CREATE_USERID = _userSession.UserID.ToString();
                outst.CREATEDATE = Sysdate;
                await _dbContext.InsertAsync(outst);

                var spoutstoredet = new List<SP_OUTSTORE_DET>();
                //申请金额
                var appmoney = 0m;
                foreach (var qryoutappdet in qryoutappdets)
                {
                    var qrypcs = await _dbContext.Query<SP_STORE>(c=>c.NUM>0)
                        .Where(c=>c.SP_CODE==qryoutappdet.SP_CODE&&c.SP_ID == qryoutappdet.SP_ID&&c.STOCK_ID == qryoutappdet.STOCK_ID
                             &&c.SP_SIZE == qryoutappdet.SP_SIZE&&c.UNIT == qryoutappdet.UNIT&&c.SP_NAME == qryoutappdet.SP_NAME&&c.STOCK_NAME == qryoutappdet.STOCK_NAME)
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
                        if (outstdet3.MONEY.HasValue)
                        {
                            appmoney += outstdet3.MONEY.Value;
                        }
                        spoutstoredet.Add(outstdet3);

                        if (applyNumToUse == res)
                        {
                            break;
                        }

                        res -= applyNumToUse;
                    }
                }
                await _dbContext.UpdateAsync<SP_OUTSTORE>(x => outst.OUT_ID == x.OUT_ID,
                  x => new SP_OUTSTORE
                  {
                      SUM_MONEY = appmoney,
                  });
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
                     c.WATER_ID,
                 })
                 .ToListAsync();
            //获取出库对应的数据
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
                        s.STORE_ID == qryoutstdet.STORE_ID);
                    if (qrystore.NUM < qryoutstdet.COUNT)
                    {
                        throw new MessageException("当前批次库存已经没这么多，请重新选择出库数量！");
                    }
                    if (qrystore != null)
                    {
                        //期初库存数量
                        var bnum = qrystore.NUM;
                        //期初库存金额
                        var bmoney = qrystore.TAX_MONEY;
                        //剩余库存数量
                        var surnum = qrystore.NUM - qryoutstdet.COUNT;
                        //剩余库存金额
                        var surmoney = surnum * qrystore.PRICE;
                        //剩余不含税金额
                        var surnomoney = surnum * qrystore.NOTAX_PRICE;
                        //更新库存表
                        var updatespstore = await _dbContext.UpdateAsync<SP_STORE>(x => x.STORE_ID == qryoutstdet.STORE_ID,
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
                        waterdata.IS_BACK = "0";

                        waterdata.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                        waterdata.CREATE_USERID = _userSession.UserID.ToString();
                        waterdata.CREATEDATE = Sysdate;
                        waterdata.WATER_DATE = Sysdate;
                        stwater.Add(waterdata);

                        await _dbContext.UpdateAsync<SP_OUTSTORE_DET>(x => sid == x.OUT_ID,
                                  x => new SP_OUTSTORE_DET
                                  {
                                      WATER_ID = waterdata.WATER_ID,
                                  });
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

        #region 物料出库冲红

        /// <summary>
        /// 获取冲红记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutBackList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUT_BACK>()
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.BACK_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条冲红记录
        /// </summary>
        /// <returns></returns>

        public async Task<SP_OUT_BACK> GetSpOutBackListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<SP_OUT_BACK>(ID);
            return qry;
        }

        /// <summary>
        /// 管理导入冲红记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageSpOutBack(List<SP_OUTSTORE> request)
        {
            using var trans = _dbContext.BeginTransaction(); // 事务保证保存数据的一致性
            try
            {
                var outlist = request.Select(x => x.OUT_ID).ToList();
                var outBackDets = _dbContext.Query<SP_OUTSTORE>().ToList();
                var request2 = outBackDets
                .Where(x => outlist.Contains(x.OUT_ID))
                .Select(x =>
                {
                    return x.MapTo<SP_OUT_BACK>();
                })
                .ToList();

                int index = 0; 
                foreach (var request1 in request2)
                {
                    request1.BACK_DATE = Sysdate;
                    string aa = "CH" + DateTime.Now.ToString("yyyyMM");
                    string def = aa + "0000";
                    var model = await _dbContext.Query<SP_OUT_BACK>(x => x.BACK_CODE.Contains(aa))
                        .Select(x => Sql.Max(x.BACK_CODE) ?? def)
                        .FirstOrDefaultAsync();

                    index++; // 增加 index
                    request1.BACK_CODE = aa + (model.SubStr(8, 4).CastTo<int>() + index).ToString("D4");
                    request1.OUT_BACK_ID = GuidHelper.NewSnowflakeId().ToString();
                    outDic[request1.OUT_ID] =  request1.OUT_BACK_ID;
                }
                await _dbContext.InsertRangeAsync(request2);
                if (request2.Count > 0)
                {
                    var keylist = request2.Select(x => x.OUT_ID).ToList();
                    var outstoreDets = _dbContext.Query<SP_OUTSTORE_DET>().ToList();

                    var spoutbackdets = outstoreDets
                        .Where(x => keylist.Contains(x.OUT_ID))
                        .Select(x =>
                        {
                            return x.MapTo<SP_OUTBACK_DET>();
                        })
                        .ToList();
                    foreach (var spoutbackdet in spoutbackdets)
                    {
                        spoutbackdet.OUTDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        spoutbackdet.OUT_BACK_ID = outDic[spoutbackdet.OUT_ID];
                    }
                    await _dbContext.InsertRangeAsync(spoutbackdets); // 插入明细数据
                }

                trans.Commit();
                return AjaxResult.Success("保存成功");
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return AjaxResult.Error("保存失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitSpOutBack(string sid)
        {
            //查冲红的出库单号
            var qryback = await _dbContext.Query<SP_OUT_BACK>(c => c.OUT_BACK_ID == sid)
                .FirstOrDefaultAsync();
            //查明细id的数据
            var qrybackdets = await _dbContext.Query<SP_OUTBACK_DET>(c => c.OUT_BACK_ID == sid)
                .ToListAsync();
            //获取明细库存id
            var backstoreDetIds = qrybackdets.Select(q => q.STORE_ID).ToList();
            //获取出库明细的库存数据
            var qryStores = await _dbContext.Query<SP_STORE>()
                .Where(s => backstoreDetIds.Contains(s.STORE_ID))
                .ToListAsync();
            //获取流水表的库存数据
            var qryWaters = await _dbContext.Query<STORE_WATER>()
                .Where(s => backstoreDetIds.Contains(s.STORE_ID))
                .ToListAsync();
            foreach (var qrybackdet in qrybackdets)
            {
                //获取库存数据
                var qrystore = qryStores.FirstOrDefault(s =>
                    s.STORE_ID == qrybackdet.STORE_ID);
                if (qrystore != null)
                {
                    //期初库存数量
                    var bnum = qrystore.NUM;
                    //期初库存金额
                    var bmoney = qrystore.TAX_MONEY;
                    //冲红库存数量
                    var chnum = qrystore.NUM + qrybackdet.COUNT;
                    //冲红库存金额
                    var chmoney = chnum * qrystore.PRICE;
                    //冲红不含税金额
                    var chnomoney = chnum * qrystore.NOTAX_PRICE;
                    //更新库存表
                    var updatespstore = await _dbContext.UpdateAsync<SP_STORE>(x => x.STORE_ID == qrybackdet.STORE_ID,
                         x => new SP_STORE
                         {
                             NUM = chnum,
                             MONEY = chmoney,
                             TAX_MONEY = chmoney,
                             NOTAX_MONEY = chnomoney,
                         });

                    //获取流水数据
                    var qryWater = qryWaters.FirstOrDefault(s =>
                        s.STORE_ID == qrybackdet.STORE_ID && s.SRC_CODE == qryback.OUT_CODE);
                    if (qryWater != null)
                    {
                        //更新出库表
                        var updatespout = await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_CODE == qryback.OUT_CODE,
                             x => new SP_OUTSTORE
                             {
                                 IS_RED = "1",
                             });
                        //更新流水表
                        var updatespwater = await _dbContext.UpdateAsync<STORE_WATER>(x => x.WATER_ID == qryWater.WATER_ID,
                             x => new STORE_WATER
                             {
                                 IS_BACK = "1",
                             });
                    }
                }
            }

            return await _dbContext.UpdateAsync<SP_OUT_BACK>(x => sid == x.OUT_BACK_ID,
                      x => new SP_OUT_BACK
                      {
                          AUDITING = "1",
                      });
        }
        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE>()
                .Where(c => c.AUDITING_A =="1")
                .GetGridData(request);
        }

        /// <summary>
        /// 保存冲红
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveSpBack(SaveRequest<SP_OUT_BACK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.BACK_DATE,
                    c.MEMO,
                    c.OUT_BACK_ID,
                },
                c => a => a.OUT_BACK_ID == c.OUT_BACK_ID);
        }

        #endregion
        /// <summary>
        /// 获取物料出库明细记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutStoreDetailList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE_DET>()
                .InnerJoin<SP_OUTSTORE>((a, b) => a.OUT_ID == b.OUT_ID)
                .Where((a, b) => b.AUDITING_A=="1")
                .Select((a, b) => new
                {
                    b.OUT_CODE,
                    b.APPLY_CODE,
                    b.OUT_DATE,
                    b.USER_NAME,
                    b.DEPT_NAME,
                    b.M_USER,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.TYPE_CODE,
                    a.TYPE_NAME,
                    a.APPLY_NUM,
                    a.COUNT,
                    a.PRICE,
                    a.DEVICE_NO,
                    a.MONEY,
                    a.HOUSE_NAME,
                    a.STOCK_NAME,
                    a.STORE_CODE,
                    a.IN_DATE,
                    a.IS_RECOVERY,
                    b.PROJECT_CODE,
                    b.PROJECT_NAME,
                    a.TAX_RATE,
                    a.NOTAX_PRICE,
                    a.NOTAX_MONEY,
                    b.SEC_DEPT,
                    a.MEMO,
                    a.STORE_ID,
                    a.OUTDET_ID,
                    a.OUT_ID,
                    a.DEPT_ID,
                })
               .GetGridData(request);
        }
        /// <summary>
        /// 获取物料冲红明细记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetSpOutBackDetailList(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTBACK_DET>()
                .GetGridData(request);
        }
    }
}