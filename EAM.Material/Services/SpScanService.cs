using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpScanService : BaseService, ISpScanService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public SpScanService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        class SpScanRes : SP_SCAN
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
            var res = await _dbContext.Query<SP_SCAN>().GetGridData(request);
            var datas = new List<SpScanRes>();
            foreach (var item in (List<SP_SCAN>)res.Rows)
            {
                var data = item.MapTo<SpScanRes>();
                data.DETAILCOUNT = _dbContext.Query<SP_SCAN_DET>().Where(t => t.SCAN_ID == item.SCAN_ID).Count();
                datas.Add(data);
            }
            return new GridData()
            {
                Rows = datas,
                Total = res.Total
            };
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
                    { "BCCode", "scanstatus" },//盘点状态
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null},//仓位
                    { "BaseSpType",(Expression<Func<BASE_SPTYPE, bool>>)null},//物资分类
                });

                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "scantype" }//盘点类型
                });
                dic.TryAdd("ScanType", dic1["BCCode"]);
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
        public async Task<AjaxResult> Save(SaveRequest<SP_SCAN> request)
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

        private async Task BeforeAdd(SP_SCAN entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.SCAN_ID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            string type = $"BP{dt.Value.ToString("yyyyMM")}";
            string def = type + "0000";
            var model = await _dbContext.Query<SP_SCAN>(x => x.SCAN_CODE.Contains(type)).Select(x => Sql.Max(x.SCAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;

            entity.SCAN_CODE = type + index.ToString("D4");
            entity.SCAN_DATE = dt;

            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

            entity.SCAN_USERID = _userSession.UserID.ToString();
            entity.SCAN_USER = _userSession.RealName;
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.AUDITING = "0";
        }

        private async Task BeforeUpdate(SP_SCAN entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeDelete(SP_SCAN entity)
        {
            await _dbContext.DeleteAsync<SP_SCAN_DET>(x => x.SCAN_ID == entity.SCAN_ID);
        }

        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_SCAN>(x => sids.Contains(x.SCAN_ID),
                    x => new SP_SCAN
                    {
                        AUDITING = "1",
                        SCAN_STATUS = "2"
                    });
            return updatedevice;
        }

        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_SCAN_DET>().GetGridData(request);
        }

        public async Task<AjaxResult> DetailSave(SaveRequest<SP_SCAN_DET> request)
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
                c => a => a.SCAN_DET_ID == c.SCAN_DET_ID, DetBeforeAdd, DetBeforeUpdate);
        }

        private async Task DetBeforeAdd(SP_SCAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
            entity.AUDITING = "0";
        }

        private async Task DetBeforeUpdate(SP_SCAN_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        public async Task<AjaxResult> GenerateDet(string SCAN_ID)
        {
            var scan = _dbContext.QueryByKey<SP_SCAN>(SCAN_ID);
            if (scan == null)
            {
                return AjaxResult.Error("没有对应申请单");
            }
            var dets = _dbContext.Query<SP_STORE>()
                .Where(a => a.TYPE_ID == scan.TYPE_ID && scan.STOCK_ID == a.STOCK_ID).ToList();

            if (dets.Count == 0)
            {
                return AjaxResult.Error("没有对应库存清单");
            }

            var scandets = _dbContext.Query<SP_SCAN_DET>().Where(a => a.SCAN_ID == SCAN_ID).ToList();
            DateTime? dt = await _dbContext.GetSysdate();
            var importDetail = new List<SP_SCAN_DET>();
            foreach (var det in dets)
            {
                if (!scandets.Any(t => t.STORE_ID == det.STORE_ID))
                {
                    var req = det.MapTo<SP_SCAN_DET>();

                    req.SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString();
                    req.CREATE_USERID = _userSession.UserID.ToString();
                    req.CREATEDATE = dt;
                    req.MODIFY_USERID = _userSession.UserID.ToString();
                    req.MODIFYDATE = dt;
                    req.SCAN_ID = SCAN_ID;
                    req.STORE_NUM = det.NUM;
                    req.AUDITING = "0";
                    importDetail.Add(req);
                    await Task.CompletedTask;
                }
            }
            await _dbContext.InsertRangeAsync<SP_SCAN_DET>(importDetail);

            return AjaxResult.Success("生成成功");
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
        public async Task<GridData> DetailAnsListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_SCAN_DET>()
                .LeftJoin<SP_SCAN>((a, b) => a.SCAN_ID == b.SCAN_ID)
                .Where((a, b) => b.AUDITING == "1")
                .Select((a, b) => new SpScanDetRes
                {
                    AUDITING = a.AUDITING,
                    SRC_TYPE = a.SRC_TYPE,
                    SP_CODE = a.SP_CODE,
                    SP_NAME = a.SP_NAME,
                    SP_SIZE = a.SP_SIZE,
                    PRODUCE = a.PRODUCE,
                    UNIT = a.UNIT,
                    STORE_NUM = a.STORE_NUM,
                    SCAN_NUM = a.SCAN_NUM,
                    STOCK_NAME = a.STOCK_NAME,
                    TYPE_NAME = a.TYPE_NAME,
                    SCAN_RESULT = a.SCAN_RESULT,
                    SCAN_DET_ID = a.SCAN_DET_ID,
                    SCAN_ID = a.SCAN_ID,
                    CREATEDATE = a.CREATEDATE,
                    STORE_CODE = a.STORE_CODE,
                    SCAN_CODE = b.SCAN_CODE,
                    SCAN_TYPE = b.SCAN_TYPE,
                    SCAN_USER = b.SCAN_USER,
                    DEPT_NAME = b.DEPT_NAME,
                    SEC_DEPT = b.SEC_DEPT,
                    SCAN_DATE = b.SCAN_DATE
                })
                .GetGridData(request);
        }

        public async Task<int> DetSubmit(List<string> sids)
        {
            var list = _dbContext.Query<SP_SCAN_DET>().Where(x => sids.Contains(x.SCAN_DET_ID)).ToList();

            foreach (var item in list)
            {
                item.AUDITING = "1";
                item.MORE_NUM = item.SCAN_NUM - item.STORE_NUM;
                item.IS_DISPOSE = "1";
                _dbContext.Update<SP_SCAN_DET>(item);
            }

            var scanids = list.Select(t => t.SCAN_ID).Distinct().ToList();
            foreach (var id in scanids)
            {
                var count = _dbContext.Query<SP_SCAN_DET>().Where(x => x.SCAN_ID == id && x.AUDITING == "0").Count();
                if (count == 0)
                {
                    await _dbContext.UpdateAsync<SP_SCAN>(x => x.SCAN_ID == id,
                          x => new SP_SCAN
                          {
                              AUDITING = "1",
                              SCAN_STATUS = "3" //盘点完成
                          });
                }
            }

            return list.Count;
        }
    }
}
