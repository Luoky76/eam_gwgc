using Chloe;
using Gksyb.Common;
using Gksyb.Common.Office.Core;
using Gksyb.Common.Office.Excel;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.OA;
using Gksyb.Core.Interfaces.Repair;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace EAM.Repair.services
{
    public class RepairPlanService : IRepairPlanService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;
        private string errMsg = string.Empty;

        public RepairPlanService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxDataAsync()
        {
            var result = await _comboxDataService.Get(new Dictionary<string, object>(){
                    { "ShipList", null },
                    { "MaintDept", null},
                    { "RepairType", null },
                    { "RepitemType", null },
                    { "RepairDealType", null },
                    { "Auditing", null },
                    { "User", null },
                    { "PlanState", null },
            });
            result.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return result;
        }

        #region 维修计划

        /// <summary>
        /// 船舶列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ShipList()
        {
            var result = await _dbContext.Query<DEVICE_CARD>(a => a.TYPE_ID == "1")//设备类别为船舶
                .OrderBy(c => c.DEVICE_ID)
                .Select(c => new DEVICE_CARD
                {
                    DEVICE_ID = c.DEVICE_ID,
                    DEVICE_NAME = c.DEVICE_NAME,
                    DEVICE_NO = c.DEVICE_NO,
                    DEPT_NAME = c.DEPT_NAME,
                    DEPT_ID = c.DEPT_ID,
                    DEVICE_TYPE = c.DEVICE_TYPE,
                    INSTALL_SITE = c.INSTALL_SITE,
                })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        public async Task<GridData> GetDeviceAsync(GridRequest request)
        {
            var query = await _dbContext.Query<DEVICE_CARD>().Where(c => c.TYPE_ID == "2").GetGridData(request);
            return query;
        }

        /// <summary>
        /// 故障报修提交
        /// </summary>
        public async Task<AjaxResult> SubmitReportAsync(string exeId)
        {
            var qry = await _dbContext.Query<REP_PLAN_EXE_ITEM>(c => c.EXE_ID == exeId).ToListAsync();
            if (!qry.Any())
            {
                throw new MessageException("请添加维修项目明细");
            }
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_A = "1",
                PLAN_STATE = "20" // 故障待审
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 故障核验提交
        /// </summary>
        public async Task<AjaxResult> SubmitAuditAsync(string exeId)
        {
            var qry = await _dbContext.Query<REP_PLAN_EXE_ITEM>(c => c.EXE_ID == exeId).ToListAsync();
            if (!qry.Any())
            {
                throw new MessageException("请添加维修项目明细");
            }
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_B = "1",
                PLAN_STATE = "30" // 待实施
            });
            //创建OA流程
            //await CreateWorkFlow(exeId);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 维修实施提交
        /// </summary>
        public async Task<AjaxResult> SubmitExeAsync(string exeId)
        {
            var qry = await _dbContext.Query<REP_PLAN_EXE_ITEM>(c => c.EXE_ID == exeId).Select(c => c.IS_COMPLETE).ToListAsync();
            if (qry.Contains(null))
            {
                throw new MessageException("请确认「是否完成」全部填写");
            }
            var sysdate = await _dbContext.GetSysdate();
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_C = "1",
                PLAN_STATE = "40", // 待验收
                EXE_USER = _userSession.UserName,
                EXE_USERID = _userSession.UserID.ToString(),
                EIDT_DATE = sysdate
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 维修验收提交
        /// </summary>
        public async Task<AjaxResult> SubmitCheckAsync(string exeId)
        {
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_D = "1",
                PLAN_STATE = "50", // 已验收
                CHECK_USER = _userSession.UserName,
                CHECK_USERID = _userSession.UserID.ToString()
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 故障报修撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeReportAsync(string exeId)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_PLAN_EXE>(exeId);
            if (qry.AUDITING_B != "0")
            {
                throw new MessageException("已核验，无法撤回");
            }
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_A = "0",
                PLAN_STATE = "10" // 故障上报
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 故障核验撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeAuditAsync(string exeId)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_PLAN_EXE>(exeId);
            if (qry.AUDITING_C != "0")
            {
                throw new MessageException("已实施，无法撤回");
            }
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_B = "0",
                PLAN_STATE = "20" // 故障待审
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 维修实施撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeExeAsync(string exeId)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_PLAN_EXE>(exeId);
            if (qry.AUDITING_D != "0")
            {
                throw new MessageException("已验收，无法撤回");
            }
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_C = "0",
                PLAN_STATE = "30", // 待实施
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 维修验收撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeCheckAsync(string exeId)
        {
            await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == exeId, x => new REP_PLAN_EXE()
            {
                AUDITING_D = "0",
                PLAN_STATE = "40" // 待验收
            });
            return AjaxResult.Success();
        }

        #endregion 维修计划

        #region 维修计划实施

        public async Task<GridData> ExeListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_EXE, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
            {
                a.AUDITING_A,
                a.AUDITING_B,
                a.AUDITING_C,
                a.AUDITING_D,
                a.EXE_CODE,
                a.CHECK_CODE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.PLAN_STATE,
                a.DEAL_TYPE,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_STOP_TIME,
                a.ACT_START_DATE,
                a.ACT_END_DATE,
                a.ACT_STOP_TIME,
                a.EXE_USER,
                a.ASSIST_USER,
                a.IS_LEAVE,
                a.EXE_DESC,
                a.LEAVE_MEMO,
                a.FAULT_DESCRIBE,
                a.REP_LEVEL,
                a.PLAN_CODE,
                a.PLAN_MEMO,
                a.AUDIT_USER,
                a.REPORT_USER,
                a.AUDIT_USERID,
                a.REPORT_USERID,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.REPAIR_MEMO,
                a.EIDT_DATE,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                a.EXE_ID,
                a.COLLECT_METHOD,
                a.PLAN_MONEY,
                a.CHECK_DATE,
            }).GetGridData(request);

            return query;
        }

        public async Task<AjaxResult> GetExeDetailAsync(string ID)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_EXE, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
            {
                a.AUDITING_A,
                a.AUDITING_B,
                a.AUDITING_C,
                a.AUDITING_D,
                a.EXE_CODE,
                a.PLAN_STATE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.AUDIT_USER,
                a.REPORT_USER,
                a.AUDIT_USERID,
                a.REPORT_USERID,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_STOP_TIME,
                a.FAULT_DESCRIBE,
                a.REP_LEVEL,
                a.PLAN_CODE,
                a.PLAN_MEMO,
                a.DEPT_ID,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.REPAIR_MEMO,
                a.EIDT_DATE,
                a.CHECK_CODE,
                a.CHECK_DESC,
                a.CHECK_DATE,
                a.CHECK_MEMO,
                a.CHECK_USER,
                a.ACT_START_DATE,
                a.ACT_END_DATE,
                a.ACT_STOP_TIME,
                a.EXE_USER,
                a.ASSIST_USER,
                a.IS_LEAVE,
                a.EXE_DESC,
                a.LEAVE_MEMO,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                a.EXE_ID,
                a.COLLECT_METHOD,
                a.PLAN_MONEY,
                a.ACT_MONEY
            }).Where(x => x.EXE_ID == ID).FirstAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 获取维修项目明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ExeItemListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<REP_PLAN_EXE_ITEM>().GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            if (!request.Added.IsNullOrEmpty() && request.Added.Any())
            {
                string exe_id;
                if (request.Added[0].EXE_ID.IsNullOrEmpty())
                {
                    exe_id = request.Added[0].EXE_ID = GuidHelper.NewSnowflakeId().ToString();
                }
                else
                {
                    exe_id = request.Added[0].EXE_ID;
                }
                foreach (var entity in requestdet.Added)
                {
                    if (entity.EXE_ID.IsNullOrWhiteSpace())
                    {
                        entity.EXE_ID = exe_id;
                    }
                }
            }
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING_A,
                         c.AUDITING_B,
                         c.AUDITING_C,
                         c.AUDITING_D,
                         c.EXE_CODE,
                         c.MAINT_TYPE,
                         c.DEAL_TYPE,
                         c.PLAN_STATE,
                         c.ACT_START_DATE,
                         c.ACT_END_DATE,
                         c.ACT_STOP_TIME,
                         c.EXE_USER,
                         c.ASSIST_USER,
                         c.IS_LEAVE,
                         c.EXE_DESC,
                         c.LEAVE_MEMO,
                         c.FAULT_DESCRIBE,
                         c.REP_LEVEL,
                         c.AUDIT_USER,
                         c.REPORT_USER,
                         c.AUDIT_USERID,
                         c.REPORT_USERID,
                         c.CHARGE_USER,
                         c.REPAIR_MEMO,
                         c.EIDT_DATE,
                         c.DEVICE_ID,
                         c.CHECK_CODE,
                         c.CHECK_DESC,
                         c.CHECK_DATE,
                         c.CHECK_MEMO,
                         c.CHECK_USER,
                         c.EXE_ID,
                         c.DEPT_NAME,
                         c.WSEC_DEPT,
                         c.PLAN_MEMO,
                         c.PLAN_START_DATE,
                         c.PLAN_END_DATE,
                         c.PLAN_STOP_TIME,
                         c.COLLECT_METHOD,
                         c.PLAN_MONEY,
                         c.ACT_MONEY
                     },
                     c => a => a.EXE_ID == c.EXE_ID
                     , BeforeAdd, null, BeforeDelete, false, null, null, true, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet ??= new SaveRequest<REP_PLAN_EXE_ITEM>();
                    detSuccess = !(await SaveExeItem(requestdet)).IsError;  //明细表是否保存成功
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

        /// <summary>
        /// 新增前
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(REP_PLAN_EXE entity)
        {
            if (entity.EXE_ID.IsNullOrEmpty())
            {
                entity.EXE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.AUDITING_A.IsNullOrWhiteSpace()) entity.AUDITING_A = "0";
            if (entity.AUDITING_B.IsNullOrWhiteSpace()) entity.AUDITING_B = "0";
            if (entity.AUDITING_C.IsNullOrWhiteSpace()) entity.AUDITING_C = "0";
            if (entity.AUDITING_D.IsNullOrWhiteSpace()) entity.AUDITING_D = "0";

            entity.REPORT_USER = _userSession.UserName;
            entity.REPORT_USERID = _userSession.UserID.ToString();
            string type = "WXSB" + DateTime.Now.ToString("yyyyMM");
            string def = type + "0000";
            var model = await _dbContext.Query<REP_PLAN_EXE>(x => x.EXE_CODE.Contains(type)).Select(x => Sql.Max(x.EXE_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            entity.EXE_CODE = type + index.ToString("D4");

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <returns></returns>
        private async Task BeforeDelete(REP_PLAN_EXE entity)
        {
            await _dbContext.DeleteAsync<REP_PLAN_EXE_ITEM>(x => x.EXE_ID == entity.EXE_ID);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(REP_PLAN_EXE_ITEM entity)
        {
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXE_ID 为空！");
            }
            if (entity.EXE_ITEM_ID.IsNullOrWhiteSpace())
            {
                entity.EXE_ITEM_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDet(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDet(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        public async Task<AjaxResult> SaveExeItem(SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _dbContext.SaveEntityAnsyc(requestdet,
                c => new
                {
                    c.EXE_ITEM_ID,
                    c.EXE_ID,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.REP_CONTENT,
                    c.IS_COMPLETE,
                    c.USE_TOOL,
                    c.LABOR_NUM,
                    c.TAKE_TIME,
                    c.BEGIN_TIME,
                    c.END_TIME,
                    c.MEMO,
                    c.DEAL_TYPE,
                    c.REP_LEADER,
                    c.REP_INDEX,
                    c.IS_ASKBID,
                    c.ITEM_TYPE,
                    c.DEVICE_NO,
                    c.DEVICE_SIZE,
                    c.DEVICE_TYPE,
                    c.DEVICE_NUM,
                    c.STOCK_NAME,
                },
                c => a => a.EXE_ITEM_ID == c.EXE_ITEM_ID, BeforeAddDet, BeforeUpdateDet);
        }

        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="exeId"></param>
        /// <returns></returns>
        public async Task<AjaxResult> CreateWorkFlow(string exeId)
        {
            #region 推送oa

            var taskId = GuidHelper.NewSnowflakeId().ToString();
            var corpId = _userSession.Corp.CorpID;

            //获取主表数据
            var query = await _dbContext.Query<REP_PLAN_EXE>(c => c.EXE_ID == exeId).FirstAsync();
            if (query == null) return AjaxResult.Error("未找到该份维修计划记录", "失败");

            //获取附件
            var fj = _dbContext.Query<SYS_ATTACH>().Where(c => c.data_id == exeId && c.table_name == "REP_PLAN_EXE").ToList();
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

            //生成维修项目明细附件
            var detQuery = await _dbContext.Query<REP_PLAN_EXE_ITEM>(c => c.EXE_ID == exeId)
                .Select(c => new REP_PLAN_EXE_ITEM
                {
                    REP_INDEX = c.REP_INDEX,
                    DEVICE_NAME = c.DEVICE_NAME,
                    DEVICE_TYPE = c.DEVICE_TYPE,
                    REP_CONTENT = c.REP_CONTENT,
                    ITEM_TYPE = c.ITEM_TYPE,
                    REP_LEADER = c.REP_LEADER,
                    IS_ASKBID = c.IS_ASKBID == "0" ? "否" : "是",
                    MEMO = c.MEMO
                }).ToListAsync();
            string fileName = "";
            string fileRealName = GuidHelper.NewSnowflakeId().ToString() + ".xlsx";
            string directoryPath = "UploadDirectory/RepairPlan/";
            string fileUrl = "";
            //创建文件夹
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (detQuery.Count > 0)
            {
                fileName = "维修计划(" + query.PLAN_CODE + ").xlsx";

                //创建EXCEL文件
                IExporter exporter = new ExcelExporter();
                var content = await exporter.ExportAsByteArray(detQuery);
                using var stream = new MemoryStream();
                stream.Write(content, 0, content.Length);
                var ff = new FormFile(stream, 0, stream.Length, fileName, fileName);
                fileUrl = await ff.SaveAs("RepairPlan", fileRealName);
            }

            string attach = attachName + (string.IsNullOrEmpty(fileName) ? "" : fileName) + "$$$"
                + attachUrl + (string.IsNullOrEmpty(fileName) ? "" : webUrl + fileUrl);

            var mainQuery = await _dbContext.Query<REP_PLAN_EXE>(c => c.EXE_ID == exeId)
                .Select(c => new
                {
                    plan_code = c.PLAN_CODE,
                    primary_key = c.EXE_ID,
                    fun_name = "_repairPlanService.ApprovalCompletedAsync",
                    bdmc = c.DEPT_NAME + "维修计划申请",
                    sm = c.PLAN_MEMO,
                    fjsc = attach
                }).FirstAsync();

            string jsonData = mainQuery.ToJson();

            //对接OA 取配置地址
            string url = _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "OA接口地址").First().CODE_EN;

            var oa = new OAHandle(_dbContext);
            var phone = (await _userService.Find(new List<long?> { _userSession.UserID })).FirstOrDefault().Phone;
            string result = await oa.CreateFlow(url, "SJQS", $"工作请示（采购需求{mainQuery.plan_code}）- {_userSession.RealName}", phone, _userSession.UserName, mainQuery, null);
            //OA返回结果：{"msg":"创建流程成功","code":"1162464","success":true,"url":"999"}
            await _dbContext.DBLog("OA创建流程返回结果", "", "案件审批流程创建" + "\n" + result, "");

            if (string.IsNullOrEmpty(result)) return AjaxResult.Error("推送OA异常", "失败");

            JObject job = JObject.Parse(result);
            if (job["success"] != null && job["success"].ToString().ToLower() == "true")
            {
                //成功后将记录状态改为审批中
                _dbContext.Update<REP_PLAN_EXE>(a => a.EXE_ID == exeId, a => new REP_PLAN_EXE
                {
                    AUDITING_B = "2",
                    OA_CODE = job["code"].ToString()
                });
            }
            else
            {
                return AjaxResult.Error("推送OA创建流程失败：" + job["msg"].ToString(), "失败");
            }

            #endregion 推送oa

            return AjaxResult.Success("创建流程成功", "成功");
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
                var updatedevice = await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == sid,
                x => new REP_PLAN_EXE
                {
                    AUDITING_B = "3"
                });
                return AjaxResult.Success("审批成功");
            }
            else
            {
                var updatedevice = await _dbContext.UpdateAsync<REP_PLAN_EXE>(x => x.EXE_ID == sid,
                x => new REP_PLAN_EXE
                {
                    AUDITING_B = "4"
                });
                return AjaxResult.Success("审批否决");
            }
        }

        #endregion 维修计划实施
    }
}