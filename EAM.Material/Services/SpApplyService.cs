using EAM.Material.DTO;
using EAM.Material.Interfaces;
using Gksyb.Common.Office;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpApplyService : BaseService, ISpApplyService, IFlowInterceptor
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private readonly IFlowEngineService _flowEngineService;
        private string errMsg = string.Empty;

        public SpApplyService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession, IFlowEngineService flowEngineService)
        {
            _dbContext = dbContext;
            //添加船舶物资需求的软删除字段过滤
            _dbContext.HasQueryFilter<SP_APPLY_DETAIL>(x => x.IS_DELETED != "1" || x.IS_DELETED == null);
            _comboxDataService = comboxDataService;
            _userSession = userSession;
            _flowEngineService = flowEngineService;
        }

        #region 船舶物资申请
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
                    SHIP_DEPT = c.SHIP_DEPT,
                    APPLY_DATE = c.APPLY_DATE,
                    CREATE_USERID = c.CREATE_USERID,
                    CREATEDATE = c.CREATEDATE,
                    TYPE_ID2 = c.TYPE_ID2,
                    CGFS = c.CGFS,
                    TYPE_CODE = c.TYPE_CODE,
                    TYPE_NAME = c.TYPE_NAME,
                    MEMO = c.MEMO,
                    DETAILCOUNT = _dbContext.Query<SP_APPLY_DETAIL>().Where(det => det.APPLY_ID == c.APPLY_ID).Count()
                })
                .GetGridData(request);
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
                    { "BCCode@#exigData", "exig_dev" },
                    { "BCCode@#CGFS", "CGtype"},
                    { "BCCode@#ShipDept", "ship_dept"},
                    { "BasePurtype", (Expression<Func<BASE_PURTYPE, bool>>)(x => true)},
                    { "SpUnit", (Expression<Func<SP_UNIT, bool>>)(x => true)},
                    { "Auditing", null },
                    { "SpTypeName", null },
                });
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
            if (!request.Added.IsNullOrEmpty() && request.Added.Any())
            {
                string apply_id;
                if (request.Added[0].APPLY_ID.IsNullOrEmpty())
                {
                    apply_id = request.Added[0].APPLY_ID = GuidHelper.NewSnowflakeId().ToString();
                }
                else
                {
                    apply_id = request.Added[0].APPLY_ID;
                }
                foreach (var entity in requestdet.Added)
                {
                    if (entity.APPLY_ID.IsNullOrEmpty())
                    {
                        entity.APPLY_ID = apply_id;
                    }
                }
            }
            
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
                             c.SHIP_DEPT,
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
                    execResult = await DetailSave(requestdet);
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
            if (entity.APPLY_ID.IsNullOrEmpty())
            {
                entity.APPLY_ID = GuidHelper.NewSnowflakeId().ToString();
            }

            string type = $"SQ{DateTime.Now:yyyyMM}";
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
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }
        private async Task BeforeDelete(SP_APPLY entity)
        {
            await _dbContext.DeleteAsync<SP_APPLY_DETAIL>(x => x.APPLY_ID == entity.APPLY_ID);
        }

        /// <summary>
        /// 获取指定物资的库存数量
        /// </summary>
        /// <param name="sp_id">物资ID</param>
        /// <returns>物资库存数量</returns>
        private async Task<decimal> GetStoreNumAsync(string sp_id)
        {
            return await _dbContext.Query<SP_STORE>(x => x.SP_ID == sp_id)
                .Select(x => Sql.Sum(x.NUM)).FirstOrDefaultAsync() ?? 0;
        }

        /// <summary>
        /// 申请提交
        /// </summary>
        /// <param name="sids">主键数组</param>
        /// <returns>匹配记录数</returns>
        public async Task<int> Submit(List<string> sids)
        {
            int updateCnt = 0;
            await _dbContext.UseTransactionAsync(async () =>
            {
                //匹配物资信息
                var sp_apply_details = await _dbContext.Query<SP_APPLY_DETAIL>(x => sids.Contains(x.APPLY_ID))
                    .ToListAsync();

                for (int i = 0; i < sp_apply_details.Count; ++i)
                {
                    //跟踪sp_apply_details中的实体，记录修改情况
                    _dbContext.TrackEntity(sp_apply_details[i]);

                    //根据 物资名称；型号规格；品牌、厂家；单位 匹配物资
                    var sp_catalog = await _dbContext.Query<BASE_SPCATALOG>(x =>
                        x.SP_NAME == sp_apply_details[i].SP_NAME
                        && x.SP_SIZE == sp_apply_details[i].SP_SIZE
                        && x.PRODUCE == sp_apply_details[i].PRODUCE
                        && x.UNIT == sp_apply_details[i].UNIT)
                        .FirstOrDefaultAsync();

                    if (sp_catalog != null)
                    {
                        //物资目录已有对应物资
                        sp_apply_details[i].SP_ID = sp_catalog.SP_ID;
                        sp_apply_details[i].SP_CODE = sp_catalog.SP_CODE;
                        sp_apply_details[i].PURTYPE_ID = sp_catalog.PURTYPE_ID;
                        sp_apply_details[i].PURTYPE_NAME = sp_catalog.PURTYPE_NAME;
                        sp_apply_details[i].LAST_PROVIDERID = sp_catalog.LAST_PROVIDERID;
                        sp_apply_details[i].LAST_PROVIDER = sp_catalog.LAST_PROVIDER;
                        sp_apply_details[i].WARRANTY = sp_catalog.WARRANTY;

                        //若物资目录中对应物资在临时类别，则将其更新为用户所选类别
                        if (sp_catalog.TYPE_NAME != "临时类别")
                        {
                            sp_apply_details[i].TYPE_ID = sp_catalog.TYPE_ID;
                            sp_apply_details[i].TYPE_NAME = sp_catalog.TYPE_NAME;
                            sp_apply_details[i].TYPE_CODE = sp_catalog.TYPE_CODE;
                        }
                        else if (!sp_apply_details[i].TYPE_ID.IsNullOrWhiteSpace() && sp_apply_details[i].TYPE_NAME != "临时类别")
                        {
                            _dbContext.Update<BASE_SPCATALOG>(a => a.SP_ID == sp_catalog.SP_ID, a => new BASE_SPCATALOG
                            {
                                TYPE_ID = sp_apply_details[i].TYPE_ID,
                                TYPE_NAME = sp_apply_details[i].TYPE_NAME,
                                TYPE_CODE = sp_apply_details[i].TYPE_CODE
                            });
                        }

                        //获取库存数量
                        sp_apply_details[i].STORE_NUM = await GetStoreNumAsync(sp_apply_details[i].SP_ID);
                    }
                    else
                    {
                        //若物资类别为空，则设为临时类别
                        if (string.IsNullOrEmpty(sp_apply_details[i].TYPE_ID))
                        {
                            var sp_type = await _dbContext.Query<BASE_SPTYPE>(x => x.TYPE_NAME == "临时类别").FirstAsync();
                            sp_apply_details[i].TYPE_ID = sp_type.TYPE_ID;
                            sp_apply_details[i].TYPE_NAME = sp_type.TYPE_NAME;
                            sp_apply_details[i].TYPE_CODE = sp_type.TYPE_CODE;
                        }

                        //新物资插入物资目录中
                        string headCode = sp_apply_details[i].TYPE_CODE + "-";
                        string newCode = headCode + "0000";
                        string model = await _dbContext.Query<BASE_SPCATALOG>(a => a.SP_CODE.StartsWith(headCode))
                            .Select(a => Sql.Max(a.SP_CODE)).FirstOrDefaultAsync() ?? newCode;
                        newCode = headCode + (long.Parse(model[headCode.Length..]) + 1).ToString("D4");
                        string newId = GuidHelper.NewSnowflakeId().ToString();
                        var sysdate = await _dbContext.GetSysdate();
                        var new_sp = new BASE_SPCATALOG
                        {
                            SP_ID = newId,
                            SP_CODE = newCode,
                            SP_NAME = sp_apply_details[i].SP_NAME,
                            IS_CANCEL = "0",
                            TYPE_NAME = sp_apply_details[i].TYPE_NAME,
                            TYPE_ID = sp_apply_details[i].TYPE_ID,
                            TYPE_CODE = sp_apply_details[i].TYPE_CODE,
                            SP_SIZE = sp_apply_details[i].SP_SIZE,
                            PRODUCE = sp_apply_details[i].PRODUCE,
                            UNIT = sp_apply_details[i].UNIT,
                            EDIT_USERID = _userSession.UserID.ToString(),
                            DEPT_ID = _userSession.Corp.CorpID,
                            DEPT_NAME = _userSession.Corp.CName,
                            EDIT_USER = _userSession.RealName,
                            SEC_DEPTID = _userSession.ParentCompany.CorpID,
                            SEC_DEPT = _userSession.ParentCompany.CName,
                            PURTYPE_ID = sp_apply_details[i].PURTYPE_ID,
                            PURTYPE_NAME = sp_apply_details[i].PURTYPE_NAME,
                            CREATE_USERID = _userSession.UserID.ToString(),
                            CREATEDATE = sysdate,
                            MODIFY_USERID = _userSession.UserID.ToString(),
                            MODIFYDATE = sysdate,
                        };
                        await _dbContext.InsertAsync(new_sp);

                        sp_apply_details[i].SP_ID = newId;
                        sp_apply_details[i].SP_CODE = newCode;
                    }

                    //物资采购状态修改为「待需求确认」
                    sp_apply_details[i].SP_STATUS = "20";

                    //更新申请明细
                    await _dbContext.UpdateAsync(sp_apply_details[i]);
                }

                //创建内部审批流程
                await CreateWorkFlow(sids);

                //更新记录状态
                updateCnt = await _dbContext.UpdateAsync<SP_APPLY>(x => sids.Contains(x.APPLY_ID),
                    x => new SP_APPLY
                    {
                        AUDITING = "2"
                    });

                //更新采购状态
                await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.APPLY_ID),
                   x => new SP_APPLY_DETAIL
                   {
                       SP_STATUS = "20"//待需求确认
                   });
            });

            return updateCnt;
        }

        /// <summary>
        /// 创建审批流程
        /// </summary>
        /// <param name="sids">主键数组</param>
        /// <returns></returns>
        private async Task CreateWorkFlow(List<string> sids)
        {
            foreach (string sid in sids)
            {
                var flowExecuteInfo = new FlowExecuteInfo();
                var dict = new Dictionary<string, object>();
                dict.TryAdd("Sid", sid);
                dict.TryAdd("isView", false);
                flowExecuteInfo.FormData = dict;
                flowExecuteInfo.FlowCode = "sp_apply";
                await _flowEngineService.StartAsync(flowExecuteInfo);
            }
        }

        /// <summary>
        /// 审批完成
        /// </summary>
        /// <param name="taskInfo">任务信息</param>
        /// <returns></returns>
        public async Task Intercept(FlowExecuteInfo taskInfo)
        {
            var apply_id = taskInfo.FormData.GetValueOrDefault("Sid").ToString();
            var status = taskInfo.NodeStatus;
            //根据审批情况更新记录状态
            switch (status)
            {
                case NodeStatus.Agree:
                    
                    await _dbContext.UpdateAsync<SP_APPLY>(x => x.APPLY_ID == apply_id,
                        x => new SP_APPLY
                        {
                            AUDITING = "3"
                        });
                    break;
                case NodeStatus.Reject:
                    await _dbContext.UpdateAsync<SP_APPLY>(x => x.APPLY_ID == apply_id,
                        x => new SP_APPLY
                        {
                            AUDITING = "4"
                        });
                    break;
                case NodeStatus.Back:
                    await _dbContext.UpdateAsync<SP_APPLY>(x => x.APPLY_ID == apply_id,
                        x => new SP_APPLY
                        {
                            AUDITING = "6"
                        });
                    break;
            }
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


        #region 需求确认
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

        /// <summary>
        /// 获取确认明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> GetCheckListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_APPLY>()
                .InnerJoin<SP_APPLY_DETAIL>((a, b) => a.APPLY_ID == b.APPLY_ID)
                .Where((a, b) => new[] { "1", "3" }.Contains(a.AUDITING))
                .Select((a, b) => new SP_APPLY_DET_DTO
                {
                    //主表数据
                    APPLY_NO = a.APPLY_NO,
                    APPLY_USER = a.APPLY_USER,
                    DEPT_NAME = a.DEPT_NAME,
                    APPLY_DATE = a.APPLY_DATE,
                    APPLY_ID = b.APPLY_ID,
                    //明细表数据
                    SP_ID = b.SP_ID,
                    SP_NAME = b.SP_NAME,
                    SP_CODE = b.SP_CODE,
                    SP_SIZE = b.SP_SIZE,
                    SP_STATUS = b.SP_STATUS,
                    PRODUCE = b.PRODUCE,
                    UNIT = b.UNIT,
                    COUNT = b.COUNT,
                    STORE_NUM = b.STORE_NUM,
                    YG_PRICE = b.YG_PRICE,
                    YG_MONEY = b.YG_MONEY,
                    TYPE_ID = b.TYPE_ID,
                    TYPE_NAME = b.TYPE_NAME,
                    SPDET_ID = b.SPDET_ID,
                    IS_XY = b.IS_XY,
                    NO_PRODUCE = b.NO_PRODUCE,
                    WARRANTY = b.WARRANTY,
                    AUDITING_CHECK = b.AUDITING_CHECK,
                    MEMO = b.MEMO,
                })
                .GetGridData(request);

            return res;
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveCheckList(SaveRequest<SP_APPLY_DETAIL> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING_CHECK,
                    c.SP_NAME,
                    c.COUNT,
                    c.UNIT,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.TYPE_NAME,
                    c.STORE_NUM,
                    c.WARRANTY,
                    c.LAST_PROVIDER,
                    c.LAST_PROVIDERID,
                    c.YG_PRICE,
                    c.YG_MONEY,
                    c.IS_XY,
                    c.NO_PRODUCE,
                    c.MEMO,
                    c.SP_ID,
                    c.APPLY_ID,
                    c.SPDET_ID,
                    c.SP_CODE
                },
                c => a => a.SPDET_ID == c.SPDET_ID
                , null, null, BeforeDeleteApplyDetail, true, null, null);
        }

        private Task BeforeDeleteApplyDetail(SP_APPLY_DETAIL entity)
        {
            entity.IS_DELETED = "1";
            return Task.CompletedTask;
        }

        /// <summary>
        /// 物资需求确认提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SubmitCheckList(List<string> sids)
        {
            var sp_apply_details = await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.SPDET_ID), x => new SP_APPLY_DETAIL
            {
                AUDITING_CHECK = "1",
                SP_STATUS = "30"
            });

            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 物资需求确认撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<AjaxResult> RevokeCheckList(List<string> sids)
        {
            //判断是否已在物资需求申请
            var sp_collect_request_list = await _dbContext.Query<SP_COLLECT_REQUEST>(a => _dbContext.Query<SP_APPLY_DETAIL>(b => sids.Contains(b.SPDET_ID))
                    .Select(b => b.SPDET_ID)
                    .ToList()
                    .Contains(a.REQUEST_DET_ID))
                .InnerJoin<SP_COLLECT>((a, b) => a.COLLECT_ID == b.COLLECT_ID)
                .Select((a, b) => new {
                    a.SP_NAME,
                    b.COLLECT_CODE
                })
                .ToListAsync();
            if (sp_collect_request_list.Any()) {
                errMsg = "物资";
                foreach (var sp_collect_request in sp_collect_request_list)
                {
                    errMsg += $"「{sp_collect_request.SP_NAME}」";
                }
                errMsg += "已在物资需求申请单中，无法撤销提交\n申请单号";
                foreach (var sp_collect_request in sp_collect_request_list)
                {
                    errMsg += $"「{sp_collect_request.COLLECT_CODE}」";
                }
                MessageException.Throw(errMsg);
            }
            var sp_apply_details = await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => sids.Contains(x.SPDET_ID), x => new SP_APPLY_DETAIL
            {
                AUDITING_CHECK = "0",
                SP_STATUS = "20"    //状态变为「待需求确认」
            });

            return AjaxResult.Success("撤销成功");
        }

        #endregion



        /// <summary>
        /// Excel导入
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ImportInDetail([FileOptions("xlsx,xls")] IFormFile formFile, string folder, string sid)
        {
            var apply = string.IsNullOrEmpty(sid) ? new SP_APPLY { AUDITING = "0" } : _dbContext.QueryByKey<SP_APPLY>(sid);
            if (string.IsNullOrEmpty(sid))
            {
                await BeforeAdd(apply);
                _dbContext.Insert(apply);
            }

            var importResult = new List<SP_APPLY_DETAIL>();

            await formFile.Import<SpExportData>(async c =>
            {
                //查询物资目录中是否有匹配的物资
                var sp = _dbContext.Query<BASE_SPCATALOG>()
                    .Where(t =>
                        t.SP_NAME == c.SP_NAME
                        && t.SP_SIZE == c.SP_SIZE
                        && t.PRODUCE == c.PRODUCE
                        && t.UNIT == c.UNIT
                    )
                    .FirstOrDefault();

                SP_APPLY_DETAIL temp;
                if (sp == null)
                {
                    temp = c.MapTo<SP_APPLY_DETAIL>();
                    //新物资默认为临时类别
                    var base_sptype = await _dbContext.Query<BASE_SPTYPE>(x => x.TYPE_NAME == "临时类别").FirstOrDefaultAsync();
                    temp.TYPE_ID = base_sptype.TYPE_ID;
                    temp.TYPE_NAME = base_sptype.TYPE_NAME;
                    temp.TYPE_CODE = base_sptype.TYPE_CODE;
                    //库存数量为0
                    temp.STORE_NUM = 0;
                }
                else
                {
                    temp = sp.MapTo<SP_APPLY_DETAIL>();
                    temp.COUNT = c.COUNT;
                    temp.MEMO = c.MEMO;
                    //获取库存数量
                    temp.STORE_NUM = await GetStoreNumAsync(temp.SP_ID);
                }
                temp.APPLY_ID = apply.APPLY_ID;

                await BeforeAddDet(temp);
                importResult.Add(temp);
                await Task.CompletedTask;
            });

            await _dbContext.InsertRangeAsync(importResult);

            return AjaxResult.Success(apply);
        }

        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_APPLY_DETAIL>()
                .OrderBy(c => c.SP_CODE)
                .GetGridData(request);
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
                    c.ZKCS,
                    c.QYKCSL
                },
                c => a => a.SPDET_ID == c.SPDET_ID, BeforeAddDet, BeforeUpdateDet, null, false, null, AfterSaveDet);
        }

        private async Task BeforeAddDet(SP_APPLY_DETAIL entity)
        {
            if (entity.APPLY_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 APPLY_ID 为空！");
            }
            DateTime? dt = await _dbContext.GetSysdate();
            entity.SPDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.SP_STATUS = "10";//物资申请
            entity.AUDITING_CHECK = "0";    //需求确认状态

            //获取库存数量
            entity.STORE_NUM = await GetStoreNumAsync(entity.SP_ID);
        }

        private async Task BeforeUpdateDet(SP_APPLY_DETAIL entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

            //获取库存数量
            entity.STORE_NUM = await GetStoreNumAsync(entity.SP_ID);
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
            public string EXIG_DEV { get; set; }

            /// <summary>
            /// 申请单号
            /// </summary>
            public string APPLY_NO { get; set; }

            public string APPLY_USER { get; set; }

            public DateTime? APPLY_DATE { get; set; }

            public string DEPT_NAME { get; set; }

            public string SEC_DEPT { get; set; }
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
                    APPLY_NO = b.APPLY_NO,
                    APPLY_USER = b.APPLY_USER,
                    APPLY_DATE = b.APPLY_DATE,
                    DEPT_NAME = b.DEPT_NAME,
                    SEC_DEPT = b.SEC_DEPT,
                    MEMO = a.MEMO,
                    SPDET_ID = a.SPDET_ID
                })
                .OrderBy(c => c.SP_STATUS)
                .ThenByDesc(c => c.APPLY_NO)
                .ThenBy(c => c.SP_CODE)
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
