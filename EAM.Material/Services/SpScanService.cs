using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpScanService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;

        public SpScanService(IDbContext dbContext, IComboxDataService comboxDataService, ICodeCreatorService codeCreatorService, UserSession userSession, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;
            _corpService = corpService;
        }

        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode@#Auditing", "auditing" },
                    { "BCCode@#ScanStatus", "scan_status" },
                    { "BCCode@#ScanType", "scan_type" },
                    { "BCCode@#StoreSrc", "store_src" },
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)( x => true ) },
                    { "SpTypeName", (Expression<Func<BASE_SPTYPE, bool>>)( x => true ) }
                });
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());
                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        public async Task<SP_SCAN> GetAsync(string scanId)
        {
            return await _dbContext.Query<SP_SCAN>().FirstOrDefaultAsync(c => c.SCAN_ID == scanId);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_SCAN>().GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_SCAN> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING_LOSS,
                    c.AUDITING_MORE,
                    c.AUDITING,
                    c.SCAN_CODE,
                    c.SCAN_STATUS,
                    c.SCAN_TYPE,
                    c.HOUSE_CODE,
                    c.HOUSE_NAME,
                    c.HOUSE_ID,
                    c.STOCK_CODE,
                    c.STOCK_NAME,
                    c.STOCK_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.SCAN_DATE,
                    c.SCAN_USER,
                    c.SCAN_USERID,
                    c.MEMO,
                    c.MORE_CODE,
                    c.MORE_DATE,
                    c.MORE_REASON,
                    c.MORE_MEMO,
                    c.LOSS_CODE,
                    c.LOSS_DATE,
                    c.LOSS_REASON,
                    c.LOSS_MEMO,
                    c.SCAN_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.PDAUDIT,
                    c.SCAN_RESULT,
                    c.CY_MONEY
                },
                c => a => a.SCAN_ID == c.SCAN_ID, BeforeAdd, BeforeUpdate, BeforeDelete);

            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].SCAN_ID;

            return AjaxResult.Success(id);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        private async Task BeforeAdd(SP_SCAN entity)
        {
            var sysdate = await _dbContext.GetSysdate();
            //生成主键
            if (entity.SCAN_ID.IsNullOrWhiteSpace())
            {
                entity.SCAN_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //生成单号
            if (entity.SCAN_CODE.IsNullOrWhiteSpace())
            {
                entity.SCAN_CODE = await _codeCreatorService.CreateCodeAsync<SP_SCAN>("BP", a => a.SCAN_CODE);
            }
            //盘点日期
            if (!entity.SCAN_DATE.HasValue)
            {
                entity.SCAN_DATE = sysdate;
            }
            //盘点人
            if (entity.SCAN_USERID.IsNullOrWhiteSpace())
            {
                entity.SCAN_USERID = _userSession.UserID.ToString();
                entity.SCAN_USER = _userSession.RealName;
            }
            //盘点部门
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.CName;
            }

            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysdate;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        private async Task BeforeUpdate(SP_SCAN entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        private async Task BeforeDelete(SP_SCAN entity)
        {
            await _dbContext.DeleteAsync<SP_SCAN_DET>(x => x.SCAN_ID == entity.SCAN_ID);
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task SubmitAsync(string scanId)
        {
            //更新记录状态
            await _dbContext.UpdateAsync<SP_SCAN>(x => x.SCAN_ID == scanId, x => new SP_SCAN
            {
                AUDITING = "1"
            });
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        public async Task RevokeAsync(string scanId)
        {
            //更新记录状态
            await _dbContext.UpdateAsync<SP_SCAN>(x => x.SCAN_ID == scanId, x => new SP_SCAN
            {
                AUDITING = "0"
            });
        }

        /// <summary>
        /// 获取盘点明细列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_SCAN_DET>().GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_SCAN_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.SRC_TYPE,
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
                    c.MORE_NUM,
                    c.LOSS_NUM,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.IS_DISPOSE,
                    c.SCAN_RESULT,
                    c.IS_NEW,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.MORE_MEMO,
                    c.LOSS_MEMO,
                    c.SCAN_DET_ID,
                    c.SCAN_ID,
                    c.SP_ID,
                    c.STORE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.STORE_CODE,
                    c.CY_NUM,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.PDNOTAX_MONEY,
                    c.CY_MONEY
                },
                c => a => a.SCAN_DET_ID == c.SCAN_DET_ID,
                BeforeAddDet, BeforeUpdateDet, null, orgin: true);
        }

        /// <summary>
        /// 新增明细前
        /// </summary>
        private async Task BeforeAddDet(SP_SCAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.AUDITING = "0";
            await HandleDet(entity);
        }

        /// <summary>
        /// 更新明细前
        /// </summary>
        private async Task BeforeUpdateDet(SP_SCAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            await HandleDet(entity);
        }

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task HandleDet(SP_SCAN_DET entity)
        {
            //判断盘盈盘亏情况
            if (entity.SCAN_NUM == entity.STORE_NUM)
            {
                entity.SCAN_RESULT = "正常";
            }
            else if (entity.SCAN_NUM > entity.STORE_NUM)
            {
                entity.SCAN_RESULT = "盘盈";
            }
            else if (entity.SCAN_NUM < entity.STORE_NUM)
            {
                entity.SCAN_RESULT = "盘亏";
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        /// <param name="request">物资盘点</param>
        /// <param name="requestDet">物资盘点明细</param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAllAsync
            (SaveRequest<SP_SCAN> request, SaveRequest<SP_SCAN_DET> requestDet)
        {
            string scan_id;
            //填写主子表关联键值
            if (request.Updated.Any() && !request.Updated.First().SCAN_ID.IsNullOrWhiteSpace())
            {
                scan_id = request.Updated.First().SCAN_ID;
            }
            else if (request.Added.Any() && !request.Added.First().SCAN_ID.IsNullOrWhiteSpace())
            {
                scan_id = request.Added.First().SCAN_ID;
            }
            else scan_id = GuidHelper.NewSnowflakeId().ToString();
            if (request.Added.Any() && request.Added.First().SCAN_ID.IsNullOrWhiteSpace())
            {
                request.Added[0].SCAN_ID = scan_id;
            }

            foreach (var entity in requestDet.Added ??= new List<SP_SCAN_DET>())
            {
                if (entity.SCAN_ID.IsNullOrWhiteSpace()) entity.SCAN_ID = scan_id;
            }

            //启用事务保存所有表
            try
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    if ((await SaveAsync(request)).IsError)
                    {
                        throw new MessageException("物资盘点保存失败");
                    }
                    if ((await DetSaveAsync(requestDet)).IsError)
                    {
                        throw new MessageException("物资盘点明细保存失败");
                    }
                });
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.Message);
            }

            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 生成盘点明细
        /// </summary>
        public async Task GenerateDet(string scanId)
        {
            var scan = _dbContext.QueryByKey<SP_SCAN>(scanId);
            MessageException.ThrowIf(scan == null, "没有对应申请单");
            var store_query = _dbContext.Query<SP_STORE>();
            if (!scan.TYPE_ID.IsNullOrWhiteSpace())
            {
                var type_code = (await _dbContext.Query<BASE_SPTYPE>(x => x.TYPE_ID == scan.TYPE_ID).FirstOrDefaultAsync()).TYPE_CODE;
                store_query = store_query.Where(x => _dbContext.Query<BASE_SPTYPE>(type => type.TYPE_ID == x.TYPE_ID && type.TYPE_CODE.StartsWith(type_code)).Any());
            }
            if (!scan.STOCK_ID.IsNullOrWhiteSpace())
            {
                var house_code = (await _dbContext.Query<SP_HOUSE>(x => x.HOUSE_ID == scan.STOCK_ID).FirstOrDefaultAsync()).HOUSE_CODE;
                store_query = store_query.Where(x => _dbContext.Query<SP_HOUSE>(house => house.HOUSE_ID == x.STOCK_ID && house.HOUSE_CODE.StartsWith(house_code)).Any());
            }
            var store_data = await store_query.ToListAsync();
            MessageException.ThrowIf(!store_data.Any(), "没有对应库存清单");

            //删除已有明细
            await _dbContext.DeleteAsync<SP_SCAN_DET>(x => x.SCAN_ID == scanId);
            var sysdate = await _dbContext.GetSysdate();
            var importList = new List<SP_SCAN_DET>();
            foreach (var store in store_data)
            {
                var scan_det = store.MapTo<SP_SCAN_DET>();
                scan_det.SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
                scan_det.CREATE_USERID = _userSession.UserID.ToString();
                scan_det.CREATEDATE = sysdate;
                scan_det.MODIFY_USERID = _userSession.UserID.ToString();
                scan_det.MODIFYDATE = sysdate;
                scan_det.SCAN_ID = scanId;
                scan_det.STORE_NUM = store.NUM;
                scan_det.AUDITING = "0";
                importList.Add(scan_det);
            }
            await _dbContext.InsertRangeAsync(importList);
        }

        class SpScanDetRes : SP_SCAN_DET
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public string SCAN_CODE;
            public string SCAN_TYPE;
            public string SCAN_USER;
            public string DEPT_NAME;
            public string SEC_DEPT;
            public DateTime? SCAN_DATE;
        }

        /// <summary>
        /// 待盘点项目
        /// </summary>
        public async Task<GridData> DetailAnsListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_SCAN>(a => a.AUDITING == "1")
                .LeftJoin<SP_SCAN_DET>((a, b) => a.SCAN_ID == b.SCAN_ID)
                .Where((a, b) => b.AUDITING == "0")
                .Select((a, b) => new SpScanDetRes
                {
                    AUDITING = b.AUDITING,
                    SRC_TYPE = b.SRC_TYPE,
                    SP_CODE = b.SP_CODE,
                    SP_NAME = b.SP_NAME,
                    SP_SIZE = b.SP_SIZE,
                    PRODUCE = b.PRODUCE,
                    UNIT = b.UNIT,
                    STORE_NUM = b.STORE_NUM,
                    SCAN_NUM = b.SCAN_NUM,
                    STOCK_NAME = b.STOCK_NAME,
                    TYPE_NAME = b.TYPE_NAME,
                    SCAN_RESULT = b.SCAN_RESULT,
                    SCAN_DET_ID = b.SCAN_DET_ID,
                    SCAN_ID = b.SCAN_ID,
                    CREATEDATE = b.CREATEDATE,
                    STORE_CODE = b.STORE_CODE,
                    SCAN_CODE = a.SCAN_CODE,
                    SCAN_TYPE = a.SCAN_TYPE,
                    SCAN_USER = a.SCAN_USER,
                    DEPT_NAME = a.DEPT_NAME,
                    SEC_DEPT = a.SEC_DEPT,
                    SCAN_DATE = a.SCAN_DATE
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 盘点项目提交
        /// </summary>
        public async Task DetSubmit(List<string> sids)
        {
            var list = _dbContext.Query<SP_SCAN_DET>().Where(x => sids.Contains(x.SCAN_DET_ID)).ToList();
            foreach (var item in list)
            {
                _dbContext.TrackEntity(item);
                item.AUDITING = "1";
                item.MORE_NUM = item.SCAN_NUM - item.STORE_NUM;
                item.IS_DISPOSE = "1";
                _dbContext.Update(item);
            }

            var scanids = list.Select(t => t.SCAN_ID).Distinct().ToList();
            foreach (var id in scanids)
            {
                var isIncomplete = await _dbContext.Query<SP_SCAN_DET>().Where(x => x.SCAN_ID == id && x.AUDITING == "0").AnyAsync();
                //所有盘点项目均提交后，更新盘点任务状态
                if (!isIncomplete)
                {
                    await _dbContext.UpdateAsync<SP_SCAN>(x => x.SCAN_ID == id, x => new SP_SCAN
                    {
                        SCAN_STATUS = "3" //盘点完成
                    });
                }
            }
        }
    }
}
