using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpPurplanService : BaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpPurplanService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }


        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_PURPLAN>().GetGridData(request);
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
                     { "ProviderName", (Expression<Func<PROVIDER, bool>>)null},
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
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_PURPLAN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.APPLY_NO,
                    c.AUDITING,
                    c.PLAN_NO,
                    c.PLAN_DATE,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.IS_SUPPACT,
                    c.XJDOWN_USERID,
                    c.XJDOWN_USER,
                    c.XJ_USER,
                    c.XJ_USERID,
                    c.PLAN_PROVIDERID,
                    c.PLAN_PROVIDER,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.OA_CHECK,
                    c.OA_DATE,
                    c.OA_MEMO,
                    c.REQUEST_ID,
                    c.APPLY_ID,
                    c.SUM_MONEY,
                    c.MEMO,
                    c.PURPLAN_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.STATUS,
                    c.IS_PRO,
                    c.BD_NAME,
                    c.PROVIDER_NAME,
                    c.PROVIDER_ID,
                    c.USER_CODE,
                    c.PUR_USERID,
                    c.PUR_USER,
                    c.REQUEST_NAME,
                    c.CGFS,
                    c.MOB_CODE,
                    c.PUR_TYPE,
                    c.PUR_TYPEID,
                    c.SSZT,
                    c.SSZTID,
                    c.MOBILE,
                    c.CGYID,
                    c.CGYDEPTID,
                    c.CGY,
                    c.OACODE,
                    c.JJ_JSON,
                    c.JJ_JSON3,
                    c.ID_URGENT_PURCHASE
                },
                c => a => a.PURPLAN_ID == c.PURPLAN_ID, BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        private async Task BeforeAdd(SP_PURPLAN entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.PURPLAN_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_PURPLAN entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        private async Task BeforeDelete(SP_PURPLAN entity)
        {
            await _dbContext.DeleteAsync<SP_PURPLAN_DET>(x => x.PURPLAN_ID == entity.PURPLAN_ID);
        }

        public async Task<int> Submit(List<string> sids)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            var updatedevice = await _dbContext.UpdateAsync<SP_PURPLAN>(x => sids.Contains(x.PURPLAN_ID),
                    x => new SP_PURPLAN
                    {
                        AUDITING = "1",
                        XJDOWN_USERID = _userSession.UserID.ToString(),
                        XJ_USERID = _userSession.UserID.ToString(),
                        XJDOWN_USER = _userSession.RealName,
                        XJ_USER = _userSession.RealName,
                        PLAN_DATE = dt
                    });

            var list = _dbContext.Query<SP_PURPLAN>().Where(x => sids.Contains(x.PURPLAN_ID)).ToList();

            if (list.Count > 0)
            {
                var importDetail = new List<SP_ORDER_DETAIL>();
                var importList = new List<SP_ORDER>();
                //单号
                string type = $"DD{dt.Value.ToString("yyyyMM")}";
                string def = type + "0000";
                var model = await _dbContext.Query<SP_ORDER>(x => x.ORDER_CODE.Contains(type)).Select(x => Sql.Max(x.ORDER_CODE) ?? def).FirstOrDefaultAsync();

                var i = 1;
                foreach (var item in list)
                {
                    var index = model.SubStr(8, 4).CastTo<int>() + i;
                    //形成物资询价方案
                    var temp = new SP_ORDER
                    {
                        PURPLAN_ID = item.PURPLAN_ID,
                        ORDER_ID = GuidHelper.NewSnowflakeId().ToString(),
                        ORDER_CODE = type + index.ToString("D4"),
                        ORDER_DATE = dt,
                        ORDER_MONEY = item.SUM_MONEY,
                        BUY_USERID = item.PUR_USERID,
                        BUY_USER = item.PUR_USER,
                        PROVIDER_ID = item.PROVIDER_ID,
                        PROVIDER_NAME = item.PROVIDER_NAME,
                        CREATE_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = dt,
                        MODIFY_USERID = _userSession.UserID.ToString(),
                        MODIFYDATE = dt,
                        AUDITING = "0",
                        IS_STOP = "0"
                    };
                    importList.Add(temp);
                    i++;
                    await Task.CompletedTask;

                    var data = _dbContext.Query<SP_PURPLAN_DET>().Where(x => x.PURPLAN_ID == item.PURPLAN_ID).ToList();

                    var appledetId = data.Select(t => t.SPDET_ID).ToList();
                    await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => appledetId.Contains(x.SPDET_ID),
                    x => new SP_APPLY_DETAIL
                    {
                        SP_STATUS = "40"//采购中
                    });

                    foreach (var det in data)
                    {
                        var apply = _dbContext.Query<SP_APPLY>()
                            .Where(a => a.APPLY_ID == det.APPLY_ID)
                            .First();

                        var req = det.MapTo<SP_ORDER_DETAIL>();
                        req.APPLY_NO = apply?.APPLY_NO;
                        req.USE_MEMO = apply?.USE_MEMO;
                        req.APPLY_USERID = apply.APPLY_USERID;
                        req.APPLY_USER = apply.APPLY_USER;
                        req.SPDET_ID = det.SPDET_ID;

                        req.ORDERDET_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.CREATE_USERID = _userSession.UserID.ToString();
                        req.CREATEDATE = dt;
                        req.MODIFY_USERID = _userSession.UserID.ToString();
                        req.MODIFYDATE = dt;
                        req.IS_STOP = "0";
                        req.ORDER_ID = temp.ORDER_ID;
                        importDetail.Add(req);
                        await Task.CompletedTask;
                    }
                }

                await _dbContext.InsertRangeAsync<SP_ORDER>(importList);
                await _dbContext.InsertRangeAsync<SP_ORDER_DETAIL>(importDetail);
            }

            return updatedevice;
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<AjaxResult> CancelSubmit(List<string> sids)
        {
            var list = _dbContext.Query<SP_PURPLAN>().Where(x => sids.Contains(x.PURPLAN_ID)).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (_dbContext.Query<SP_ORDER>().Any(t => t.PURPLAN_ID == item.PURPLAN_ID && t.AUDITING == "1"))
                    {
                        throw new Exception($"{item.PLAN_NO}采购中,不能撤销!");
                    }
                }

                var updatedevice = await _dbContext.UpdateAsync<SP_PURPLAN>(x => sids.Contains(x.PURPLAN_ID),
                    x => new SP_PURPLAN
                    {
                        AUDITING = "0"
                    });

                var data = _dbContext.Query<SP_PURPLAN_DET>().Where(x => sids.Contains(x.PURPLAN_ID)).Select(t => t.SPDET_ID).ToList();
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => data.Contains(x.SPDET_ID),
                x => new SP_APPLY_DETAIL
                {
                    SP_STATUS = "30"//请购中
                });

                var orderId = _dbContext.Query<SP_ORDER>().Where(t => sids.Contains(t.PURPLAN_ID)).Select(t => t.ORDER_ID).ToList();
                await _dbContext.DeleteAsync<SP_ORDER>(x => orderId.Contains(x.ORDER_ID));
                await _dbContext.DeleteAsync<SP_ORDER_DETAIL>(x => orderId.Contains(x.ORDER_ID));
            }
            return AjaxResult.Success("成功");
        }

        public async Task<GridData> DetailListAsync(string PURPLAN_ID, GridRequest request)
        {
            return await _dbContext.Query<SP_PURPLAN_DET>().Where(t => t.PURPLAN_ID == PURPLAN_ID).GetGridData(request);
        }

        public async Task<AjaxResult> DetailSave(SaveRequest<SP_PURPLAN_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.STATUS,
                    c.APPLY_ID,
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.UNIT,
                    c.COUNT,
                    c.PRICE,
                    c.MONEY,
                    c.STORE_NUM,
                    c.APPLY_NO,
                    c.APPLY_DATE,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.LAST_PRICE,
                    c.LAST_PROVIDERID,
                    c.LAST_PROVIDER,
                    c.EXIG_DEV,
                    c.DEVICE_ID,
                    c.DEVICE_CODE,
                    c.DEVICE_NAME,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TIME_REQ,
                    c.PROJECT_CODE,
                    c.PERIOD,
                    c.MEMO,
                    c.REQUEST_ID,
                    c.PURPLAN_ID,
                    c.PLAN_ID,
                    c.SPDET_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.USE_MEMO,
                    c.APPLY_MEMO,
                    c.APPLY_USERID,
                    c.APPLY_USER,
                    c.REQ_DATE,
                    c.WARRANTY,
                    c.YG_MONEY,
                    c.STOP_DATE,
                    c.SYDD,
                    c.CGFS,
                    c.XHZQ,
                    c.SYDDDEPTID,
                    c.COUNT2,
                    c.SP_CODE2,
                    c.SP_NAME2,
                    c.SP_SIZE2,
                    c.PRODUCE2,
                    c.UNIT2
                },
                c => a => a.PLAN_ID == c.PLAN_ID, DetBeforeAdd, DetBeforeUpdate, null, false, null, AfterSaveDet);
        }

        private async Task DetBeforeAdd(SP_PURPLAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.PLAN_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task DetBeforeUpdate(SP_PURPLAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }
        private async Task AfterSaveDet(List<SP_PURPLAN_DET> added, List<SP_PURPLAN_DET> updated, List<SP_PURPLAN_DET> deleted)
        {
            var PurplanId = added.Count == 0 ? (updated.Count == 0 ? deleted.Select(c => c.PURPLAN_ID).FirstOrDefault() : updated.Select(c => c.PURPLAN_ID).FirstOrDefault()) : added.Select(c => c.PURPLAN_ID).FirstOrDefault();
            await Task.CompletedTask;
            if (!string.IsNullOrEmpty(PurplanId))
            {
                var data = _dbContext.Query<SP_PURPLAN_DET>().Where(t => t.PURPLAN_ID == PurplanId)
                    .Select(t => new
                    {
                        t.MONEY
                    }).ToList();

                var SUM_MONEY = data.Sum(t => t.MONEY) ?? 0;
                await _dbContext.UpdateAsync<SP_PURPLAN>(x => x.PURPLAN_ID == PurplanId,
                    x => new SP_PURPLAN
                    {
                        SUM_MONEY = SUM_MONEY
                    });
            }
        }
    }
}
