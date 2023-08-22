using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.OpenXmlFormats.Wordprocessing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Emit;
using WkHtmlToPdfDotNet;

namespace EAM.Material.Services
{
    public class SpStoreService : BaseService, ISpStoreService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpStoreService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
            return await _dbContext.Query<SP_STORE>().GetGridData(request);
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
                    { "BCCode", "store_src" },
                    { "ProviderName", (Expression<Func<PROVIDER, bool>>)null}
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
        public async Task<AjaxResult> Save(SaveRequest<SP_STORE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.STORE_CODE,
                    c.SRC_TYPE,
                    c.SP_ID,
                    c.NUM,
                    c.PRICE,
                    c.MONEY,
                    c.HOUSE_ID,
                    c.HOUSE_NAME,
                    c.STOCK_ID,
                    c.STOCK_NAME,
                    c.M_USERID,
                    c.M_USER,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.DRAWING_NO,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.WEIGHT,
                    c.UNIT,
                    c.PRODUCE,
                    c.ONROAD_NUM,
                    c.REUSABILITY,
                    c.IN_NUM,
                    c.OUT_NUM,
                    c.BORROW_NUM,
                    c.APPLY_NO,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.IN_DATE,
                    c.IN_CODE,
                    c.STORE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.STOCK_CODE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.HOUSE_CODE,
                    c.DELIVERY_CODE,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.STORE_TOP,
                    c.STORE_LOWER,
                    c.SRC_ID,
                    c.IS_BACK,
                    c.IS_FIC,
                    c.TAX_MONEY,
                    c.TAX_PRICE,
                    c.INDET_ID,
                    c.COMP_CODE,
                    c.STORE_MONTH,
                    c.PURTYPE_CODE,
                    c.PRO_DET_ID,
                    c.INVOICE_CODE,
                    c.UNTAX_MONEY,
                    c.UNTAX_MONEY2,
                    c.TAX_RATE,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.NUM2,
                    c.PRICE2,
                    c.ISOLD,

                },
                c => a => a.STORE_ID == c.STORE_ID, BeforeAdd, BeforeUpdate);
        }

        private async Task BeforeAdd(SP_STORE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.STORE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

            //存入流水库存中
            var temp = entity.MapTo<STORE_WATER>();
            temp.WATER_ID = GuidHelper.NewSnowflakeId().ToString();
            await _dbContext.InsertAsync<STORE_WATER>(temp);
        }

        private async Task BeforeUpdate(SP_STORE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
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
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> StoreSumListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                 .Select(t => new
                 {
                     t.SP_ID,
                     t.SP_CODE,
                     t.SP_NAME,
                     STOCK_ID = t.STOCK_ID ?? "",
                     t.STOCK_NAME,
                     t.SP_SIZE,
                     t.UNIT,
                     t.PRODUCE,
                     NUM = t.NUM ?? 0
                 })
                .GroupBy(t => new { t.SP_ID, t.SP_CODE, t.SP_NAME, t.STOCK_ID, t.STOCK_NAME, t.SP_SIZE, t.UNIT, t.PRODUCE })
                .Select(t => new
                {
                    t.SP_ID,
                    t.SP_CODE,
                    t.SP_NAME,
                    t.STOCK_ID,
                    t.STOCK_NAME,
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    NUM = Sql.Sum(t.NUM)
                }).GetGridData(request);
        }

        /// <summary>
        /// 库存上下限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> LimitListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_LIMIT>()
                .Select(a => new StoreLimitReq
                {
                    SP_ID = a.SP_ID,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    STOCK_ID = a.STOCK_ID ?? "",
                    STOCK_NAME = a.STOCK_NAME ,
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
                } else
                {
                    query = query.Where(t => t.STOCK_ID == item.STOCK_ID);
                }
                var data = query.GroupBy(t => new { t.SP_ID, t.STOCK_ID })
                 .AndBy(t => t.SP_SIZE)
                  .AndBy(t => t.UNIT)
                   .AndBy(t => t.PRODUCE)
                .Select(t => new
                {
                    t.SP_ID,
                    t.STOCK_ID,
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    NUM = Sql.Sum(t.NUM),
                    MONEY = Sql.Sum(t.MONEY)
                }).First();

                item.SP_SIZE = data?.SP_SIZE;
                item.UNIT = data?.UNIT;
                item.PRODUCE = data?.PRODUCE;
                item.NUM = data?.NUM;
                item.MONEY = data?.MONEY;
            }
            return res;
        }
     
        /// <summary>
        /// 仓库库存预警
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> StoreLimitListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                .GroupBy(t => new { t.SP_ID, t.STOCK_ID, t.SP_SIZE, t.UNIT, t.PRODUCE })
                .Select(t => new
                {
                    t.SP_ID,
                    STOCK_ID = t.STOCK_ID??"",
                    t.SP_SIZE,
                    t.UNIT,
                    t.PRODUCE,
                    NUM = Sql.Sum(t.NUM),
                    MONEY = Sql.Sum(t.MONEY)
                }).
                LeftJoin<SP_LIMIT>((a, b) => a.SP_ID == b.SP_ID && a.STOCK_ID == Case.When(string.IsNullOrEmpty(b.STOCK_ID)).Then("").Else(b.STOCK_ID))
                .Where((a, b) => (b.STORE_LOWER > 0 && a.NUM < b.STORE_LOWER) || (b.STORE_TOP > 0 && a.NUM > b.STORE_TOP))
                .Select((a, b) => new StoreLimitReq
                {
                    STORE_TOP = b.STORE_TOP,
                    STORE_LOWER = b.STORE_LOWER,
                    SP_CODE = b.SP_CODE,
                    SP_NAME = b.SP_NAME,
                    STOCK_NAME = b.STOCK_NAME,
                    FDEPT_NAME = b.FDEPT_NAME,
                    SP_ID = a.SP_ID,
                    NUM = a.NUM,
                    MONEY = a.MONEY,
                    SP_SIZE = a.SP_SIZE,
                    UNIT = a.UNIT,
                    PRODUCE = a.PRODUCE
                })
                .GetGridData(request);
        }

        public async Task<AjaxResult> LimitSave(SaveRequest<SP_LIMIT> request)
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

        private async Task LimitBeforeAdd(SP_LIMIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.LIMIT_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task LimitBeforeUpdate(SP_LIMIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }

        /// <summary>
        /// 设置上下限
        /// </summary>
        /// <param name="LIMITID"></param>
        /// <param name="TOP">上限</param>
        /// <param name="LOWER">下限</param>
        /// <returns></returns>
        public async Task<int> SetTopLower(string LIMITID,int? TOP,int? LOWER)
        {
            DateTime? dt = await _dbContext.GetSysdate();
            var updatedevice = await _dbContext.UpdateAsync<SP_LIMIT>(x => x.LIMIT_ID == LIMITID,
                    x => new SP_LIMIT
                    {
                        STORE_TOP = TOP,
                        STORE_LOWER = LOWER,
                        MODIFY_USERID = _userSession.UserID.ToString(),
                        MODIFYDATE = dt
                    });
            return updatedevice;
        }
        #endregion

        #region 库存报表
        public async Task<AjaxResult> ReportComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BaseSpType",(Expression<Func<BASE_SPTYPE, bool>>)null }, //物资
                    { "BasePurtype",(Expression<Func<BASE_PURTYPE, bool>>)null },//采购
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null}//库位
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
        /// <summary>
        ///库存定期查询 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> StoreSearchListAsync(GridRequest request)
        {
            return await _dbContext.Query<STORE_WATER>()
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
                    NUM = Sql.Sum(t.IN_NUM) - Sql.Sum(t.OUT_NUM)
                })
                .LeftJoin<SP_STORE>((a, b) => a.STORE_ID == b.STORE_ID)
                .Select((a, b) => new SP_STORE
                {
                    STORE_ID = a.STORE_ID,
                    NUM = a.NUM,
                    STOCK_NAME = b.STOCK_NAME,
                    STOCK_ID = b.STOCK_ID,
                    STOCK_CODE = b.STOCK_CODE,
                    PURTYPE_NAME = b.PURTYPE_NAME,
                    PURTYPE_ID = b.PURTYPE_ID,
                    PRODUCE = b.PRODUCE,
                    TYPE_NAME = b.TYPE_NAME,
                    TYPE_ID = b.TYPE_ID,
                    UNIT = b.UNIT,
                    SP_SIZE = b.SP_SIZE,
                    SP_NAME = b.SP_NAME,
                    SP_CODE = b.SP_CODE,
                    DELIVERY_CODE = b.DELIVERY_CODE,
                    APPLY_NO = b.APPLY_NO,
                    STORE_MONTH = b.STORE_MONTH,
                    IN_DATE = b.IN_DATE,
                    CREATEDATE = b.CREATEDATE,
                    NOTAX_PRICE = b.NOTAX_PRICE,
                    PRICE = b.PRICE,
                    NOTAX_MONEY = a.NUM * b.NOTAX_PRICE,
                    MONEY = a.NUM * b.PRICE
                })
                .GetGridData(request);
        }

        public class StoreInOutReq: SP_STORE
        {
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
        }
        /// <summary>
        /// 收发存报表
        /// </summary>
        /// <param name="CREATEDATE"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> StoreInOutListAsync(DateTime? CREATEDATE, GridRequest request)
        {
            var res = await _dbContext.Query<STORE_WATER>()
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
                    STOCK_NAME = b.STOCK_NAME,
                    STOCK_ID = b.STOCK_ID,
                    STOCK_CODE = b.STOCK_CODE,
                    PURTYPE_NAME = b.PURTYPE_NAME,
                    PURTYPE_ID = b.PURTYPE_ID,
                    PRODUCE = b.PRODUCE,
                    TYPE_NAME = b.TYPE_NAME,
                    TYPE_ID = b.TYPE_ID,
                    UNIT = b.UNIT,
                    SP_SIZE = b.SP_SIZE,
                    SP_NAME = b.SP_NAME,
                    SP_CODE = b.SP_CODE,
                    DELIVERY_CODE = b.DELIVERY_CODE,
                    APPLY_NO = b.APPLY_NO,
                    STORE_MONTH = b.STORE_MONTH,
                    IN_DATE = b.IN_DATE,
                    CREATEDATE = b.CREATEDATE,
                    NOTAX_PRICE = b.NOTAX_PRICE,
                    PRICE = b.PRICE,
                    IN_MONEY = a.IN_NUM * b.PRICE,
                    IN_NOTAX_MONEY = a.IN_NUM * b.NOTAX_PRICE,
                    OUT_MONEY = a.OUT_NUM * b.PRICE,
                    OUT_NOTAX_MONEY = a.OUT_NUM * b.NOTAX_PRICE
                })
                .GetGridData(request);
            foreach (var item in (List<StoreInOutReq>)res.Rows)
            {
                decimal? sum = 0;
                if (CREATEDATE.HasValue)
                {
                    sum = _dbContext.Query<STORE_WATER>().Where(t => t.STORE_ID == item.STORE_ID && t.CREATEDATE < CREATEDATE)
                        .Select(t => new
                        {
                            IN_NUM = t.IN_NUM ?? 0,
                            OUT_NUM = t.OUT_NUM ?? 0
                        }).
                        Sum(t => Sql.Sum(t.IN_NUM) - Sql.Sum(t.OUT_NUM));
                }

                item.BEG_NUM = sum ?? 0;
                item.BEG_MONEY = item.PRICE * item.BEG_NUM;
                item.BEG_NOTAX_MONEY = item.NOTAX_PRICE * item.BEG_NUM;

                item.END_NUM = (item.BEG_NUM + item.IN_NUM - item.OUT_NUM) ?? 0;
                item.END_MONEY = item.PRICE * item.END_NUM;
                item.END_NOTAX_MONEY = item.NOTAX_PRICE * item.END_NUM;

            }
            return res;
        }
        #endregion
    }
}
