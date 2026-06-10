using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Core.Interfaces.Material;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpOutService : IBaseService, IFlowInterceptor
    {
        private readonly string _flowCode = "sp_out";
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;
        private readonly SpStoreService _spStoreService;
        private readonly IFlowEngineService _flowEngineService;

        public SpOutService(IDbContext dbContext, IComboxDataService comboxDataService,
            ICodeCreatorService codeCreatorService, UserSession userSession,
            ICorpService corpService, SpStoreService spStoreService,
            IFlowEngineService flowEngineService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;
            _corpService = corpService;
            _spStoreService = spStoreService;
            _flowEngineService = flowEngineService;
        }

        #region 下拉数据

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            var data = await _comboxDataService.Get(new Dictionary<string, object>()
            {
                { "BCCode@#Auditing", "auditing" },
                { "BCCode@#Emergency", "emergency" },
                { "SpHouse", (Expression<Func<SP_HOUSE, bool>>)(x => true) },
                { "DeviceCard", (Expression<Func<DEVICE_CARD, bool>>)(x => _dbContext.Query<CF_CORP>(cc => cc.CORPID == x.DEPT_ID && cc.CORP_PATH.StartsWith(_userSession.ParentCompany.TreeNode)).Any()) },
                { "UserWithCorp@#User", (Expression<Func<CF_USER, bool>>)(x => true) },
            });
            data.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return data;
        }

        #endregion 下拉数据

        #region 主表CRUD

        /// <summary>
        /// 根据ID获取出库记录
        /// </summary>
        public async Task<SP_OUTSTORE> GetAsync(string outId)
        {
            return await _dbContext.QueryByKeyAsync<SP_OUTSTORE>(outId);
        }

        /// <summary>
        /// 获取出库列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE>(x => _dbContext.Query<CF_CORP>().Where(a => a.CORPID == x.OUT_DEPT_ID && a.CORP_PATH.StartsWith(_userSession.ParentCompany.TreeNode)).Any())
                .LeftJoin<BC_CODE>((a, b) => b.CODE_TYPE == "auditing" && a.AUDITING == b.CODE_EN)
                .Select((a, b) => new
                {
                    a.AUDITING,
                    ORDERED_AUDITING = b.CODE_SEQ,
                    a.OUT_ID,
                    a.OUT_CODE,
                    a.OUT_DATE,
                    a.OUT_USERID,
                    a.OUT_USER,
                    a.OUT_DEPT_ID,
                    a.OUT_DEPT_NAME,
                    a.SHIP_DEPT,
                    a.OUT_TYPE,
                    a.EMERGENCY,
                    a.PURPOSE,
                    a.MEMO,
                    a.IS_RED,
                    a.RED_USERID,
                    a.RED_USER,
                    a.RED_DATE
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 保存主表
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_OUTSTORE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.AUDITING, c.OUT_CODE, c.OUT_DATE, c.OUT_USERID, c.OUT_USER, c.OUT_DEPT_ID, c.OUT_DEPT_NAME, c.SHIP_DEPT, c.OUT_TYPE, c.EMERGENCY, c.PURPOSE, c.MEMO, c.IS_RED },
                c => a => a.OUT_ID == c.OUT_ID,
                BeforeAdd, BeforeUpdate, BeforeDelete, orgin: true);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAdd(SP_OUTSTORE entity)
        {
            if (entity.OUT_ID.IsNullOrWhiteSpace())
            {
                entity.OUT_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.OUT_CODE.IsNullOrWhiteSpace())
            {
                entity.OUT_CODE = await _codeCreatorService.CreateCodeAsync<SP_OUTSTORE>("CK", a => a.OUT_CODE);
            }
            if (entity.OUT_DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.OUT_DEPT_ID = _userSession.Corp.CorpID;
                entity.OUT_DEPT_NAME = _userSession.Corp.SName;
            }
            if (entity.OUT_USERID.IsNullOrWhiteSpace())
            {
                entity.OUT_USERID = _userSession.UserID.ToString();
                entity.OUT_USER = _userSession.RealName;
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            if (entity.OUT_TYPE.IsNullOrWhiteSpace())
            {
                entity.OUT_TYPE = "20";
            }
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task BeforeUpdate(SP_OUTSTORE entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前处理
        /// </summary>
        private async Task BeforeDelete(SP_OUTSTORE entity)
        {
            await _dbContext.DeleteAsync<SP_OUTSTORE_DET>(x => x.OUT_ID == entity.OUT_ID);
        }

        #endregion 主表CRUD

        #region 明细CRUD

        /// <summary>
        /// 获取明细列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE_DET>().GetGridData(request);
        }

        /// <summary>
        /// 保存明细
        /// </summary>
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_OUTSTORE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.OUT_ID, c.STORE_ID, c.BATCH_CODE, c.SP_ID, c.SP_NAME, c.SP_CODE, c.TYPE_ID, c.TYPE_NAME, c.PRODUCE, c.SP_SIZE, c.UNIT, c.PROPERTY, c.OUT_NUM, c.TAX_MONEY, c.NOTAX_MONEY, c.HOUSE_ID, c.HOUSE_NAME, c.HOUSE_CODE, c.DEVICE_ID, c.DEVICE_NAME, c.DEVICE_NO },
                c => a => a.OUT_DET_ID == c.OUT_DET_ID,
                BeforeAddDet, BeforeUpdateDet, null, orgin: true);
        }

        /// <summary>
        /// 新增明细前处理
        /// </summary>
        private async Task BeforeAddDet(SP_OUTSTORE_DET entity)
        {
            if (entity.OUT_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("子表关联外键缺失");
            }
            if (entity.OUT_DET_ID.IsNullOrWhiteSpace())
            {
                entity.OUT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await HandleDet(entity);
        }

        /// <summary>
        /// 更新明细前处理
        /// </summary>
        private async Task BeforeUpdateDet(SP_OUTSTORE_DET entity)
        {
            await HandleDet(entity);
        }

        /// <summary>
        /// 明细检查和预处理
        /// </summary>
        private async Task HandleDet(SP_OUTSTORE_DET entity)
        {
            if (entity.OUT_NUM < 0)
            {
                throw new MessageException($"物资「{entity.SP_NAME}」出库数量不能为负数");
            }
            if (entity.OUT_NUM == 0)
            {
                throw new MessageException($"物资「{entity.SP_NAME}」出库数量不能为零");
            }
            if (!entity.OUT_NUM.HasValue)
            {
                throw new MessageException($"物资「{entity.SP_NAME}」出库数量不能为空");
            }
            // 有批次时按可变单价计算出库金额；无批次时金额为空
            if (entity.STORE_ID.IsNullOrWhiteSpace())
            {
                entity.TAX_MONEY = null;
                entity.NOTAX_MONEY = null;
            }
            else
            {
                var store = await _dbContext.QueryByKeyAsync<SP_STORE>(entity.STORE_ID);
                MessageException.ThrowIf(store == null, $"批次「{entity.BATCH_CODE}」物资「{entity.SP_NAME}」可能已被冲红撤回，请删除后重新选择新批次");
                // 扣减审批中和本单已占用的数量
                var occupied_num = _dbContext.Query<SP_OUTSTORE>(so => so.AUDITING == "2" || so.OUT_ID == entity.OUT_ID)
                    .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                    .Where((so, sod) => sod.STORE_ID == entity.STORE_ID)
                    .Select((so, sod) => Sql.Sum(sod.OUT_NUM)).FirstOrDefault() ?? 0;
                MessageException.ThrowIf(store.STORE_NUM - occupied_num < 0, $"批次「{entity.BATCH_CODE}」物资「{entity.SP_NAME}」库存数量不足");
                // 可变单价计算金额
                entity.TAX_MONEY = store.STORE_NUM == entity.OUT_NUM ? store.TAX_MONEY : Math.Round((store.TAX_MONEY * entity.OUT_NUM / store.STORE_NUM).Value, 2);
                entity.NOTAX_MONEY = store.STORE_NUM == entity.OUT_NUM ? store.NOTAX_MONEY : Math.Round((store.NOTAX_MONEY * entity.OUT_NUM / store.STORE_NUM).Value, 2);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 子表导入批次
        /// </summary>
        public async Task<AjaxResult> DetImportAsync(string outId, List<string> store_ids)
        {
            var store_ids_count = store_ids.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var out_det_list = await _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == outId).ToListAsync();
            var request = new SaveRequest<SP_OUTSTORE_DET>()
            {
                Added = new List<SP_OUTSTORE_DET>(),
                Original = new List<SP_OUTSTORE_DET>(),
                Updated = new List<SP_OUTSTORE_DET>()
            };
            foreach (var store_id_count in store_ids_count)
            {
                var store_id = store_id_count.Key;
                // 判断明细中是否已存在该批次
                var existed_out_det = out_det_list.FirstOrDefault(x => x.STORE_ID == store_id);
                if (existed_out_det != null)
                {
                    request.Original.Add(existed_out_det.MapTo<SP_OUTSTORE_DET>());
                    existed_out_det.OUT_NUM = (existed_out_det.OUT_NUM ?? 0) + store_id_count.Value;
                    request.Updated.Add(existed_out_det);
                    continue;
                }
                var store = await _dbContext.Query<SP_STORE>(x => x.STORE_ID == store_id).FirstOrDefaultAsync();
                MessageException.ThrowIf(store == null, "批次不存在");
                var new_out_det = store.MapTo<SP_OUTSTORE_DET>();
                new_out_det.OUT_ID = outId;
                new_out_det.OUT_NUM = store_id_count.Value;
                new_out_det.TAX_MONEY = null;
                new_out_det.NOTAX_MONEY = null;
                new_out_det.CREATE_USERID = null;
                new_out_det.CREATEUSER = null;
                new_out_det.CREATEDATE = null;
                new_out_det.MODIFY_USERID = null;
                new_out_det.MODIFYUSER = null;
                new_out_det.MODIFYDATE = null;
                request.Added.Add(new_out_det);
            }
            return await DetSaveAsync(request);
        }

        #endregion 明细CRUD

        #region 同时保存主子表

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(
            SaveRequest<SP_OUTSTORE> request1, SaveRequest<SP_OUTSTORE_DET> request2)
        {
            string OUT_ID;
            // 填写主子表关联键值
            if (request1.Updated.Any() && !request1.Updated.First().OUT_ID.IsNullOrWhiteSpace())
            {
                OUT_ID = request1.Updated.First().OUT_ID;
            }
            else if (request1.Added.Any() && !request1.Added.First().OUT_ID.IsNullOrWhiteSpace())
            {
                OUT_ID = request1.Added.First().OUT_ID;
            }
            else OUT_ID = GuidHelper.NewSnowflakeId().ToString();
            if (request1.Added.Any() && request1.Added.First().OUT_ID.IsNullOrWhiteSpace())
            {
                request1.Added[0].OUT_ID = OUT_ID;
            }

            foreach (var entity in request2.Added ??= new List<SP_OUTSTORE_DET>())
            {
                if (entity.OUT_ID.IsNullOrWhiteSpace()) entity.OUT_ID = OUT_ID;
            }

            try
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    if ((await SaveAsync(request1)).IsError)
                    {
                        throw new MessageException("物资出库保存失败");
                    }
                    if ((await DetSaveAsync(request2)).IsError)
                    {
                        throw new MessageException("物资出库明细保存失败");
                    }
                });
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.Message);
            }
            return AjaxResult.Success("保存成功");
        }

        #endregion 同时保存主子表

        #region 提交与撤销

        /// <summary>
        /// 提交出库
        /// </summary>
        public async Task<AjaxResult> SubmitAsync(string outId)
        {
            // 判断是否有定义内部流程
            var hasFlow = (await _flowEngineService.FlowListAsync(x => x.FlowCode == _flowCode)).Any();
            if (hasFlow)
            {
                await SubmitWithFlow(outId);
            }
            else
            {
                await SubmitNomal(outId);
            }
            return AjaxResult.Success();
        }

        /// <summary>
        /// 无审批流程提交
        /// </summary>
        private async Task SubmitNomal(string outId)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                // 分配批次
                await AllocateBatch(outId);
                // 更新库存，生成出库流水
                await Outstore(outId);
                // 更新记录状态为已提交
                await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == outId,
                    x => new SP_OUTSTORE { AUDITING = "1" });
            });
        }

        /// <summary>
        /// 有审批流程提交
        /// </summary>
        private async Task SubmitWithFlow(string outId)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                // 分配批次
                await AllocateBatch(outId);
                // 更新记录状态为审批中
                await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == outId,
                    x => new SP_OUTSTORE { AUDITING = "2" });
                // 创建审批流程
                await CreateWorkFlow(outId);
            });
        }

        /// <summary>
        /// 创建审批流程
        /// </summary>
        private async Task CreateWorkFlow(string sid)
        {
            var flowExecuteInfo = new FlowExecuteInfo();
            var dict = new Dictionary<string, object>();
            dict.TryAdd("Sid", sid);
            dict.TryAdd("isView", true);
            // 计算出库金额
            var EST_MONEY = _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == sid)
                .Sum(x => x.TAX_MONEY) ?? 0;
            dict.TryAdd("EST_MONEY", EST_MONEY);
            var sp_out = await _dbContext.QueryByKeyAsync<SP_OUTSTORE>(sid);
            // 单号
            dict.TryAdd("DOCUMENT_CODE", sp_out.OUT_CODE);
            // 获取部门信息
            var corp = await _dbContext.QueryByKeyAsync<CF_CORP>(sp_out.OUT_DEPT_ID);
            dict.TryAdd("DEPT_NAME", corp.CORP_SNAME);
            // 组织层级
            dict.TryAdd("DEPT_LEVEL", corp.CORP_PATH.Length / 3);
            // 紧急程度
            dict.TryAdd("EMERGENCY", sp_out.EMERGENCY);
            // 领用人id
            dict.TryAdd("OUT_USERID", sp_out.OUT_USERID);
            flowExecuteInfo.FormData = dict;
            flowExecuteInfo.FlowCode = _flowCode;
            flowExecuteInfo.CorpId = sp_out.OUT_DEPT_ID;
            await _flowEngineService.StartAsync(flowExecuteInfo);
        }

        /// <summary>
        /// 审批流程回调
        /// </summary>
        public async Task Intercept(FlowExecuteInfo taskInfo)
        {
            var out_id = taskInfo.FormData.GetValueOrDefault("Sid").ToString();
            var status = taskInfo.NodeStatus;
            var current_node = "";
            if (taskInfo.CurrentNode != null) current_node = taskInfo.CurrentNode.Name;
            switch (status)
            {
                case NodeStatus.Agree:
                    if (current_node == "end")
                    {
                        // 审批通过，扣库存生成流水
                        await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == out_id,
                            x => new SP_OUTSTORE { AUDITING = "3" });
                        await Outstore(out_id);
                    }
                    break;
                case NodeStatus.Reject:
                    await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == out_id,
                        x => new SP_OUTSTORE { AUDITING = "4" });
                    break;
                case NodeStatus.Back:
                    await _dbContext.UseTransactionAsync(async () =>
                    {
                        await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == out_id,
                            x => new SP_OUTSTORE { AUDITING = "6" });
                    });
                    break;
            }
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeAsync(string outId)
        {
            var sp_out_det = _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == outId).Select(x => x.OUT_DET_ID);
            var qry_water = _dbContext.Query<SP_STORE_WATER>(x => sp_out_det.ToList().Contains(x.SRC_ID));
            var qry_after_water = qry_water.InnerJoin<SP_STORE_WATER>(
                (a, b) =>
                    a.STORE_ID == b.STORE_ID &&
                    b.WATER_DATE > a.WATER_DATE &&
                    !qry_water.Where(x => x.WATER_ID == b.WATER_ID).Any())
                .Select((a, b) => b);

            if (qry_after_water.Any())
            {
                throw new MessageException("库存已被使用，无法撤销");
            }

            // 撤销审批流程
            var hasFlow = (await _flowEngineService.FlowListAsync(x => x.FlowCode == _flowCode)).Any();
            if (hasFlow)
            {
                // 获取当前任务ID
                var taskInfo = await _flowEngineService.TaskInfoAsync(t => t.TaskKey == outId && t.FlowCode == _flowCode);
                if (taskInfo != null)
                {
                    var cancelInfo = new FlowExecuteInfo { TaskId = taskInfo.TaskId };
                    await _flowEngineService.CancelAsync(cancelInfo);
                }
            }

            await _dbContext.UseTransactionAsync(async () =>
            {
                // 相关库存主键
                var store_ids = await qry_water.Select(x => x.STORE_ID).Distinct().ToListAsync();
                // 删除流水
                await _dbContext.DeleteAsync<SP_STORE_WATER>(x => sp_out_det.ToList().Contains(x.SRC_ID));
                // 删除没有流水的库存
                await _dbContext.DeleteAsync<SP_STORE>(x => store_ids.Contains(x.STORE_ID) && !_dbContext.Query<SP_STORE_WATER>(ssw => ssw.STORE_ID == x.STORE_ID).Any());
                // 依据最后的流水更新库存
                await _dbContext.UpdateAsync<SP_STORE>(x => store_ids.Contains(x.STORE_ID), x => new SP_STORE
                {
                    STORE_NUM = _dbContext.Query<SP_STORE_WATER>(ssw => ssw.STORE_ID == x.STORE_ID)
                        .OrderByDesc(ssw => ssw.WATER_DATE).First().CUR_NUM,
                    TAX_MONEY = _dbContext.Query<SP_STORE_WATER>(ssw => ssw.STORE_ID == x.STORE_ID)
                        .OrderByDesc(ssw => ssw.WATER_DATE).First().CUR_TAX_MONEY,
                    NOTAX_MONEY = _dbContext.Query<SP_STORE_WATER>(ssw => ssw.STORE_ID == x.STORE_ID)
                        .OrderByDesc(ssw => ssw.WATER_DATE).First().CUR_NOTAX_MONEY
                });

                await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_ID == outId, x => new SP_OUTSTORE
                {
                    AUDITING = "0"
                });
            });

            return AjaxResult.Success();
        }

        /// <summary>
        /// 获取审批任务ID
        /// </summary>
        public async Task<string> GetTaskIdAsync(string outId)
        {
            var taskInfo = await _flowEngineService.TaskInfoAsync(t => t.TaskKey == outId && t.FlowCode == _flowCode);
            return taskInfo?.TaskId;
        }

        #endregion 提交与撤销

        #region FIFO批次分配

        /// <summary>
        /// 提交时按先进先出方式分配批次
        /// </summary>
        private async Task AllocateBatch(string outId)
        {
            // 获取当前登录用户所在公司的所有仓库
            var house_id_list = await _dbContext.Query<SP_HOUSE>()
                .LeftJoin<CF_CORP>((sh, cc) => sh.DEPT_ID == cc.CORPID)
                .Where((sh, cc) => cc.CORP_PATH.StartsWith(_userSession.ParentCompany.TreeNode))
                .Select((sh, cc) => sh.HOUSE_ID)
                .ToListAsync();

            // 获取所需领用的各仓库各物资总数及对应当前库存数量
            var sum_out_det = _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == outId)
                .GroupBy(x => new { x.SP_ID, x.SP_NAME })
                .Select(x => new
                {
                    x.SP_ID,
                    x.SP_NAME,
                    SUM_OUT_NUM = Sql.Sum(x.OUT_NUM) ?? 0,
                    SUM_STORE_NUM = _dbContext.Query<SP_STORE>(s =>
                            s.SP_ID == x.SP_ID && house_id_list.Contains(s.HOUSE_ID))
                        .Sum(s => s.STORE_NUM) ?? 0,
                    SUM_OCCUPY_NUM = _dbContext.Query<SP_OUTSTORE>(so => so.AUDITING == "2")
                        .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                        .Where((so, sod) => sod.SP_ID == x.SP_ID && house_id_list.Contains(sod.HOUSE_ID))
                        .Select((so, sod) => Sql.Sum(sod.OUT_NUM)).FirstOrDefault() ?? 0
                });

            // 判断库存数量是否足够
            var understock = await sum_out_det.Where(x => x.SUM_OUT_NUM > x.SUM_STORE_NUM - x.SUM_OCCUPY_NUM).ToListAsync();
            if (understock.Any())
            {
                string msg = "库存数量不足\n";
                foreach (var i in understock)
                {
                    msg += $"物资「{i.SP_NAME}」可领用数量为「{i.SUM_STORE_NUM - i.SUM_OCCUPY_NUM}」，领用数量「{i.SUM_OUT_NUM}」过多\n";
                }
                throw new MessageException(msg);
            }

            {
                // 已有确定批次的出库明细不再重新分配批次，只计算出库金额并事先更新
                var allocated_out_det_data = await _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == outId && !string.IsNullOrWhiteSpace(x.STORE_ID)).ToListAsync();
                foreach (var out_det_datum in allocated_out_det_data)
                {
                    var batch = await _dbContext.QueryByKeyAsync<SP_STORE>(out_det_datum.STORE_ID);
                    MessageException.ThrowIf(batch == null, $"批次「{out_det_datum.BATCH_CODE}」物资「{out_det_datum.SP_NAME}」可能已被冲红撤回，请删除后重新选择新批次");
                    _dbContext.TrackEntity(out_det_datum);
                    out_det_datum.TAX_MONEY = (out_det_datum.OUT_NUM == batch.STORE_NUM ? batch.TAX_MONEY : Math.Round((batch.TAX_MONEY * out_det_datum.OUT_NUM / batch.STORE_NUM).Value, 2)).Value;
                    out_det_datum.NOTAX_MONEY = (out_det_datum.OUT_NUM == batch.STORE_NUM ? batch.NOTAX_MONEY : Math.Round((batch.NOTAX_MONEY * out_det_datum.OUT_NUM / batch.STORE_NUM).Value, 2)).Value;
                    await _dbContext.UpdateAsync(out_det_datum);
                }
            }

            // 按先进先出分配批次
            var sum_out_det_data = await sum_out_det.ToListAsync();
            foreach (var sum_out_det_datum in sum_out_det_data)
            {
                var out_det_data = await _dbContext.Query<SP_OUTSTORE_DET>(x => x.OUT_ID == outId && x.SP_ID == sum_out_det_datum.SP_ID && string.IsNullOrWhiteSpace(x.STORE_ID)).ToListAsync();
                var all_store = await _dbContext.Query<SP_STORE>(x => x.SP_ID == sum_out_det_datum.SP_ID && house_id_list.Contains(x.HOUSE_ID))
                    .Select(x => new SP_STORE
                    {
                        STORE_ID = x.STORE_ID,
                        BATCH_CODE = x.BATCH_CODE,
                        IN_DATE = x.IN_DATE,
                        HOUSE_ID = x.HOUSE_ID,
                        HOUSE_NAME = x.HOUSE_NAME,
                        HOUSE_CODE = x.HOUSE_CODE,
                        STORE_NUM = x.STORE_NUM - (_dbContext.Query<SP_OUTSTORE>(so => so.AUDITING == "2" || so.OUT_ID == outId)
                            .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                            .Where((so, sod) => sod.STORE_ID == x.STORE_ID)
                            .Select((so, sod) => Sql.Sum(sod.OUT_NUM)).FirstOrDefault() ?? 0),
                        TAX_MONEY = x.TAX_MONEY - (_dbContext.Query<SP_OUTSTORE>(so => so.AUDITING == "2" || so.OUT_ID == outId)
                            .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                            .Where((so, sod) => sod.STORE_ID == x.STORE_ID)
                            .Select((so, sod) => Sql.Sum(sod.TAX_MONEY)).FirstOrDefault() ?? 0),
                        NOTAX_MONEY = x.NOTAX_MONEY - (_dbContext.Query<SP_OUTSTORE>(so => so.AUDITING == "2" || so.OUT_ID == outId)
                            .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                            .Where((so, sod) => sod.STORE_ID == x.STORE_ID)
                            .Select((so, sod) => Sql.Sum(sod.NOTAX_MONEY)).FirstOrDefault() ?? 0)
                    })
                    .ToListAsync();
                var has_error_store = all_store.Where(x => x.STORE_NUM < 0).Any();
                MessageException.ThrowIf(has_error_store, $"物资「{sum_out_det_datum.SP_NAME}」库存不足\n");
                var store = all_store.Where(x => x.STORE_NUM > 0).OrderBy(x => x.IN_DATE).ToList();

                int i = 0;
                foreach (var out_det_datum in out_det_data)
                {
                    decimal out_num = out_det_datum.OUT_NUM.Value;
                    _dbContext.TrackEntity(out_det_datum);
                    while (out_num > 0 && i < store.Count)
                    {
                        var batch = store[i];

                        decimal cur_out_num = Math.Min(out_num, batch.STORE_NUM.Value);
                        out_num -= cur_out_num;
                        // 可变单价计算金额
                        decimal cur_out_tax_money = (cur_out_num == batch.STORE_NUM ? batch.TAX_MONEY : Math.Round((batch.TAX_MONEY * cur_out_num / batch.STORE_NUM).Value, 2)).Value;
                        decimal cur_out_notax_money = (cur_out_num == batch.STORE_NUM ? batch.NOTAX_MONEY : Math.Round((batch.NOTAX_MONEY * cur_out_num / batch.STORE_NUM).Value, 2)).Value;

                        out_det_datum.STORE_ID = batch.STORE_ID;
                        out_det_datum.BATCH_CODE = batch.BATCH_CODE;
                        out_det_datum.HOUSE_ID = batch.HOUSE_ID;
                        out_det_datum.HOUSE_NAME = batch.HOUSE_NAME;
                        out_det_datum.HOUSE_CODE = batch.HOUSE_CODE;
                        out_det_datum.OUT_NUM = cur_out_num;
                        out_det_datum.TAX_MONEY = cur_out_tax_money;
                        out_det_datum.NOTAX_MONEY = cur_out_notax_money;
                        out_det_datum.MODIFYDATE = await _dbContext.GetSysdate();
                        out_det_datum.MODIFY_USERID = _userSession.UserID;
                        out_det_datum.MODIFYUSER = _userSession.UserName;
                        await _dbContext.InsertOrUpdateAsync(out_det_datum);

                        batch.STORE_NUM -= cur_out_num;
                        batch.TAX_MONEY -= cur_out_tax_money;
                        batch.NOTAX_MONEY -= cur_out_notax_money;
                        if (batch.STORE_NUM == 0) ++i;
                        else if (batch.STORE_NUM < 0) throw new MessageException("分配批次失败");

                        // 剩余未分配数量需要分配到其它批次上，故重新生成主键
                        out_det_datum.OUT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
                    }
                    MessageException.ThrowIf(out_num > 0 && i >= store.Count, "分配批次失败");
                }
            }
        }

        #endregion FIFO批次分配

        #region 扣库存与生成流水

        /// <summary>
        /// 提交完成后或审批完成后，根据分配好的批次扣除库存并生成流水
        /// </summary>
        private async Task Outstore(string outId)
        {
            var out_det_data = await _dbContext.Query<SP_OUTSTORE>(x => x.OUT_ID == outId)
                .InnerJoin<SP_OUTSTORE_DET>((so, sod) => so.OUT_ID == sod.OUT_ID)
                .Select((so, sod) => new
                {
                    so.OUT_CODE,
                    so.OUT_DATE,
                    so.OUT_TYPE,
                    so.OUT_DEPT_ID,
                    so.OUT_DEPT_NAME,
                    sod.OUT_DET_ID,
                    sod.STORE_ID,
                    sod.SP_ID,
                    sod.SP_NAME,
                    sod.OUT_NUM,
                    sod.TAX_MONEY,
                    sod.NOTAX_MONEY
                })
                .ToListAsync();

            // 重复出库验证
            {
                var outDetIdList = out_det_data.Select(x => x.OUT_DET_ID).ToList();
                var isRepeatedOut = await _dbContext.Query<SP_STORE_WATER>(x => outDetIdList.Contains(x.SRC_ID)).AnyAsync();
                MessageException.ThrowIf(isRepeatedOut, $"重复出库，请联系管理员\noutId='{outId}'");
            }

            foreach (var out_det_datum in out_det_data)
            {
                MessageException.ThrowIf(out_det_datum.STORE_ID.IsNullOrWhiteSpace(), $"物资「{out_det_datum.SP_NAME}」未分配批次");
                MessageException.ThrowIf(!out_det_datum.OUT_NUM.HasValue || out_det_datum.OUT_NUM <= 0, $"物资「{out_det_datum.SP_NAME}」出库数量需为正数");
                var batch = await _dbContext.QueryByKeyAsync<SP_STORE>(out_det_datum.STORE_ID);
                MessageException.ThrowIf(out_det_datum.OUT_NUM > batch.STORE_NUM, $"批次号「{batch.BATCH_CODE}」的物资「{out_det_datum.SP_NAME}」库存数量不足\n出库数量「{out_det_datum.OUT_NUM}」\n库存数量「{batch.STORE_NUM}」");
                _dbContext.TrackEntity(batch);

                // 可变单价计算出库金额
                decimal cur_out_num = out_det_datum.OUT_NUM.Value;
                decimal cur_out_tax_money = (cur_out_num == batch.STORE_NUM ? batch.TAX_MONEY : Math.Round((batch.TAX_MONEY * cur_out_num / batch.STORE_NUM).Value, 2)).Value;
                decimal cur_out_notax_money = (cur_out_num == batch.STORE_NUM ? batch.NOTAX_MONEY : Math.Round((batch.NOTAX_MONEY * cur_out_num / batch.STORE_NUM).Value, 2)).Value;

                // 生成流水
                var water = new SP_STORE_WATER();
                water.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                water.STORE_ID = batch.STORE_ID;
                water.SRC_TYPE = out_det_datum.OUT_TYPE;
                water.SRC_ID = out_det_datum.OUT_DET_ID;
                water.SRC_CODE = out_det_datum.OUT_CODE;
                water.SRC_DATE = out_det_datum.OUT_DATE;
                water.INIT_NUM = batch.STORE_NUM;
                water.INIT_TAX_MONEY = batch.TAX_MONEY;
                water.INIT_NOTAX_MONEY = batch.NOTAX_MONEY;
                water.IN_NUM = 0;
                water.IN_TAX_MONEY = 0;
                water.IN_NOTAX_MONEY = 0;
                water.OUT_NUM = out_det_datum.OUT_NUM;
                water.OUT_TAX_MONEY = out_det_datum.TAX_MONEY;
                water.OUT_NOTAX_MONEY = out_det_datum.NOTAX_MONEY;
                water.CUR_NUM = batch.STORE_NUM - out_det_datum.OUT_NUM;
                water.CUR_TAX_MONEY = batch.TAX_MONEY - out_det_datum.TAX_MONEY;
                water.CUR_NOTAX_MONEY = batch.NOTAX_MONEY - out_det_datum.NOTAX_MONEY;
                water.WATER_DATE = await _dbContext.GetSysdate();
                water.DEPT_ID = out_det_datum.OUT_DEPT_ID;
                water.DEPT_NAME = out_det_datum.OUT_DEPT_NAME;
                await _dbContext.UseTransactionAsync(async () =>
                {
                    // 插入流水记录
                    var detRequest = new SaveRequest<SP_STORE_WATER> { Added = new List<SP_STORE_WATER> { water } };
                    await _spStoreService.DetSaveAsync(detRequest);

                    // 更新库存
                    batch.STORE_NUM -= cur_out_num;
                    batch.TAX_MONEY -= cur_out_tax_money;
                    batch.NOTAX_MONEY -= cur_out_notax_money;
                    await _dbContext.UpdateAsync(batch);

                    // 更新出库明细中的出库金额
                    await _dbContext.UpdateAsync<SP_OUTSTORE_DET>(x => x.OUT_DET_ID == out_det_datum.OUT_DET_ID, x => new SP_OUTSTORE_DET
                    {
                        TAX_MONEY = cur_out_tax_money,
                        NOTAX_MONEY = cur_out_notax_money
                    });
                });
            }
        }

        #endregion 扣库存与生成流水

        #region 出库冲红

        /// <summary>
        /// 出库冲红
        /// </summary>
        public async Task OutRedAsync(string outId)
        {
            var sp_out = await _dbContext.QueryByKeyAsync<SP_OUTSTORE>(outId);
            MessageException.ThrowIf(sp_out == null, "出库单不存在");
            await _dbContext.UseTransactionAsync(async () =>
            {
                _dbContext.TrackEntity(sp_out);
                sp_out.IS_RED = "1";
                sp_out.RED_USERID = _userSession.UserID.ToString();
                sp_out.RED_USER = _userSession.RealName;
                sp_out.RED_DATE = await _dbContext.GetSysdate();
                await _dbContext.UpdateAsync(sp_out);

                var src_type = await _dbContext.Query<BC_CODE>().Where(c => c.CODE_CN == "出库冲红" && c.CODE_TYPE == "src_type").Select(c => c.CODE_EN).FirstAsync();
                var sp_out_det_data = await _dbContext.Query<SP_OUTSTORE_DET>().Where(c => c.OUT_ID == sp_out.OUT_ID).ToListAsync();

                foreach (var sp_out_det_datum in sp_out_det_data)
                {
                    var store = await _dbContext.QueryByKeyAsync<SP_STORE>(sp_out_det_datum.STORE_ID);

                    // 生成冲红流水
                    var water = new SP_STORE_WATER();
                    water.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
                    water.STORE_ID = store.STORE_ID;
                    water.SRC_TYPE = src_type;
                    water.SRC_ID = sp_out_det_datum.OUT_DET_ID;
                    water.SRC_CODE = sp_out.OUT_CODE;
                    water.SRC_DATE = sp_out.RED_DATE;
                    water.INIT_NUM = store.STORE_NUM;
                    water.INIT_TAX_MONEY = store.TAX_MONEY;
                    water.INIT_NOTAX_MONEY = store.NOTAX_MONEY;
                    water.IN_NUM = sp_out_det_datum.OUT_NUM ?? 0;
                    water.IN_TAX_MONEY = sp_out_det_datum.TAX_MONEY ?? 0;
                    water.IN_NOTAX_MONEY = sp_out_det_datum.NOTAX_MONEY ?? 0;
                    water.OUT_NUM = 0;
                    water.OUT_TAX_MONEY = 0;
                    water.OUT_NOTAX_MONEY = 0;
                    water.CUR_NUM = store.STORE_NUM + sp_out_det_datum.OUT_NUM ?? 0;
                    water.CUR_TAX_MONEY = store.TAX_MONEY + sp_out_det_datum.TAX_MONEY ?? 0;
                    water.CUR_NOTAX_MONEY = store.NOTAX_MONEY + sp_out_det_datum.NOTAX_MONEY ?? 0;
                    water.WATER_DATE = await _dbContext.GetSysdate();
                    water.DEPT_ID = sp_out.OUT_DEPT_ID;
                    water.DEPT_NAME = sp_out.OUT_DEPT_NAME;
                    var detRequest = new SaveRequest<SP_STORE_WATER> { Added = new List<SP_STORE_WATER> { water } };
                    await _spStoreService.DetSaveAsync(detRequest);

                    // 修改库存（冲红为入库）
                    store.STORE_NUM = store.STORE_NUM + sp_out_det_datum.OUT_NUM;
                    store.TAX_MONEY = store.TAX_MONEY + sp_out_det_datum.TAX_MONEY;
                    store.NOTAX_MONEY = store.NOTAX_MONEY + sp_out_det_datum.NOTAX_MONEY;
                    var request = new SaveRequest<SP_STORE>
                    {
                        Original = new List<SP_STORE> { new SP_STORE { STORE_ID = store.STORE_ID } },
                        Updated = new List<SP_STORE> { store }
                    };
                    await _spStoreService.SaveAsync(request);
                }
            });
        }

        #endregion 出库冲红

        #region 冲红管理

        /// <summary>
        /// 获取冲红记录列表
        /// </summary>
        public async Task<GridData> OutBackListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUT_BACK>()
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.BACK_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取冲红记录详情
        /// </summary>
        public async Task<SP_OUT_BACK> OutBackDetailAsync(string id)
        {
            return await _dbContext.QueryByKeyAsync<SP_OUT_BACK>(id);
        }

        /// <summary>
        /// 保存冲红记录
        /// </summary>
        public async Task<AjaxResult> SaveOutBackAsync(SaveRequest<SP_OUT_BACK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.BACK_DATE, c.MEMO, c.OUT_BACK_ID },
                c => a => a.OUT_BACK_ID == c.OUT_BACK_ID);
        }

        /// <summary>
        /// 管理冲红导入
        /// </summary>
        public async Task<AjaxResult> ManageOutBackAsync(List<SP_OUTSTORE> request)
        {
            try
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    var outlist = request.Select(x => x.OUT_ID).ToList();
                    var outBackDets = _dbContext.Query<SP_OUTSTORE>().ToList();
                    var request2 = outBackDets
                        .Where(x => outlist.Contains(x.OUT_ID))
                        .Select(x => x.MapTo<SP_OUT_BACK>())
                        .ToList();

                    var outDic = new Dictionary<string, string>();
                    foreach (var req in request2)
                    {
                        req.BACK_DATE = await _dbContext.GetSysdate();
                        req.BACK_CODE = await _codeCreatorService.CreateCodeAsync<SP_OUT_BACK>("CH", a => a.BACK_CODE);
                        req.OUT_BACK_ID = GuidHelper.NewSnowflakeId().ToString();
                        req.MEMO = "";
                        outDic[req.OUT_ID] = req.OUT_BACK_ID;
                    }
                    await _dbContext.InsertRangeAsync(request2);
                    if (request2.Count > 0)
                    {
                        var keylist = request2.Select(x => x.OUT_ID).ToList();
                        var outstoreDets = _dbContext.Query<SP_OUTSTORE_DET>().ToList();
                        var spoutbackdets = outstoreDets
                            .Where(x => keylist.Contains(x.OUT_ID))
                            .Select(x => x.MapTo<SP_OUTBACK_DET>())
                            .ToList();
                        foreach (var spoutbackdet in spoutbackdets)
                        {
                            spoutbackdet.OUTDET_ID = GuidHelper.NewSnowflakeId().ToString();
                            spoutbackdet.OUT_BACK_ID = outDic[spoutbackdet.OUT_ID];
                        }
                        await _dbContext.InsertRangeAsync(spoutbackdets);
                    }
                });
                return AjaxResult.Success("保存成功");
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("保存失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 提交冲红
        /// </summary>
        public async Task<int> SubmitOutBackAsync(string sid)
        {
            var qryback = await _dbContext.Query<SP_OUT_BACK>(c => c.OUT_BACK_ID == sid).FirstOrDefaultAsync();
            var qrybackdets = await _dbContext.Query<SP_OUTBACK_DET>(c => c.OUT_BACK_ID == sid).ToListAsync();
            var backstoreDetIds = qrybackdets.Select(q => q.STORE_ID).ToList();
            var qryStores = await _dbContext.Query<SP_STORE>()
                .Where(s => backstoreDetIds.Contains(s.STORE_ID))
                .ToListAsync();
            var qryWaters = await _dbContext.Query<SP_STORE_WATER>()
                .Where(s => backstoreDetIds.Contains(s.STORE_ID))
                .ToListAsync();

            foreach (var qrybackdet in qrybackdets)
            {
                var qrystore = qryStores.FirstOrDefault(s => s.STORE_ID == qrybackdet.STORE_ID);
                if (qrystore != null)
                {
                    var chnum = qrystore.STORE_NUM + qrybackdet.COUNT;
                    var chmoney = chnum * qrystore.TAX_PRICE;
                    var chnomoney = chnum * qrystore.NOTAX_PRICE;
                    await _dbContext.UpdateAsync<SP_STORE>(x => x.STORE_ID == qrybackdet.STORE_ID,
                        x => new SP_STORE
                        {
                            STORE_NUM = chnum,
                            TAX_MONEY = chmoney,
                            NOTAX_MONEY = chnomoney,
                        });

                    var qryWater = qryWaters.FirstOrDefault(s =>
                        s.STORE_ID == qrybackdet.STORE_ID && s.SRC_CODE == qryback.OUT_CODE);
                    if (qryWater != null)
                    {
                        await _dbContext.UpdateAsync<SP_OUTSTORE>(x => x.OUT_CODE == qryback.OUT_CODE,
                            x => new SP_OUTSTORE { IS_RED = "1" });
                    }
                }
            }

            return await _dbContext.UpdateAsync<SP_OUT_BACK>(x => sid == x.OUT_BACK_ID,
                x => new SP_OUT_BACK { AUDITING = "1" });
        }

        /// <summary>
        /// 撤销冲红
        /// </summary>
        public async Task<int> UnSubmitOutBackAsync(string sid)
        {
            return await _dbContext.UpdateAsync<SP_OUT_BACK>(x => sid == x.OUT_BACK_ID,
                x => new SP_OUT_BACK { AUDITING = "0" });
        }

        /// <summary>
        /// 导入冲红（已提交的出库单列表）
        /// </summary>
        public async Task<GridData> ImportOutBackListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE>()
                .Where(c => c.AUDITING == "1")
                .GetGridData(request);
        }

        /// <summary>
        /// 获取冲红明细列表
        /// </summary>
        public async Task<GridData> GetOutBackDetListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTBACK_DET>()
                .GetGridData(request);
        }

        #endregion 冲红管理

        #region 出库明细汇总

        /// <summary>
        /// 获取出库明细汇总记录
        /// </summary>
        public async Task<GridData> GetOutStoreDetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_OUTSTORE_DET>()
                .InnerJoin<SP_OUTSTORE>((a, b) => a.OUT_ID == b.OUT_ID)
                .Where((a, b) => b.AUDITING == "1")
                .Select((a, b) => new
                {
                    b.OUT_CODE,
                    b.OUT_DATE,
                    b.OUT_USER,
                    b.OUT_DEPT_NAME,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.TYPE_NAME,
                    a.OUT_NUM,
                    a.TAX_MONEY,
                    a.NOTAX_MONEY,
                    a.HOUSE_NAME,
                    a.HOUSE_CODE,
                    a.BATCH_CODE,
                    a.DEVICE_NAME,
                    a.DEVICE_NO,
                    b.OUT_TYPE,
                    b.PURPOSE,
                    b.MEMO,
                    a.STORE_ID,
                    a.OUT_DET_ID,
                    a.OUT_ID,
                })
                .GetGridData(request);
        }

        #endregion 出库明细汇总
    }
}
