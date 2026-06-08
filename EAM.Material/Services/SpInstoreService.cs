using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Core.Interfaces.Material;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpInstoreService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private readonly ISpStoreService _spStoreService;

        public SpInstoreService(IDbContext dbContext, ICodeCreatorService codeCreatorService, IComboxDataService comboxDataService, UserSession userSession, ISpStoreService spStoreService)
        {
            _dbContext = dbContext;
            _codeCreatorService = codeCreatorService;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
            _spStoreService = spStoreService;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode@#Auditing", "auditing" },
                    { "SpHouse", (Expression<Func<SP_HOUSE, bool>>)(x => x.AUDITING == "1") }
                });
                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_INSTORE>().GetGridData(request);
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string inId)
        {
            var query = await _dbContext.Query<SP_INSTORE>()
                .Where(c => c.IN_ID == inId)
                .FirstOrDefaultAsync();
            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 获取明细列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_INSTORE_DET>().GetGridData(request);
        }

        /// <summary>
        /// 获取可导入的采购物资明细
        /// </summary>
        public async Task<GridData> ImportListAsync(GridRequest request)
        {
            //查询已审批通过的采购单中的物资明细
            var query = _dbContext.Query<SP_COLLECT_REQUEST>()
                .InnerJoin<SP_COLLECT>((cr, c) => cr.COLLECT_ID == c.COLLECT_ID)
                .Where((cr, c) => c.AUDITING == "3");

            //使用子查询获取每种物资已验收入库的总数，筛选未全部入库的物资
            return await query
                .Select((cr, c) => new
                {
                    cr.COLLECT_REQUEST_ID,
                    cr.COLLECT_ID,
                    cr.REQUEST_DET_ID,
                    c.COLLECT_CODE,
                    cr.SP_ID,
                    cr.SP_CODE,
                    cr.SP_NAME,
                    cr.SP_SIZE,
                    cr.PRODUCE,
                    cr.UNIT,
                    cr.TYPE_ID,
                    cr.TYPE_NAME,
                    cr.REQUEST_NUM,
                    cr.TAX_PRICE,
                    cr.NOTAX_PRICE,
                    cr.TAX_MONEY,
                    cr.NOTAX_MONEY,
                    cr.DEPT_ID,
                    cr.DEPT_NAME,
                    c.HOUSE_ID,
                    c.HOUSE_NAME,
                    c.HOUSE_CODE,
                    c.PROVIDER_NAME
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestDet)
        {
            string in_id;
            //填写主子表关联键值
            if (request.Updated != null && request.Updated.Any() && !request.Updated.First().IN_ID.IsNullOrWhiteSpace())
            {
                in_id = request.Updated.First().IN_ID;
            }
            else if (request.Added != null && request.Added.Any() && !request.Added.First().IN_ID.IsNullOrWhiteSpace())
            {
                in_id = request.Added.First().IN_ID;
            }
            else
            {
                in_id = GuidHelper.NewSnowflakeId().ToString();
            }
            if (request.Added != null && request.Added.Any() && request.Added.First().IN_ID.IsNullOrWhiteSpace())
            {
                request.Added[0].IN_ID = in_id;
            }

            foreach (var entity in requestDet.Added ??= new List<SP_INSTORE_DET>())
            {
                if (entity.IN_ID.IsNullOrWhiteSpace()) entity.IN_ID = in_id;
            }

            try
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    if ((await SaveAsync(request)).IsError)
                    {
                        throw new MessageException("验收入库保存失败");
                    }
                    if ((await DetSaveAsync(requestDet)).IsError)
                    {
                        throw new MessageException("验收入库明细保存失败");
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
        /// 保存主表
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_INSTORE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.IN_CODE,
                    c.IN_DATE,
                    c.PROVIDER_NAME,
                    c.IN_USERID,
                    c.IN_USER,
                    c.CHK_USERID,
                    c.CHK_USER,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.MEMO,
                    c.CONSIGNEE_ID,
                    c.CONSIGNEE,
                    c.IN_ID
                },
                c => a => a.IN_ID == c.IN_ID,
                BeforeAdd, BeforeUpdate, BeforeDelete, orgin: true);
        }

        /// <summary>
        /// 保存子表
        /// </summary>
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_INSTORE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.IN_ID,
                    c.COLLECT_REQUEST_ID,
                    c.REQUEST_DET_ID,
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.UNIT,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.HOUSE_ID,
                    c.HOUSE_NAME,
                    c.HOUSE_CODE,
                    c.IN_NUM,
                    c.TAX_RATE,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.IN_DET_ID,
                },
                c => a => a.IN_DET_ID == c.IN_DET_ID,
                BeforeAddDet, BeforeUpdateDet, null, orgin: true);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        private async Task BeforeAdd(SP_INSTORE entity)
        {
            if (entity.IN_ID.IsNullOrWhiteSpace())
            {
                entity.IN_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.IN_CODE.IsNullOrWhiteSpace())
            {
                entity.IN_CODE = await _codeCreatorService.CreateCodeAsync<SP_INSTORE>("YK", a => a.IN_CODE);
            }
            if (!entity.IN_DATE.HasValue)
            {
                entity.IN_DATE = await _dbContext.GetSysdate();
            }
            if (entity.IN_USERID.IsNullOrWhiteSpace())
            {
                entity.IN_USERID = _userSession.UserID.ToString();
                entity.IN_USER = _userSession.RealName;
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.SName;
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
        }

        /// <summary>
        /// 更新前
        /// </summary>
        private async Task BeforeUpdate(SP_INSTORE entity)
        {
        }

        /// <summary>
        /// 删除前
        /// </summary>
        private async Task BeforeDelete(SP_INSTORE entity)
        {
            await _dbContext.DeleteAsync<SP_INSTORE_DET>(x => x.IN_ID == entity.IN_ID);
        }

        /// <summary>
        /// 新增明细前
        /// </summary>
        private async Task BeforeAddDet(SP_INSTORE_DET entity)
        {
            if (entity.IN_DET_ID.IsNullOrWhiteSpace())
            {
                entity.IN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
        }

        /// <summary>
        /// 更新明细前
        /// </summary>
        private async Task BeforeUpdateDet(SP_INSTORE_DET entity)
        {
        }

        /// <summary>
        /// 提交验收入库
        /// </summary>
        public async Task SubmitAsync(string inId)
        {
            var sp_instore = await _dbContext.QueryByKeyAsync<SP_INSTORE>(inId);
            MessageException.ThrowIf(sp_instore == null, "验收入库单不存在");
            MessageException.ThrowIf(new[] { "1", "2", "3" }.Contains(sp_instore.AUDITING), "验收入库单已提交");
            MessageException.ThrowIf(!sp_instore.IN_DATE.HasValue, "入库日期未填写");

            var det_list = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == inId).ToListAsync();
            MessageException.ThrowIf(det_list.Count == 0, "验收入库明细为空");

            //校验验收数量：每种物资的验收数量不得超过对应的采购申请数量减去已验收入库数量
            foreach (var det in det_list)
            {
                MessageException.ThrowIf(!det.IN_NUM.HasValue || det.IN_NUM <= 0, $"物资「{det.SP_NAME}」的入库数量必须大于零");

                if (!det.COLLECT_REQUEST_ID.IsNullOrWhiteSpace())
                {
                    //获取该采购需求明细的申请数量
                    var collect_request = await _dbContext.QueryByKeyAsync<SP_COLLECT_REQUEST>(det.COLLECT_REQUEST_ID);
                    if (collect_request != null)
                    {
                        //获取已提交的验收入库单ID
                        var submitted_in_ids = await _dbContext.Query<SP_INSTORE>()
                            .Where(i => i.AUDITING == "1")
                            .Select(i => i.IN_ID)
                            .ToListAsync();
                        //计算该采购需求明细已验收入库的总数量
                        var instored_num = await _dbContext.Query<SP_INSTORE_DET>()
                            .Where(d => d.COLLECT_REQUEST_ID == det.COLLECT_REQUEST_ID && submitted_in_ids.Contains(d.IN_ID))
                            .SumAsync(d => d.IN_NUM) ?? 0;
                        var remain_num = (collect_request.REQUEST_NUM ?? 0) - instored_num;
                        MessageException.ThrowIf(det.IN_NUM > remain_num, $"物资「{det.SP_NAME}」的入库数量「{det.IN_NUM}」超过剩余可入库数量「{remain_num}」");
                    }
                }
            }

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var det in det_list)
                {
                    //每次入库的每样物资，视为独立的新批次，生成独立的SP_STORE和SP_STORE_WATER记录
                    await CreateStoreAndWaterAsync(sp_instore, det);
                }
                //更新记录状态
                await _dbContext.UpdateAsync<SP_INSTORE>(x => x.IN_ID == inId, x => new SP_INSTORE
                {
                    AUDITING = "1"
                });
            });
        }

        /// <summary>
        /// 生成库存和流水记录
        /// </summary>
        private async Task CreateStoreAndWaterAsync(SP_INSTORE sp_instore, SP_INSTORE_DET det)
        {
            //生成库存记录
            var sp_store = new SP_STORE
            {
                STORE_ID = GuidHelper.NewSnowflakeId().ToString(),
                IN_DATE = sp_instore.IN_DATE,
                SP_ID = det.SP_ID,
                SP_CODE = det.SP_CODE,
                SP_NAME = det.SP_NAME,
                TYPE_ID = det.TYPE_ID,
                TYPE_NAME = det.TYPE_NAME,
                PRODUCE = det.PRODUCE,
                SP_SIZE = det.SP_SIZE,
                UNIT = det.UNIT,
                STORE_NUM = det.IN_NUM,
                TAX_PRICE = det.TAX_PRICE,
                TAX_MONEY = det.TAX_MONEY,
                NOTAX_PRICE = det.NOTAX_PRICE,
                NOTAX_MONEY = det.NOTAX_MONEY,
                HOUSE_ID = det.HOUSE_ID,
                HOUSE_NAME = det.HOUSE_NAME,
                HOUSE_CODE = det.HOUSE_CODE,
                DEPT_ID = sp_instore.DEPT_ID,
                DEPT_NAME = sp_instore.DEPT_NAME,
                PROVIDER_NAME = sp_instore.PROVIDER_NAME
            };
            var storeRequest = new SaveRequest<SP_STORE>
            {
                Added = new List<SP_STORE> { sp_store }
            };

            //生成库存流水记录
            var sp_store_water = new SP_STORE_WATER
            {
                WATER_ID = GuidHelper.NewSnowflakeId().ToString(),
                STORE_ID = sp_store.STORE_ID,
                SRC_ID = det.IN_DET_ID,
                SRC_CODE = sp_instore.IN_CODE,
                SRC_DATE = sp_instore.IN_DATE,
                SRC_TYPE = "1",  //验收入库
                WATER_DATE = await _dbContext.GetSysdate(),
                INIT_NUM = 0,
                INIT_TAX_MONEY = 0,
                INIT_NOTAX_MONEY = 0,
                IN_NUM = det.IN_NUM,
                IN_TAX_MONEY = det.TAX_MONEY,
                IN_NOTAX_MONEY = det.NOTAX_MONEY,
                OUT_NUM = 0,
                OUT_TAX_MONEY = 0,
                OUT_NOTAX_MONEY = 0,
                CUR_NUM = det.IN_NUM,
                CUR_TAX_MONEY = det.TAX_MONEY,
                CUR_NOTAX_MONEY = det.NOTAX_MONEY,
                DEPT_ID = sp_instore.DEPT_ID,
                DEPT_NAME = sp_instore.DEPT_NAME
            };
            var waterRequest = new SaveRequest<SP_STORE_WATER>
            {
                Added = new List<SP_STORE_WATER> { sp_store_water }
            };

            await _spStoreService.SaveAsync(storeRequest);
            await _spStoreService.DetSaveAsync(waterRequest);
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        public async Task RevokeAsync(string inId)
        {
            var sp_instore = await _dbContext.QueryByKeyAsync<SP_INSTORE>(inId);
            MessageException.ThrowIf(sp_instore == null, "验收入库单不存在");
            MessageException.ThrowIf(sp_instore.AUDITING != "1", "只有已提交的单据才能撤销");

            var det_list = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == inId).ToListAsync();

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var det in det_list)
                {
                    //查找该入库明细生成的库存流水
                    var water = await _dbContext.Query<SP_STORE_WATER>(x => x.SRC_ID == det.IN_DET_ID).FirstOrDefaultAsync();
                    if (water == null) continue;

                    //检查是否有更新的流水
                    var hasNewWater = await _dbContext.Query<SP_STORE_WATER>(x => x.STORE_ID == water.STORE_ID && x.WATER_DATE > water.WATER_DATE).AnyAsync();
                    MessageException.ThrowIf(hasNewWater, $"物资「{det.SP_NAME}」的库存批次存在新流水，不允许撤销提交");

                    //删除流水
                    var waterDeleteRequest = new SaveRequest<SP_STORE_WATER>
                    {
                        Deleted = new List<SP_STORE_WATER> { water }
                    };
                    await _spStoreService.DetSaveAsync(waterDeleteRequest);

                    //删除库存记录
                    var store = await _dbContext.QueryByKeyAsync<SP_STORE>(water.STORE_ID);
                    if (store != null)
                    {
                        var storeDeleteRequest = new SaveRequest<SP_STORE>
                        {
                            Deleted = new List<SP_STORE> { store }
                        };
                        await _spStoreService.SaveAsync(storeDeleteRequest);
                    }
                }

                //更新记录状态
                await _dbContext.UpdateAsync<SP_INSTORE>(x => x.IN_ID == inId, x => new SP_INSTORE
                {
                    AUDITING = "0"
                });
            });
        }
    }
}
