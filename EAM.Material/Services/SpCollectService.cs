using Gksyb.Common.Office.Core;
using Gksyb.Common.Office.Excel;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.Material;
using Gksyb.Core.Interfaces.OA;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpCollectService : BaseService, ISpCollectService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        private string _rentID = string.Empty, errMsg = string.Empty;

        public SpCollectService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        #region 请购申请

        private class SpCollectRes : SP_COLLECT
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
            var res = await _dbContext.Query<SP_COLLECT>()
                .Select(c => new SpCollectRes
                {
                    COLLECT_ID = c.COLLECT_ID,
                    AUDITING = c.AUDITING,
                    COLLECT_CODE = c.COLLECT_CODE,
                    COLLECT_DATE = c.COLLECT_DATE,
                    COLLECT_USER = c.COLLECT_USER,
                    COLLECT_USERID = c.COLLECT_USERID,
                    DEPT_ID = c.DEPT_ID,
                    DEPT_NAME = c.DEPT_NAME,
                    SEC_DEPTID = c.SEC_DEPTID,
                    SEC_DEPT = c.SEC_DEPT,
                    COLLECT_PRICE = c.COLLECT_PRICE,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    COLLECT_SPTYPE = c.COLLECT_SPTYPE,
                    COLLECT_METHOD = c.COLLECT_METHOD,
                    CONSULT_PROVIDER = c.CONSULT_PROVIDER,
                    MEMO = c.MEMO
                })
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.COLLECT_CODE)
                .GetGridData(request);
            foreach (var item in (List<SpCollectRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => t.COLLECT_ID == item.COLLECT_ID).Count();
            }
            return res;
        }

        public async Task<SP_COLLECT> GetCollectDetail(string ID)
        {
            return await _dbContext.QueryByKeyAsync<SP_COLLECT>(ID);
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
                    { "BCCode", "CGtype" },
                    { "BaseSpType", (Expression<Func<BASE_SPTYPE, bool>>)null},
                    { "ProviderName", (Expression<Func<PROVIDER, bool>>)null},
                    { "Auditing", null }
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestdet"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_COLLECT> request, SaveRequest<SP_COLLECT_REQUEST> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = true, detSuccess = true;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                         c => new
                         {
                             c.AUDITING,
                             c.COLLECT_ID,
                             c.COLLECT_CODE,
                             c.COLLECT_DATE,
                             c.COLLECT_USER,
                             c.COLLECT_USERID,
                             c.DEPT_ID,
                             c.DEPT_NAME,
                             c.SEC_DEPT,
                             c.SEC_DEPTID,
                             c.HOUSE_NAME,
                             c.HOUSE_ID,
                             c.MEMO,
                             c.CREATE_USERID,
                             c.CREATEDATE,
                             c.MODIFY_USERID,
                             c.MODIFYDATE,
                             c.COLLECT_METHOD,
                             c.EDIT_USER,
                             c.COLLECT_PRICE,
                             c.CONFIRM_AUDIT,
                             c.TAX_MONEY,
                             c.NOTAX_MONEY,
                             c.CONFIRM_CODE,
                             c.CONFIRM_DATE,
                             c.PROVIDER_CODE,
                             c.PROVIDER_ID,
                             c.PROVIDER_NAME,
                             c.HOUSE_CODE,
                             c.STORE_TYPE,
                             c.HOUSE_USER,
                             c.HOUSE_USERID,
                             c.COLLECT_SPTYPE,
                             c.RATIO,
                             c.CONSULT_PROVIDER,
                             c.BD_NO
                         },
                          c => a => a.COLLECT_ID == c.COLLECT_ID, BeforeAdd, BeforeUpdate, BeforeDelete);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet ??= new SaveRequest<SP_COLLECT_REQUEST>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                      c => new
                      {
                          c.REQUEST_CODE,
                          c.REQUEST_DATE,
                          c.REQUEST_USER,
                          c.REQUEST_USERID,
                          c.SP_CODE,
                          c.SP_NAME,
                          c.SP_SIZE,
                          c.PRODUCE,
                          c.OTHER_CODE,
                          c.UNIT,
                          c.DEPT_NAME,
                          c.DEPT_ID,
                          c.SEC_DEPT,
                          c.SEC_DEPTID,
                          c.MEMO,
                          c.COLLECT_REQUEST_ID,
                          c.COLLECT_DET_ID,
                          c.COLLECT_ID,
                          c.REQUEST_DET_ID,
                          c.CREATE_USERID,
                          c.CREATEDATE,
                          c.MODIFY_USERID,
                          c.MODIFYDATE,
                          c.TYPE_CODE,
                          c.TYPE_NAME,
                          c.TYPE_ID,
                          c.SP_DAIMA,
                          c.SP_TUHAO,
                          c.SP_ENGNAME,
                          c.SP_ID,
                          c.CONFIRM_NUM,
                          c.COLLECT_MONEY,
                          c.TAX_PRICE,
                          c.TAX_MONEY,
                          c.NOTAX_PRICE,
                          c.NOTAX_MONEY,
                          c.REQUEST_NUM,
                          c.CHECK_NUM,
                          c.IS_FULLBUY
                      },
                     c => a => a.COLLECT_REQUEST_ID == c.COLLECT_REQUEST_ID, BeforeAddRequest, BeforeUpdateRequest, BeforeDeleteRequest);

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

        private async Task BeforeAdd(SP_COLLECT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.COLLECT_ID = _rentID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            string type = $"QG{dt.Value:yyyyMM}";
            string def = type + "0000";
            var model = await _dbContext.Query<SP_COLLECT>(x => x.COLLECT_CODE.Contains(type)).Select(x => Sql.Max(x.COLLECT_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;

            entity.COLLECT_CODE = type + index.ToString("D4");

            entity.COLLECT_DATE = dt;
            entity.COLLECT_USERID = _userSession.UserID.ToString();
            entity.COLLECT_USER = _userSession.RealName;
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.AUDITING = "0";
            entity.CONFIRM_AUDIT = "0";

            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_COLLECT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            _rentID = entity.COLLECT_ID;
            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeDelete(SP_COLLECT entity)
        {
            await _dbContext.DeleteAsync<SP_COLLECT_DET>(x => x.COLLECT_ID == entity.COLLECT_ID);
            await _dbContext.DeleteAsync<SP_COLLECT_REQUEST>(x => x.COLLECT_ID == entity.COLLECT_ID);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_COLLECT>(x => sids.Contains(x.COLLECT_ID),
                    x => new SP_COLLECT
                    {
                        AUDITING = "1"
                    });

            var isDockOA = _dbContext.Query<BC_CODE>(c => c.CODE_TYPE == "对接OA").First().CODE_EN;

            if (isDockOA == "1")
            {
                //推送到OA
                foreach (var i in sids)
                {
                    await CreateWorkFlow(i);
                }
            }

            return updatedevice;
        }

        /// <summary>
        /// 审批完成 OA回调接口
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="isPass"></param>
        /// <returns></returns>
        public async Task<AjaxResult> ApprovalCompletedAsync(string sid, bool isPass)
        {
            if (isPass)
            {
                var updatedevice = await _dbContext.UpdateAsync<SP_COLLECT>(x => x.COLLECT_ID == sid,
                x => new SP_COLLECT
                {
                    AUDITING = "3"
                });
            }
            else
            {
                var updatedevice = await _dbContext.UpdateAsync<SP_COLLECT>(x => x.COLLECT_ID == sid,
                x => new SP_COLLECT
                {
                    AUDITING = "4"
                });
                return AjaxResult.Success("审批否决");
            }

            var appledetId = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => t.COLLECT_ID == sid).Select(t => t.REQUEST_DET_ID).ToList();
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                  x => new SP_APPLY_DETAIL
                  {
                      SP_STATUS = "50"//采购订单待提交
                  });

            var list = _dbContext.Query<SP_COLLECT>().Where(x => x.COLLECT_ID == sid).ToList();

            if (list.Count > 0)
            {
                DateTime? dt = await _dbContext.GetSysdate();
                var importDetail = new List<SP_ORDER_DETAIL>();
                var importList = new List<SP_ORDER>();
                string type = $"DD{dt.Value:yyyyMM}";
                string def = type + "0000";
                var model = await _dbContext.Query<SP_ORDER>(x => x.ORDER_CODE.Contains(type)).Select(x => Sql.Max(x.ORDER_CODE) ?? def).FirstOrDefaultAsync();
                var i = 1;

                foreach (var item in list)
                {
                    var index = model.SubStr(8, 4).CastTo<int>() + i;
                    //形成采购订单
                    var temp = new SP_ORDER
                    {
                        PURPLAN_ID = item.COLLECT_ID,
                        ORDER_ID = GuidHelper.NewSnowflakeId().ToString(),
                        ORDER_CODE = type + index.ToString("D4"),
                        ORDER_DATE = dt,
                        ORDER_MONEY = item.COLLECT_PRICE,
                        BUY_USERID = item.COLLECT_USERID,
                        BUY_USER = item.COLLECT_USER,
                        PROVIDER_ID = item.PROVIDER_ID,
                        PROVIDER_NAME = item.PROVIDER_NAME,
                        CREATE_USERID = "", //原为_userSession.UserID.ToString()，因OA回调时并无登录用户，故取消
                        CREATEDATE = dt,
                        MODIFY_USERID = "",
                        MODIFYDATE = dt,
                        AUDITING = "0",
                        IS_STOP = "0"
                    };
                    i++;
                    importList.Add(temp);
                    await Task.CompletedTask;

                    var data = _dbContext.Query<SP_COLLECT_REQUEST>().Where(x => x.COLLECT_ID == item.COLLECT_ID).ToList();
                    foreach (var det in data)
                    {
                        var apply = _dbContext.Query<SP_APPLY>()
                            .LeftJoin<SP_APPLY_DETAIL>((a, b) => a.APPLY_ID == b.APPLY_ID)
                            .Where((a, b) => b.SPDET_ID == det.REQUEST_DET_ID)
                            .Select((a, b) => new
                            {
                                a.APPLY_NO,
                                a.USE_MEMO
                            })
                            .FirstOrDefault();
                        var req = det.MapTo<SP_ORDER_DETAIL>();
                        req.APPLY_NO = apply?.APPLY_NO;
                        req.USE_MEMO = apply?.USE_MEMO;
                        req.APPLY_USERID = det.REQUEST_USERID;
                        req.APPLY_USER = det.REQUEST_USER;
                        req.SPDET_ID = det.REQUEST_DET_ID;

                        req.ORDERDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.CREATE_USERID = "";
                        req.CREATEDATE = dt;
                        req.MODIFY_USERID = "";
                        req.MODIFYDATE = dt;

                        req.COUNT = det.CHECK_NUM;
                        req.PRICE = det.TAX_PRICE;
                        req.MONEY = det.COLLECT_MONEY;
                        req.ORDER_ID = temp.ORDER_ID;
                        req.IS_STOP = "0";
                        importDetail.Add(req);
                        await Task.CompletedTask;
                    }
                }

                await _dbContext.InsertRangeAsync(importList);
                await _dbContext.InsertRangeAsync(importDetail);
            }

            return AjaxResult.Success("成功");
        }

        public async Task<AjaxResult> Revoke(List<string> sids)
        {
            var list = _dbContext.Query<SP_COLLECT>().Where(x => sids.Contains(x.COLLECT_ID)).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (_dbContext.Query<SP_ORDER>().Any(t => t.PURPLAN_ID == item.COLLECT_ID && t.AUDITING == "1"))
                    {
                        throw new Exception($"{item.COLLECT_CODE}采购中,不能撤销!");
                    }
                }

                var updatedevice = await _dbContext.UpdateAsync<SP_COLLECT>(x => sids.Contains(x.COLLECT_ID),
                x => new SP_COLLECT
                {
                    AUDITING = "0"
                });
                var appledetId = _dbContext.Query<SP_COLLECT_REQUEST>().Where(t => sids.Contains(t.COLLECT_ID)).Select(t => t.REQUEST_DET_ID).ToList();
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                      x => new SP_APPLY_DETAIL
                      {
                          SP_STATUS = "30"//待需求申请
                      });

                var orderId = _dbContext.Query<SP_ORDER>().Where(t => sids.Contains(t.PURPLAN_ID)).Select(t => t.ORDER_ID).ToList();
                await _dbContext.DeleteAsync<SP_ORDER>(x => orderId.Contains(x.ORDER_ID));
                await _dbContext.DeleteAsync<SP_ORDER_DETAIL>(x => orderId.Contains(x.ORDER_ID));
            }
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_COLLECT_DET>()
                .OrderBy(a => a.SP_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SP_COLLECT_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.FACTORY,
                    c.COLLECT_NUM,
                    c.MEMO,
                    c.ARRIVE_NUM,
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.SP_ID,
                    c.REQUEST_DET_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.IN_NUM,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.STORE_NUM
                },
                c => a => a.COLLECT_DET_ID == c.COLLECT_DET_ID, BeforeAddDet, BeforeUpdateDet);
        }

        private async Task BeforeAddDet(SP_COLLECT_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.COLLECT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdateDet(SP_COLLECT_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        /// <summary>
        /// 需求列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> RequestListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_COLLECT_REQUEST>().GetGridData(request);
        }

        /// <summary>
        /// 需求保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> RequestSave(SaveRequest<SP_COLLECT_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REQUEST_CODE,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.COLLECT_REQUEST_ID,
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.REQUEST_DET_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.SP_ID,
                    c.CONFIRM_NUM,
                    c.COLLECT_MONEY,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.REQUEST_NUM,
                    c.CHECK_NUM,
                    c.IS_FULLBUY
                },
                c => a => a.COLLECT_REQUEST_ID == c.COLLECT_REQUEST_ID, BeforeAddRequest, BeforeUpdateRequest, BeforeDeleteRequest);
        }

        private async Task BeforeAddRequest(SP_COLLECT_REQUEST entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            entity.COLLECT_ID = string.IsNullOrEmpty(entity.COLLECT_ID) ? _rentID : entity.COLLECT_ID;
            entity.COLLECT_REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

            var appledet = _dbContext.Query<SP_APPLY_DETAIL>()
              .Where(t => t.SPDET_ID == entity.REQUEST_DET_ID)
              .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
              .Select((a, b) => new SpApplyDetRes
              {
                  SP_ID = a.SP_ID,
                  SP_CODE = a.SP_CODE,
                  SP_NAME = a.SP_NAME,
                  SP_SIZE = a.SP_SIZE,
                  PRODUCE = a.PRODUCE,
                  UNIT = a.UNIT,
                  COUNT = a.COUNT,
                  STORE_NUM = a.STORE_NUM,
                  TYPE_ID = a.TYPE_ID,
                  TYPE_CODE = a.TYPE_CODE,
                  TYPE_NAME = a.TYPE_NAME,
                  APPLY_NO = b.APPLY_NO,
                  APPLY_USER = b.APPLY_USER,
                  DEPT_NAME = b.DEPT_NAME,
                  DEPT_ID = b.DEPT_ID,
                  SEC_DEPT = b.SEC_DEPT,
                  SEC_DEPTID = b.SEC_DEPTID,
                  APPLY_USERID = b.APPLY_USERID
              }).FirstOrDefault();

            entity.REQUEST_CODE = appledet.APPLY_NO;
            entity.REQUEST_USER = appledet.APPLY_USER;
            entity.REQUEST_USERID = appledet.APPLY_USERID;
            entity.DEPT_ID = appledet.DEPT_ID;
            entity.SEC_DEPT = appledet.SEC_DEPT;
            entity.SEC_DEPTID = appledet.SEC_DEPTID;
            entity.TYPE_ID = appledet.TYPE_ID;
            entity.TYPE_CODE = appledet.TYPE_CODE;
        }

        private async Task BeforeUpdateRequest(SP_COLLECT_REQUEST entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeDeleteRequest(SP_COLLECT_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 待请购的采购申请明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SpApplyListAsync(GridRequest request)
        {
            var request_det_ids = _dbContext.Query<SP_COLLECT_REQUEST>().Select(x => x.REQUEST_DET_ID)
                .ToList();

            return await _dbContext.Query<SP_APPLY_DETAIL>()
                .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                .Where((a, b) => a.AUDITING_CHECK == "1" && !request_det_ids.Contains(a.SPDET_ID))
                .Select((a, b) => new
                {
                    a.SPDET_ID,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.TYPE_ID,
                    a.TYPE_NAME,
                    a.TYPE_CODE,
                    a.COUNT,
                    b.APPLY_NO,
                    b.APPLY_USER,
                    b.DEPT_NAME,
                    b.APPLY_DATE
                })
                .OrderByDesc(c => c.APPLY_NO)
                .ThenBy(c => c.SP_CODE)
                .GetGridData(request);
        }

        private class SpApplyDetRes : SP_APPLY_DETAIL
        {
            /// <summary>
            /// 申请编号
            /// </summary>
            public string APPLY_NO;

            public string APPLY_USER;
            public string APPLY_USERID;
            public string DEPT_NAME { get; set; }

            /// <summary>
            /// 申请部门ID
            /// </summary>
            public string DEPT_ID { get; set; }

            /// <summary>
            /// 二级单位
            /// </summary>
            public string SEC_DEPT { get; set; }

            /// <summary>
            /// 二级单位ID
            /// </summary>
            public string SEC_DEPTID { get; set; }
        }

        /// <summary>
        /// 选中的采购申请明细
        /// </summary>
        /// <param name="SpdetID"></param>
        /// <param name="Cid"></param>
        /// <returns></returns>
        public async Task<int> SelectApply(List<string> SpdetID, string Cid)
        {
            var appledet = _dbContext.Query<SP_APPLY_DETAIL>()
                .Where(t => SpdetID.Contains(t.SPDET_ID))
                .LeftJoin<SP_APPLY>((a, b) => a.APPLY_ID == b.APPLY_ID)
                .Select((a, b) => new SpApplyDetRes
                {
                    SPDET_ID = a.SPDET_ID,
                    APPLY_ID = a.APPLY_ID,
                    SP_ID = a.SP_ID,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    COUNT = a.COUNT,
                    STORE_NUM = a.STORE_NUM,
                    YG_PRICE = a.YG_PRICE,
                    YG_MONEY = a.YG_MONEY,
                    LAST_PROVIDERID = a.LAST_PROVIDERID,
                    LAST_PROVIDER = a.LAST_PROVIDER,
                    TYPE_ID = a.TYPE_ID,
                    TYPE_CODE = a.TYPE_CODE,
                    TYPE_NAME = a.TYPE_NAME,
                    IS_STOP = a.IS_STOP,
                    MEMO = a.MEMO,
                    IS_GEN = a.IS_GEN,
                    TENANT_ID = a.TENANT_ID,
                    PURTYPE_ID = a.PURTYPE_ID,
                    PURTYPE_NAME = a.PURTYPE_NAME,
                    IS_CANCEL = a.IS_CANCEL,
                    NO_PRODUCE = a.NO_PRODUCE,
                    COMP_CODE = a.COMP_CODE,
                    STORE_MONTH = a.STORE_MONTH,
                    PUR_PERIOD = a.PUR_PERIOD,
                    ONROAD_NUM = a.ONROAD_NUM,
                    PRO_ID = a.PRO_ID,
                    PRO_DET_ID = a.PRO_DET_ID,
                    PERIOD = a.PERIOD,
                    IS_XY = a.IS_XY,
                    WARRANTY = a.WARRANTY,
                    DELIVERY_CODE = a.DELIVERY_CODE,
                    XHZQ = a.XHZQ,
                    SYDD = a.SYDD,
                    CGFS = a.CGFS,
                    SYDDDEPTID = a.SYDDDEPTID,
                    ZKCS = a.ZKCS,
                    QYKCSL = a.QYKCSL,
                    APPLY_NO = b.APPLY_NO,
                    APPLY_USER = b.APPLY_USER,
                    DEPT_NAME = b.DEPT_NAME,
                    DEPT_ID = b.DEPT_ID,
                    SEC_DEPT = b.SEC_DEPT,
                    SEC_DEPTID = b.SEC_DEPTID,
                    APPLY_USERID = b.APPLY_USERID
                })
                .OrderBy(a => a.SP_CODE)
                .ToList();

            var importRequest = new List<SP_COLLECT_REQUEST>();
            foreach (var item in appledet)
            {
                var req = item.MapTo<SP_COLLECT_REQUEST>();
                await BeforeAddRequest(req);
                req.COLLECT_ID = Cid;
                req.REQUEST_CODE = item.APPLY_NO;
                req.REQUEST_DET_ID = item.SPDET_ID;
                req.REQUEST_NUM = item.COUNT;
                req.REQUEST_USER = item.APPLY_USER;
                req.REQUEST_USERID = item.APPLY_USERID;
                importRequest.Add(req);
                await Task.CompletedTask;
            }

            if (importRequest.Count > 0)
            {
                await _dbContext.InsertRangeAsync<SP_COLLECT_REQUEST>(importRequest);
            }

            var appIds = appledet.Select(t => t.SPDET_ID).ToList();
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appIds.Contains(x.SPDET_ID),
                x => new SP_APPLY_DETAIL
                {
                    SP_STATUS = "30"//请购中
                });
            return appledet.Count;
        }

        #endregion 请购申请

        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="collectId"></param>
        /// <returns></returns>
        public async Task<AjaxResult> CreateWorkFlow(string collectId)
        {
            await _dbContext.DBLog("OA创建流程", "", $"即将请求创建OA流程 COLLECT_ID = {collectId}", "");

            #region 推送oa

            var taskId = GuidHelper.NewSnowflakeId().ToString();
            var corpId = _userSession.Corp.CorpID;

            //获取主表数据
            var query = await _dbContext.Query<SP_COLLECT>(c => c.COLLECT_ID == collectId).FirstAsync();
            if (query == null) return AjaxResult.Error("未找到该份采购需求记录", "失败");

            //获取附件
            var fj = _dbContext.Query<SYS_ATTACH>().Where(c => c.data_id == collectId && c.table_name == "SP_COLLECT").ToList();
            var webUrl = _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "网站地址").First().CODE_EN;
            string attachName = string.Empty, attachUrl = string.Empty;
            if (fj != null && fj.Count > 0)
            {
                foreach (var item in fj)
                {
                    attachName += item.attach_name + "|";
                    attachUrl += webUrl + item.attach_path + "|";
                }
            }

            //生成物资明细附件
            var detQuery = await _dbContext.Query<SP_COLLECT_REQUEST>(c => c.COLLECT_ID == collectId)
                .Select(c => new SP_COLLECT_REQUEST_DTO
                {
                    REQUEST_CODE = c.REQUEST_CODE,
                    REQUEST_DATE = c.REQUEST_DATE,
                    DEPT_NAME = c.DEPT_NAME,
                    REQUEST_USER = c.REQUEST_USER,
                    SP_CODE = c.SP_CODE,
                    SP_NAME = c.SP_NAME,
                    SP_SIZE = c.SP_SIZE,
                    PRODUCE = c.PRODUCE,
                    UNIT = c.UNIT,
                    REQUEST_NUM = c.REQUEST_NUM,
                    TYPE_NAME = c.TYPE_NAME,
                    MEMO = c.MEMO
                }).ToListAsync();

            string fileName = "";
            string fileRealName = GuidHelper.NewSnowflakeId().ToString() + ".xlsx";
            string directoryPath = "UploadDirectory/SpCollect/";
            string fileUrl = "";
            //创建文件夹
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (detQuery.Count > 0)
            {
                fileName = "物资需求申请(" + query.COLLECT_CODE + ").xlsx";

                //创建EXCEL文件
                IExporter exporter = new ExcelExporter();
                var content = await exporter.ExportAsByteArray(detQuery);
                using var stream = new MemoryStream();
                stream.Write(content, 0, content.Length);
                FormFile ff = new(stream, 0, stream.Length, fileName, fileName);
                fileUrl = await ff.SaveAs("SpCollect", fileRealName);
            }

            string excelUrl = attachUrl + (string.IsNullOrEmpty(fileName) ? "" : webUrl + fileUrl);

            string attach = attachName + (string.IsNullOrEmpty(fileName) ? "" : fileName) + "$$$"
                + excelUrl;

            var mainQuery = await _dbContext.Query<SP_COLLECT>(c => c.COLLECT_ID == collectId)
                .Select(c => new
                {
                    collect_code = c.COLLECT_CODE,
                    primary_key = c.COLLECT_ID,
                    fun_name = "_spCollectService.ApprovalCompletedAsync",
                    sm = c.MEMO,
                    fjsc = attach
                }).FirstAsync();

            //对接OA 取配置地址
            string url = _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "OA接口地址").First().CODE_EN;

            OAHandle oa = new(_dbContext);
            string result = await oa.CreateFlow(url, "SJQS", $"工作请示（采购需求{mainQuery.collect_code}）- {_userSession.RealName}", _userSession.Phone, null, mainQuery, null);
            //OA返回结果：{"msg":"创建流程成功","code":"1162464","success":true,"url":"999"}
            await _dbContext.DBLog("OA创建流程返回结果", "", "案件审批流程创建" + "\n" + result, "");

            if (string.IsNullOrEmpty(result)) return AjaxResult.Error("推送OA异常", "失败");

            JObject job = JObject.Parse(result);
            if (job["success"] != null && job["success"].ToString().ToLower() == "true")
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    //成功后将记录状态改为审批中，保存OA编号和Excel附件的网络地址
                    _dbContext.Update<SP_COLLECT>(a => a.COLLECT_ID == collectId, a => new SP_COLLECT
                    {
                        AUDITING = "2",
                        OA_CODE = job["code"].ToString(),
                        EXCEL_URL = excelUrl
                    });

                    //修改船舶物资申请明细的物资采购状态
                    var request_det_ids = _dbContext.Query<SP_COLLECT_REQUEST>(x => x.COLLECT_ID == collectId)
                        .Select(x => x.REQUEST_DET_ID)
                        .ToList();
                    await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => request_det_ids.Contains(x.SPDET_ID), x => new SP_APPLY_DETAIL
                    {
                        SP_STATUS = "40"
                    });
                });
            }
            else
            {
                return AjaxResult.Error("推送OA创建流程失败：" + job["msg"].ToString(), "失败");
            }

            #endregion 推送oa

            return AjaxResult.Success("创建流程成功", "成功");
        }
    }

    #region SP_COLLECT_REQUEST DTO

    public class SP_COLLECT_REQUEST_DTO
    {
        /// <summary>
        /// 需求计划单号
        /// </summary>
        [ExporterHeader(DisplayName = "申请单号", Width = 15)]
        [Display(Name = "申请单号")]
        [Description("需求计划单号")]
        public string REQUEST_CODE { get; set; }

        /// <summary>
        /// 申请部门
        /// </summary>
        [ExporterHeader(DisplayName = "申请部门", Width = 10)]
        [Display(Name = "申请部门")]
        [Description("申请部门")]
        public string DEPT_NAME { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        [ExporterHeader(DisplayName = "申请人", Width = 10)]
        [Display(Name = "申请人")]
        [Description("申请人")]
        public string REQUEST_USER { get; set; }

        /// <summary>
        /// 申请日期
        /// </summary>
        [ExporterHeader(DisplayName = "申请日期", Format = "yyyy-MM-dd hh:mm:ss")]
        [Display(Name = "申请日期")]
        [Description("申请日期")]
        public DateTime? REQUEST_DATE { get; set; }

        /// <summary>
        /// 物资编码
        /// </summary>
        [ExporterHeader(DisplayName = "物资编码", Width = 15)]
        [Display(Name = "物资编码")]
        [Description("物资编码")]
        public string SP_CODE { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        [ExporterHeader(DisplayName = "物资名称", Width = 30)]
        [Display(Name = "物资名称")]
        [Description("物资名称")]
        public string SP_NAME { get; set; }

        /// <summary>
        /// 型号规格
        /// </summary>
        [ExporterHeader(DisplayName = "型号规格", Width = 30)]
        [Display(Name = "型号规格")]
        [Description("型号规格")]
        public string SP_SIZE { get; set; }

        /// <summary>
        /// 品牌、厂家
        /// </summary>
        [ExporterHeader(DisplayName = "品牌、厂家", Width = 30)]
        [Display(Name = "品牌、厂家")]
        [Description("品牌、厂家")]
        public string PRODUCE { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Display(Name = "计量单位")]
        [Description("计量单位")]
        public string UNIT { get; set; }

        /// <summary>
        /// 申请数量
        /// </summary>
        [Display(Name = "申请数量")]
        [Description("申请数量")]
        public decimal? REQUEST_NUM { get; set; }

        /// <summary>
        /// 物资类别
        /// </summary>
        [ExporterHeader(DisplayName = "物资类别", Width = 30)]
        [Display(Name = "物资类别")]
        [Description("物资类别")]
        public string TYPE_NAME { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [ExporterHeader(DisplayName = "备注", Width = 30)]
        [Display(Name = "备注")]
        [Description("备注")]
        public string MEMO { get; set; }
    }

    #endregion SP_COLLECT_REQUEST DTO
}