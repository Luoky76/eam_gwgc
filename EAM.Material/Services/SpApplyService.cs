using EAM.Material.DTO;
using EAM.Material.Interfaces;
using Gksyb.Common.Office;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpApplyService : BaseService, ISpApplyService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private string _rentID = string.Empty, errMsg = string.Empty;

        public SpApplyService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        #region 采购申请
        class SpApplyRes : SP_APPLY
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public int DETAILCOUNT;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_APPLY>()
                .Select(c => new SpApplyRes
                {
                    APPLY_ID = c.APPLY_ID,
                    AUDITING = c.AUDITING,
                    AUDITING_CHECK = c.AUDITING_CHECK,
                    APPLY_NO = c.APPLY_NO,
                    TYPE_ID = c.TYPE_ID,
                    USE_MEMO = c.USE_MEMO,
                    EXIG_DEV = c.EXIG_DEV,
                    APPLY_USER = c.APPLY_USER,
                    DEPT_ID = c.DEPT_ID,
                    DEPT_NAME = c.DEPT_NAME,
                    SEC_DEPTID = c.SEC_DEPTID,
                    SEC_DEPT = c.SEC_DEPT,
                    APPLY_DATE = c.APPLY_DATE,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    TYPE_ID2 = c.TYPE_ID2,
                    CGFS = c.CGFS,
                    TYPE_CODE = c.TYPE_CODE,
                    TYPE_NAME = c.TYPE_NAME,
                    MEMO = c.MEMO
                })
                .GetGridData(request);
            foreach (var item in (List<SpApplyRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_APPLY_DETAIL>().Where(t => t.APPLY_ID == item.APPLY_ID).Count();
            }
            return res;
        }


        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "exig_dev" },
                    { "BasePurtype", (Expression<Func<BASE_PURTYPE, bool>>)null},
                    { "SpUnit", (Expression<Func<SP_UNIT, bool>>)null},
                });
                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "CGtype" }
                });
                dic.TryAdd("CGFS", dic1["BCCode"]);
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        public async Task<SP_APPLY> GetApplyDetail(string ID)
        {
            return await _dbContext.QueryByKeyAsync<SP_APPLY>(ID);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestdet"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_APPLY> request, SaveRequest<SP_APPLY_DETAIL> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                         c => new
                         {
                             c.AUDITING,
                             c.APPLY_NO,
                             c.APPLY_DATE,
                             c.APPLY_USERID,
                             c.APPLY_USER,
                             c.DEPT_ID,
                             c.DEPT_CODE,
                             c.DEPT_NAME,
                             c.IS_REC,
                             c.TIME_REQ,
                             c.SOURCE_ID,
                             c.SOURCE,
                             c.USE_MEMO,
                             c.TYPE_ID,
                             c.TYPE_CODE,
                             c.TYPE_NAME,
                             c.EXIG_DEV,
                             c.PROJECT_CODE,
                             c.OA_CHECK,
                             c.OA_DATE,
                             c.OA_MEMO,
                             c.SEC_DEPTID,
                             c.REQUEST_ID,
                             c.SEC_DEPT,
                             c.MEMO,
                             c.APPLY_ID,
                             c.IS_GEN,
                             c.CREATE_USERID,
                             c.CREATEDATE,
                             c.MODIFY_USERID,
                             c.MODIFYDATE,
                             c.SUM_MONEY,
                             c.CGFS,
                             c.TYPE_ID2,
                             c.SSZT,
                             c.SSZTID,
                             c.BD_NAME
                         },
                          c => a => a.APPLY_ID == c.APPLY_ID, BeforeAdd, BeforeUpdate, BeforeDelete);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet ??= new SaveRequest<SP_APPLY_DETAIL>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                      c => new
                      {
                          c.SPDET_ID,
                          c.APPLY_ID,
                          c.SP_ID,
                          c.SP_CODE,
                          c.SP_NAME,
                          c.SP_SIZE,
                          c.PRODUCE,
                          c.UNIT,
                          c.COUNT,
                          c.STORE_NUM,
                          c.YG_PRICE,
                          c.YG_MONEY,
                          c.LAST_PROVIDERID,
                          c.LAST_PROVIDER,
                          c.TYPE_ID,
                          c.TYPE_CODE,
                          c.TYPE_NAME,
                          c.IS_STOP,
                          c.MEMO,
                          c.IS_GEN,
                          c.CREATE_USERID,
                          c.CREATEDATE,
                          c.MODIFY_USERID,
                          c.MODIFYDATE,
                          c.TENANT_ID,
                          c.PURTYPE_ID,
                          c.PURTYPE_NAME,
                          c.IS_CANCEL,
                          c.NO_PRODUCE,
                          c.COMP_CODE,
                          c.STORE_MONTH,
                          c.PUR_PERIOD,
                          c.ONROAD_NUM,
                          c.PRO_ID,
                          c.PRO_DET_ID,
                          c.PERIOD,
                          c.IS_XY,
                          c.WARRANTY,
                          c.DELIVERY_CODE,
                          c.XHZQ,
                          c.SYDD,
                          c.CGFS,
                          c.SYDDDEPTID,
                          c.COUNT2,
                          c.CGFS2,
                          c.SP_CODE2,
                          c.COMP_CODE2,
                          c.SP_NAME2,
                          c.SYDD2,
                          c.SYDDDEPTID2,
                          c.SP_SIZE2,
                          c.PRODUCE2,
                          c.UNIT2,
                          c.ZKCS,
                          c.QYKCSL
                      },
                     c => a => a.SPDET_ID == c.SPDET_ID, BeforeAddDet, BeforeUpdateDet, null, false, null, AfterSaveDet);

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

        private async Task BeforeAdd(SP_APPLY entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.APPLY_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();

            string type = $"SQ{DateTime.Now.ToString("yyyyMM")}";
            string def = type + "0000";
            var model = await _dbContext.Query<SP_APPLY>(x => x.APPLY_NO.Contains(type)).Select(x => Sql.Max(x.APPLY_NO) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.APPLY_NO = type + index.ToString("D4");

            entity.APPLY_DATE = dt;
            entity.APPLY_USERID = _userSession.UserID.ToString();
            entity.APPLY_USER = _userSession.RealName;
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.AUDITING = "0";
            entity.AUDITING_CHECK = "0";
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_APPLY entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            _rentID = entity.APPLY_ID;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }
        private async Task BeforeDelete(SP_APPLY entity)
        {
            await _dbContext.DeleteAsync<SP_APPLY_DETAIL>(x => x.APPLY_ID == entity.APPLY_ID);
        }

        /// <summary>
        /// 申请提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                    x => new SP_APPLY
                    {
                        AUDITING = "1"
                    });

            return updatedevice;
        }

        /// <summary>
        /// 申请提交撤销
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AjaxResult> CancelSubmit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                     x => new SP_APPLY
                     {
                         AUDITING = "0"
                     });
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 确认提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> CheckSubmit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                    x => new SP_APPLY
                    {
                        AUDITING_CHECK = "1"
                    });
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.APPLY_ID),
                   x => new SP_APPLY_DETAIL
                   {
                       SP_STATUS = "20"//待请购
                   });

            return updatedevice;
        }

        /// <summary>
        /// 确认提交撤销
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AjaxResult> CheckCancelSubmit(List<string> sids)
        {
            var list = _dbContext.Query<SP_APPLY>().Where(x => sids.Contains(x.APPLY_ID)).ToList();

            if (list.Count > 0)
            {
                //状态
                var dic = _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "pur_state").ToList();
                foreach (var item in list)
                {
                    var detStatus = _dbContext.Query<SP_APPLY_DETAIL>().Where(t => t.APPLY_ID == item.APPLY_ID && t.SP_STATUS != "20").Select(t => t.SP_STATUS).FirstOrDefault();
                    if (!string.IsNullOrEmpty(detStatus))
                    {
                        throw new Exception($"{item.APPLY_NO}{(dic.Where(t => t.CODE_EN == detStatus).FirstOrDefault()?.CODE_CN)},不能撤销!");
                    }
                }
                var updatedevice = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                     x => new SP_APPLY
                     {
                         AUDITING_CHECK = "0"
                     });

                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.APPLY_ID),
                       x => new SP_APPLY_DETAIL
                       {
                           SP_STATUS = "10"//计划APPLY_ID
                       });
            }
            return AjaxResult.Success("成功");
        }

        public async Task<AjaxResult> ImportInDetail([FileOptions("xlsx,xls")] IFormFile formFile, string folder, string sid)
        {
            var apply = string.IsNullOrEmpty(sid) ? new SP_APPLY { AUDITING = "0" } : _dbContext.QueryByKey<SP_APPLY>(sid);
            if (string.IsNullOrEmpty(sid))
            {
                await BeforeAdd(apply);
                _dbContext.Insert(apply);
            }

            var importResult = new List<SP_APPLY_DETAIL>();

            try
            {
                DateTime? dt = await _dbContext.GetSysdate();
                var type = _dbContext.Query<BASE_SPTYPE>().Where(t => t.TYPE_NAME == "临时类别").FirstOrDefault();
                var typeCount = _dbContext.Query<BASE_SPCATALOG>().Where(t => t.TYPE_ID == type.TYPE_ID).Count();

                await formFile.Import<SpExportData>(async c =>
                {

                    var sp = _dbContext.Query<BASE_SPCATALOG>().Where(t => t.SP_NAME == c.SP_NAME && t.SP_SIZE == c.SP_SIZE).FirstOrDefault();
                    if (sp == null)
                    {
                        typeCount++;
                        sp = new BASE_SPCATALOG
                        {
                            SP_ID = GuidHelper.NewSnowflakeId().ToString(),
                            SP_CODE = $"{type.TYPE_CODE}-{typeCount.ToString("D4")}",
                            SP_NAME = c.SP_NAME,
                            IS_CANCEL = "0",
                            SP_SIZE = c.SP_SIZE,
                            TYPE_NAME = type.TYPE_NAME,
                            TYPE_ID = type.TYPE_ID,
                            TYPE_CODE = type.TYPE_CODE,
                            UNIT = c.UNIT,
                            EDIT_USERID = _userSession.UserID.ToString(),
                            DEPT_ID = _userSession.Corp.CorpID,
                            DEPT_NAME = _userSession.Corp.CName,
                            EDIT_USER = _userSession.RealName,
                            SEC_DEPTID = _userSession.ParentCompany.CorpID,
                            SEC_DEPT = _userSession.ParentCompany.CName,
                            PURTYPE_ID = type.PURTYPE_ID,
                            PURTYPE_NAME = type.PURTYPE_NAME,
                            CREATE_USERID = _userSession.UserID.ToString(),
                            CREATEDATE = dt,
                            MODIFY_USERID = _userSession.UserID.ToString(),
                            MODIFYDATE = dt
                        };
                        _dbContext.Insert(sp);
                    }
                    var temp = sp.MapTo<SP_APPLY_DETAIL>();

                    temp.COUNT = c.COUNT;
                    temp.MEMO = c.MEMO;
                    temp.APPLY_ID = apply.APPLY_ID;
                    await BeforeAddDet(temp);
                    importResult.Add(temp);
                    await Task.CompletedTask;
                });

                _dbContext.InsertRange<SP_APPLY_DETAIL>(importResult);

                return AjaxResult.Success(apply);
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.Message);
            }

        }

        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>().GetGridData(request);
        }

        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_APPLY_DETAIL> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SPDET_ID,
                    c.APPLY_ID,
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.UNIT,
                    c.COUNT,
                    c.STORE_NUM,
                    c.YG_PRICE,
                    c.YG_MONEY,
                    c.LAST_PROVIDERID,
                    c.LAST_PROVIDER,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.IS_STOP,
                    c.MEMO,
                    c.IS_GEN,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.TENANT_ID,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.IS_CANCEL,
                    c.NO_PRODUCE,
                    c.COMP_CODE,
                    c.STORE_MONTH,
                    c.PUR_PERIOD,
                    c.ONROAD_NUM,
                    c.PRO_ID,
                    c.PRO_DET_ID,
                    c.PERIOD,
                    c.IS_XY,
                    c.WARRANTY,
                    c.DELIVERY_CODE,
                    c.XHZQ,
                    c.SYDD,
                    c.CGFS,
                    c.SYDDDEPTID,
                    c.COUNT2,
                    c.CGFS2,
                    c.SP_CODE2,
                    c.COMP_CODE2,
                    c.SP_NAME2,
                    c.SYDD2,
                    c.SYDDDEPTID2,
                    c.SP_SIZE2,
                    c.PRODUCE2,
                    c.UNIT2,
                    c.ZKCS,
                    c.QYKCSL
                },
                c => a => a.SPDET_ID == c.SPDET_ID, BeforeAddDet, BeforeUpdateDet, null, false, null, AfterSaveDet);
        }

        private async Task BeforeAddDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            entity.APPLY_ID = string.IsNullOrEmpty(entity.APPLY_ID) ? _rentID : entity.APPLY_ID;
            entity.SPDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.SP_STATUS = "10";//计划

            if (string.IsNullOrEmpty(entity.SP_CODE))
            {
                var sp = _dbContext.Query<BASE_SPCATALOG>().Where(t => t.SP_NAME == entity.SP_NAME && t.SP_SIZE == entity.SP_SIZE).FirstOrDefault();
                if (sp == null)
                {
                    var type = _dbContext.Query<BASE_SPTYPE>().Where(t => t.TYPE_NAME == "临时类别").FirstOrDefault();
                    var typeCount = _dbContext.Query<BASE_SPCATALOG>().Where(t => t.TYPE_ID == type.TYPE_ID).Count();

                    typeCount++;
                    sp = new BASE_SPCATALOG
                    {
                        SP_ID = GuidHelper.NewSnowflakeId().ToString(),
                        SP_CODE = $"{type.TYPE_CODE}-{typeCount.ToString("D4")}",
                        SP_NAME = entity.SP_NAME,
                        IS_CANCEL = "0",
                        SP_SIZE = entity.SP_SIZE,
                        TYPE_NAME = type.TYPE_NAME,
                        TYPE_ID = type.TYPE_ID,
                        TYPE_CODE = type.TYPE_CODE,
                        UNIT = entity.UNIT,
                        PRODUCE = entity.PRODUCE,
                        EDIT_USERID = _userSession.UserID.ToString(),
                        DEPT_ID = _userSession.Corp.CorpID,
                        DEPT_NAME = _userSession.Corp.CName,
                        EDIT_USER = _userSession.RealName,
                        SEC_DEPTID = _userSession.ParentCompany.CorpID,
                        SEC_DEPT = _userSession.ParentCompany.CName,
                        PURTYPE_ID = type.PURTYPE_ID,
                        PURTYPE_NAME = type.PURTYPE_NAME,
                        CREATE_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = dt,
                        MODIFY_USERID = _userSession.UserID.ToString(),
                        MODIFYDATE = dt
                    };
                    _dbContext.Insert(sp);
                }

                entity.SP_ID = sp.SP_ID;
                entity.SP_CODE = sp.SP_CODE;
                entity.PURTYPE_ID = sp.PURTYPE_ID;
                entity.PURTYPE_NAME = sp.PURTYPE_NAME;
                entity.TYPE_NAME = sp.TYPE_NAME;
                entity.TYPE_ID = sp.TYPE_ID;
                entity.TYPE_CODE = sp.TYPE_CODE;
            }
        }

        private async Task BeforeUpdateDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        private async Task AfterSaveDet(List<SP_APPLY_DETAIL> added, List<SP_APPLY_DETAIL> updated, List<SP_APPLY_DETAIL> deleted)
        {
            var applyId = added.Count == 0 ? updated.Count == 0 ? deleted.Select(c => c.APPLY_ID).FirstOrDefault() : updated.Select(c => c.APPLY_ID).FirstOrDefault() : added.Select(c => c.APPLY_ID).FirstOrDefault();
            await Task.CompletedTask;
            if (!string.IsNullOrEmpty(applyId))
            {
                await _dbContext.UpdateAsync<SP_APPLY>(x => x.APPLY_ID == applyId,
                    x => new SP_APPLY
                    {
                        SUM_MONEY = _dbContext.Query<SP_APPLY_DETAIL>().Where(t => t.APPLY_ID == applyId).Sum(t => t.YG_MONEY)
                    });
            }
        }
        #endregion

        #region 采购进度跟踪
        public class SpApplyDetRes : SP_APPLY_DETAIL
        {
            /// <summary>
            /// 紧急程度
            /// </summary>
            public string EXIG_DEV;

            public string APPLY_USER;

            public DateTime? APPLY_DATE;

            public string DEPT_NAME;
            public string SEC_DEPT;
        }
        public async Task<GridData> ApplyListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>()
                 .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                 .Where((a, b) => b.AUDITING == "1")
                .Select((a, b) => new SpApplyDetRes
                {
                    SP_STATUS = a.SP_STATUS,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    TYPE_NAME = a.TYPE_NAME,
                    IS_XY = a.IS_XY,
                    EXIG_DEV = b.EXIG_DEV,
                    APPLY_USER = b.APPLY_USER,
                    APPLY_DATE = b.APPLY_DATE,
                    DEPT_NAME = b.DEPT_NAME,
                    SEC_DEPT = b.SEC_DEPT,
                    MEMO = a.MEMO,
                    SPDET_ID = a.SPDET_ID
                })
                .GetGridData(request);
        }

        public async Task<AjaxResult> ApplyComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "exig_dev" },
                });
                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "pur_state" }
                });
                dic.TryAdd("StatusData", dic1["BCCode"]);
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        public class SpApplyDetFlowRes
        {
            public string SPDET_ID;
            public string SP_STATUS;
            public string SP_CODE;
            public string SP_NAME;

            public string SP_SIZE;
            public string PRODUCE;
            public string UNIT;
            public string TYPE_NAME;
            public string APPLY_NO;
            public string COLLECT_CODE;
            public string PLAN_NO;
            public string ORDER_CODE;
            public string APPLY_USER;
            public decimal? APPLY_COUNT;
            public string PROVIDER_NAME;
            public string DEPT_NAME;
            public string XJDOWN_USER;
            public string BUY_USER;
            public string PERIOD;
            public decimal? COUNT;
            public decimal? INSTORE_COUNT;
            public decimal? STOP_NUM;
            public decimal? YG_PRICE;
            public decimal? PRICE;


            public DateTime? APPLY_DATE;
            public DateTime? COLLECT_DATE;
            public DateTime? PLAN_DATE;
            public DateTime? ORDER_DATE;
            public DateTime? STOP_DATE;
            public string T_MEMO;
        }
        public async Task<AjaxResult> ApplyDetFlowAsync(string SPDET_ID)
        {
            var applydet = _dbContext.Query<SP_APPLY_DETAIL>()
                 .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                 .Where((a, b) => a.SPDET_ID == SPDET_ID)
                .Select((a, b) => new SpApplyDetFlowRes
                {
                    SP_STATUS = a.SP_STATUS,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    TYPE_NAME = a.TYPE_NAME,
                    APPLY_COUNT = a.COUNT,
                    APPLY_NO = b.APPLY_NO,
                    APPLY_USER = b.APPLY_USER,
                    APPLY_DATE = b.APPLY_DATE,
                    SPDET_ID = a.SPDET_ID,
                    YG_PRICE = a.YG_PRICE
                }).First();

            var col = _dbContext.Query<SP_COLLECT_REQUEST>()
                .LeftJoin<SP_COLLECT>((a, b) => a.COLLECT_ID == b.COLLECT_ID)
                 .Where((a, b) => a.REQUEST_DET_ID == SPDET_ID)
                 .Select((a, b) => new
                 {
                     b.COLLECT_CODE,
                     b.COLLECT_DATE
                 }).FirstOrDefault();

            var pur = _dbContext.Query<SP_PURPLAN_DET>()
             .LeftJoin<SP_PURPLAN>((a, b) => a.PURPLAN_ID == b.PURPLAN_ID)
              .Where((a, b) => a.SPDET_ID == SPDET_ID)
              .Select((a, b) => new
              {
                  b.PLAN_NO,
                  b.PLAN_DATE,
                  b.XJDOWN_USER,
                  a.PERIOD
              }).FirstOrDefault();

            var order = await _dbContext.Query<SP_ORDER_DETAIL>()
             .LeftJoin<SP_ORDER>((a, b) => a.ORDER_ID == b.ORDER_ID)
              .Where((a, b) => a.SPDET_ID == SPDET_ID)
              .Select((a, b) => new
              {
                  b.ORDER_CODE,
                  b.ORDER_DATE,
                  a.STOP_DATE,
                  a.T_MEMO,
                  b.PROVIDER_NAME,
                  b.DEPT_NAME,
                  b.BUY_USER,
                  a.COUNT,
                  a.PRICE,
                  a.STOP_NUM,
                  a.INSTORE_COUNT
              }).FirstAsync();

            applydet.COLLECT_CODE = col?.COLLECT_CODE;
            applydet.COLLECT_DATE = col?.COLLECT_DATE;
            applydet.PLAN_NO = pur?.PLAN_NO;
            applydet.PLAN_DATE = pur?.PLAN_DATE;
            applydet.XJDOWN_USER = pur?.XJDOWN_USER;
            applydet.PERIOD = pur?.PERIOD;

            applydet.ORDER_CODE = order?.ORDER_CODE;
            applydet.ORDER_DATE = order?.ORDER_DATE;
            applydet.STOP_DATE = order?.STOP_DATE;
            applydet.T_MEMO = order?.T_MEMO;
            applydet.PROVIDER_NAME = order?.PROVIDER_NAME;
            applydet.DEPT_NAME = order?.DEPT_NAME;
            applydet.BUY_USER = order?.BUY_USER;
            applydet.COUNT = order?.COUNT;
            applydet.PRICE = order?.PRICE;
            applydet.STOP_NUM = order?.STOP_NUM;
            applydet.INSTORE_COUNT = order?.INSTORE_COUNT;

            return AjaxResult.Success(applydet);
        }
        #endregion
    }
}
