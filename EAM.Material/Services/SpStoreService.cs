using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Core.Interfaces.Material;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Data;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpStoreService : ISpStoreService, IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly UserSession _userSession;

        public SpStoreService(IDbContext dbContext, IComboxDataService comboxDataService, ICodeCreatorService codeCreatorService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>().GetGridData(request);
        }

        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "store_src" },
                    { "ProviderName", (Expression<Func<PROVIDER, bool>>)null},
                    { "DeptData", (Expression<Func<CF_CORP, bool>>)(a => a.CORPID == _userSession.Corp.CorpID)}
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 保存库存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_STORE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.BATCH_CODE,
                    c.IN_DATE,
                    c.SP_ID,
                    c.SP_NAME,
                    c.SP_CODE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.PRODUCE,
                    c.SP_SIZE,
                    c.UNIT,
                    c.PROPERTY,
                    c.STORE_NUM,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.HOUSE_ID,
                    c.HOUSE_NAME,
                    c.HOUSE_CODE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.PROVIDER_NAME,
                    c.STORE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.STORE_ID == c.STORE_ID, BeforeAdd, BeforeUpdate);
        }

        /// <summary>
        /// 保存库存流水
        /// </summary>
        public async Task<AjaxResult> DetSaveAsync(SaveRequest<SP_STORE_WATER> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.WATER_ID,
                    c.STORE_ID,
                    c.SRC_ID,
                    c.SRC_CODE,
                    c.SRC_DATE,
                    c.SRC_TYPE,
                    c.WATER_DATE,
                    c.INIT_NUM,
                    c.INIT_TAX_MONEY,
                    c.INIT_NOTAX_MONEY,
                    c.IN_NUM,
                    c.IN_TAX_MONEY,
                    c.IN_NOTAX_MONEY,
                    c.OUT_NUM,
                    c.OUT_TAX_MONEY,
                    c.OUT_NOTAX_MONEY,
                    c.CUR_NUM,
                    c.CUR_TAX_MONEY,
                    c.CUR_NOTAX_MONEY,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.WATER_ID == c.WATER_ID, BeforeAddWater, BeforeUpdateWater);
        }

        /// <summary>
        /// 新增库存前处理
        /// </summary>
        private async Task BeforeAdd(SP_STORE entity)
        {
            if (entity.STORE_ID.IsNullOrWhiteSpace())
            {
                entity.STORE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.BATCH_CODE.IsNullOrWhiteSpace())
            {
                entity.BATCH_CODE = await _codeCreatorService.CreateCodeAsync<SP_STORE>("PC", a => a.BATCH_CODE);
            }
            if (!entity.IN_DATE.HasValue)
            {
                entity.IN_DATE = await _dbContext.GetSysdate();
            }
        }

        /// <summary>
        /// 更新库存前处理
        /// </summary>
        private async Task BeforeUpdate(SP_STORE entity)
        {
        }

        /// <summary>
        /// 新增流水前处理
        /// </summary>
        private async Task BeforeAddWater(SP_STORE_WATER entity)
        {
            if (entity.WATER_ID.IsNullOrWhiteSpace())
            {
                entity.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (!entity.WATER_DATE.HasValue)
            {
                entity.WATER_DATE = await _dbContext.GetSysdate();
            }
        }

        /// <summary>
        /// 更新流水前处理
        /// </summary>
        private async Task BeforeUpdateWater(SP_STORE_WATER entity)
        {
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<SP_HOUSE>().ToListAsync();
            var data = list.Select(c => new
            {
                c.HOUSE_CODE,
                c.HOUSE_NAME,
                c.HOUSE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PARENT_HOUSE_CODE) || c.PARENT_HOUSE_CODE == "0") ? "ROOT" : c.PARENT_HOUSE_CODE,
                ICON = "fa fa-group"
            }).OrderBy(c => c.HOUSE_CODE).ToList();
            data.Add(new { HOUSE_CODE = "ROOT", HOUSE_NAME = "仓库货位", HOUSE_ID = "ROOT", PARENTID = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }

        #region 库存预警

        public class StoreLimitReq
        {
            public string LIMIT_ID;
            public string STOCK_ID;
            /// <summary>
            /// 库存上限
            /// </summary>
            public decimal? STORE_TOP;

            /// <summary>
            /// 库存下限
            /// </summary>
            public decimal? STORE_LOWER;
            /// <summary>
            /// 物料编码
            /// </summary>
            public string SP_CODE;
            /// <summary>
            /// 物料名称
            /// </summary>
            public string SP_NAME;
            /// <summary>
            /// 规格型号
            /// </summary>
            public string SP_SIZE;
            /// <summary>
            /// 计量单位
            /// </summary>
            public string UNIT;
            /// <summary>
            /// 品牌、厂家
            /// </summary>
            public string PRODUCE;
            /// <summary>
            /// 库位名称
            /// </summary>
            public string STOCK_NAME;
            /// <summary>
            /// 单位名称
            /// </summary>
            public string FDEPT_NAME;
            /// <summary>
            /// 物料ID
            /// </summary>
            public string SP_ID;

            public decimal? NUM;
            public decimal? MONEY;
        }

        /// <summary>
        /// 仓库上下限设置
        /// </summary>
        public async Task<GridData> StoreSumListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                 .Select(t => new
                 {
                     t.SP_ID,
                     t.SP_CODE,
                     t.SP_NAME,
                     t.HOUSE_ID,
                     t.HOUSE_NAME,
                     t.SP_SIZE,
                     t.UNIT,
                     t.PRODUCE,
                     STORE_NUM = t.STORE_NUM ?? 0
                 })
                .GroupBy(t => new { t.SP_ID, t.SP_CODE, t.SP_NAME, t.HOUSE_ID, t.HOUSE_NAME, t.SP_SIZE, t.UNIT, t.PRODUCE })
                .Select(t => new
                {
                    t.SP_ID,
                    t.SP_CODE,
                    t.SP_NAME,
                    t.HOUSE_ID,
                    t.HOUSE_NAME,
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    STORE_NUM = Sql.Sum(t.STORE_NUM)
                }).GetGridData(request);
        }

        /// <summary>
        /// 库存上下限列表
        /// </summary>
        public async Task<GridData> LimitListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_LIMIT>()
                .Select(a => new StoreLimitReq
                {
                    SP_ID = a.SP_ID,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    STOCK_ID = a.STOCK_ID ?? "",
                    STOCK_NAME = a.STOCK_NAME,
                    LIMIT_ID = a.LIMIT_ID,
                    STORE_LOWER = a.STORE_LOWER,
                    STORE_TOP = a.STORE_TOP
                })
                .GetGridData(request);

            foreach (var item in (List<StoreLimitReq>)res.Rows)
            {
                var query = _dbContext.Query<SP_STORE>().Where(t => t.SP_ID == item.SP_ID);
                if (string.IsNullOrEmpty(item.STOCK_ID))
                {
                    query = query.Where(t => string.IsNullOrEmpty(item.STOCK_ID));
                }
                else
                {
                    query = query.Where(t => t.HOUSE_ID == item.STOCK_ID);
                }
                var data = query.GroupBy(t => new { t.SP_ID, t.HOUSE_ID })
                 .AndBy(t => t.SP_SIZE)
                  .AndBy(t => t.UNIT)
                   .AndBy(t => t.PRODUCE)
                .Select(t => new
                {
                    t.SP_ID,
                    t.HOUSE_ID,
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    STORE_NUM = Sql.Sum(t.STORE_NUM),
                    MONEY = Sql.Sum(t.TAX_MONEY)
                }).First();

                item.SP_SIZE = data?.SP_SIZE;
                item.UNIT = data?.UNIT;
                item.PRODUCE = data?.PRODUCE;
                item.NUM = data?.STORE_NUM;
                item.MONEY = data?.MONEY;
            }
            return res;
        }

        /// <summary>
        /// 仓库库存预警
        /// </summary>
        public async Task<GridData> StoreLimitListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                .GroupBy(t => new { t.SP_ID, t.HOUSE_ID, t.SP_SIZE, t.UNIT, t.PRODUCE })
                .Select(t => new
                {
                    t.SP_ID,
                    t.HOUSE_ID,
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    STORE_NUM = Sql.Sum(t.STORE_NUM),
                    MONEY = Sql.Sum(t.TAX_MONEY)
                })
                .LeftJoin<SP_LIMIT>((a, b) => a.SP_ID == b.SP_ID && a.HOUSE_ID == Case.When(string.IsNullOrEmpty(b.STOCK_ID)).Then("").Else(b.STOCK_ID))
                .Where((a, b) => (b.STORE_LOWER > 0 && a.STORE_NUM < b.STORE_LOWER) || (b.STORE_TOP > 0 && a.STORE_NUM > b.STORE_TOP))
                .Select((a, b) => new StoreLimitReq
                {
                    STORE_TOP = b.STORE_TOP,
                    STORE_LOWER = b.STORE_LOWER,
                    SP_CODE = b.SP_CODE,
                    SP_NAME = b.SP_NAME,
                    STOCK_NAME = b.STOCK_NAME,
                    FDEPT_NAME = b.FDEPT_NAME,
                    SP_ID = a.SP_ID,
                    NUM = a.STORE_NUM,
                    MONEY = a.MONEY,
                    SP_SIZE = a.SP_SIZE,
                    UNIT = a.UNIT,
                    PRODUCE = a.PRODUCE
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 预警保存
        /// </summary>
        public async Task<AjaxResult> LimitSaveAsync(SaveRequest<SP_LIMIT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.STORE_TOP,
                    c.STORE_LOWER,
                    c.TOTAL_TOP,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.STOCK_ID,
                    c.STOCK_NAME,
                    c.FDEPT_ID,
                    c.FDEPT_NAME,
                    c.SP_ID,
                    c.LIMIT_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.HOUSE_ID
                },
                c => a => a.LIMIT_ID == c.LIMIT_ID, LimitBeforeAdd, LimitBeforeUpdate);
        }

        /// <summary>
        /// 新增预警前处理
        /// </summary>
        private async Task LimitBeforeAdd(SP_LIMIT entity)
        {
            entity.LIMIT_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 更新预警前处理
        /// </summary>
        private async Task LimitBeforeUpdate(SP_LIMIT entity)
        {
        }

        /// <summary>
        /// 设置上下限
        /// </summary>
        public async Task<int> SetTopLower(string LIMITID, int? TOP, int? LOWER)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_LIMIT>(x => x.LIMIT_ID == LIMITID,
                    x => new SP_LIMIT
                    {
                        STORE_TOP = TOP,
                        STORE_LOWER = LOWER
                    });
            return updatedevice;
        }
        #endregion

        #region 库存报表

        /// <summary>
        /// 获取报表下拉框数据
        /// </summary>
        public async Task<AjaxResult> ReportComboxDataAsync()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BaseSpType",(Expression<Func<BASE_SPTYPE, bool>>)null },
                    { "BasePurtype",(Expression<Func<BASE_PURTYPE, bool>>)null },
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null}
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 库存定期查询
        /// </summary>
        public async Task<GridData> StoreSearchListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE_WATER>()
                 .Select(t => new
                 {
                     t.STORE_ID,
                     IN_NUM = t.IN_NUM ?? 0,
                     OUT_NUM = t.OUT_NUM ?? 0
                 })
                .GroupBy(t => new { t.STORE_ID })
                .Select(t => new
                {
                    t.STORE_ID,
                    STORE_NUM = Sql.Sum(t.IN_NUM) - Sql.Sum(t.OUT_NUM)
                })
                .LeftJoin<SP_STORE>((a, b) => a.STORE_ID == b.STORE_ID)
                .Select((a, b) => new
                {
                    b.STORE_ID,
                    a.STORE_NUM,
                    b.HOUSE_NAME,
                    b.HOUSE_ID,
                    b.PRODUCE,
                    b.TYPE_NAME,
                    b.TYPE_ID,
                    b.UNIT,
                    b.SP_SIZE,
                    b.SP_NAME,
                    b.SP_CODE,
                    b.IN_DATE,
                    b.CREATEDATE,
                    b.NOTAX_PRICE,
                    b.TAX_PRICE,
                    NOTAX_MONEY = a.STORE_NUM * b.NOTAX_PRICE,
                    TAX_MONEY = a.STORE_NUM * b.TAX_PRICE
                })
                .GetGridData(request);
        }

        public class StoreInOutReq
        {
            public string STORE_ID { get; set; }
            public string SP_CODE { get; set; }
            public string SP_NAME { get; set; }
            public string SP_SIZE { get; set; }
            public string TYPE_NAME { get; set; }
            public string PRODUCE { get; set; }
            public string HOUSE_NAME { get; set; }
            public string BATCH_CODE { get; set; }
            public string UNIT { get; set; }
            public decimal? STORE_NUM { get; set; }
            public decimal? TAX_PRICE { get; set; }
            public decimal? TAX_MONEY { get; set; }
            public decimal? NOTAX_PRICE { get; set; }
            public decimal? NOTAX_MONEY { get; set; }
            public DateTime? IN_DATE { get; set; }
            public string PROVIDER_NAME { get; set; }
            public string DEPT_NAME { get; set; }
            public DateTime? CREATEDATE { get; set; }
            /// <summary>
            /// 期初
            /// </summary>
            public decimal? BEG_NUM { get; set; }
            public decimal? BEG_MONEY { get; set; }
            public decimal? BEG_NOTAX_MONEY { get; set; }
            /// <summary>
            /// 期末
            /// </summary>
            public decimal? END_NUM { get; set; }
            public decimal? END_MONEY { get; set; }
            public decimal? END_NOTAX_MONEY { get; set; }
            /// <summary>
            /// 入库
            /// </summary>
            public decimal? IN_MONEY { get; set; }
            public decimal? IN_NOTAX_MONEY { get; set; }
            /// <summary>
            /// 出库
            /// </summary>
            public decimal? OUT_MONEY { get; set; }
            public decimal? OUT_NOTAX_MONEY { get; set; }
            /// <summary>
            /// 入库数量
            /// </summary>
            public decimal? IN_NUM { get; set; }
            /// <summary>
            /// 出库数量
            /// </summary>
            public decimal? OUT_NUM { get; set; }
        }

        /// <summary>
        /// 收发存报表
        /// </summary>
        public async Task<GridData> StoreInOutListAsync(DateTime? CREATEDATE, GridRequest request)
        {
            var res = await _dbContext.Query<SP_STORE_WATER>()
                 .Select(t => new
                 {
                     t.STORE_ID,
                     IN_NUM = t.IN_NUM ?? 0,
                     OUT_NUM = t.OUT_NUM ?? 0
                 })
                .GroupBy(t => new { t.STORE_ID })
                .Select(t => new
                {
                    t.STORE_ID,
                    IN_NUM = Sql.Sum(t.IN_NUM),
                    OUT_NUM = Sql.Sum(t.OUT_NUM)
                })
                .LeftJoin<SP_STORE>((a, b) => a.STORE_ID == b.STORE_ID)
                .Select((a, b) => new StoreInOutReq
                {
                    STORE_ID = a.STORE_ID,
                    IN_NUM = a.IN_NUM,
                    OUT_NUM = a.OUT_NUM,
                    HOUSE_NAME = b.HOUSE_NAME,
                    PRODUCE = b.PRODUCE,
                    TYPE_NAME = b.TYPE_NAME,
                    UNIT = b.UNIT,
                    SP_SIZE = b.SP_SIZE,
                    SP_NAME = b.SP_NAME,
                    SP_CODE = b.SP_CODE,
                    IN_DATE = b.IN_DATE,
                    CREATEDATE = b.CREATEDATE,
                    NOTAX_PRICE = b.NOTAX_PRICE,
                    TAX_PRICE = b.TAX_PRICE,
                    BATCH_CODE = b.BATCH_CODE,
                    PROVIDER_NAME = b.PROVIDER_NAME,
                    DEPT_NAME = b.DEPT_NAME,
                    IN_MONEY = a.IN_NUM * b.TAX_PRICE,
                    IN_NOTAX_MONEY = a.IN_NUM * b.NOTAX_PRICE,
                    OUT_MONEY = a.OUT_NUM * b.TAX_PRICE,
                    OUT_NOTAX_MONEY = a.OUT_NUM * b.NOTAX_PRICE
                })
                .GetGridData(request);
            foreach (var item in (List<StoreInOutReq>)res.Rows)
            {
                decimal? sum = 0;
                if (CREATEDATE.HasValue)
                {
                    sum = _dbContext.Query<SP_STORE_WATER>().Where(t => t.STORE_ID == item.STORE_ID && t.WATER_DATE < CREATEDATE)
                        .Select(t => new
                        {
                            IN_NUM = t.IN_NUM ?? 0,
                            OUT_NUM = t.OUT_NUM ?? 0
                        })
                        .Sum(t => Sql.Sum(t.IN_NUM) - Sql.Sum(t.OUT_NUM));
                }

                item.BEG_NUM = sum ?? 0;
                item.BEG_MONEY = item.TAX_PRICE * item.BEG_NUM;
                item.BEG_NOTAX_MONEY = item.NOTAX_PRICE * item.BEG_NUM;

                item.END_NUM = (item.BEG_NUM + item.IN_NUM - item.OUT_NUM) ?? 0;
                item.END_MONEY = item.TAX_PRICE * item.END_NUM;
                item.END_NOTAX_MONEY = item.NOTAX_PRICE * item.END_NUM;
            }
            return res;
        }
        #endregion
    }
}
