using Chloe;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace EAM.Device.services
{
    public class ReportService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;

        public ReportService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _userSession = userSession;
        }

        public class CostReportRes
        {
            /// <summary>
            /// 部门
            /// </summary>
            public string DEPT_NAME { get; set; }
            public string DEPT_ID { get; set; }
            public string DEVICE_ID { get; set; }
            /// <summary>
            /// 维修成本
            /// </summary>
            public decimal? REP { get; set; }
            /// <summary>
            /// 维保
            /// </summary>
            public decimal? PM { get; set; }
            /// <summary>
            /// 维修和维保
            /// </summary>
            public decimal? REP_AND_PM { get; set; }
            /// <summary>
            /// 订单
            /// </summary>
            public decimal? ORDER { get; set; }
            /// <summary>
            /// 物资消耗
            /// </summary>
            public decimal? OUTSTORE { get; set; }
            /// <summary>
            /// 淡水消耗量
            /// </summary>
            public decimal? DAILYCONSUMPTION { get; set; }
            /// <summary>
            /// 柴油消耗量
            /// </summary>
            public decimal? MASTER { get; set; }
            /// <summary>
            /// 滑油消耗量
            /// </summary>
            public decimal? LUBRICATE { get; set; }

        }

        /// <summary>
        /// 单船成本统计
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public async Task<GridData> CostReportAsync(string dateFrom, string dateTo)
        {
            DateTime b_time = Convert.ToDateTime(dateFrom + " 00:00:00");
            DateTime e_time = Convert.ToDateTime(dateTo + " 23:59:59");

            //var card = _dbContext.Query<DEVICE_CARD>()
            //    .Select(b => new { b.DEVICE_NAME, b.DEVICE_ID, b.DEPT_ID })
            //    .Where(x => _userSession.Corp.CorpID == x.DEPT_ID).FirstOrDefault();

            //BUILD_COUNT ,REP_PLAN_EXE, PM_PLAN_EXE-PM_PLAN_SP,SP_ORDER,SP_OUTSTORE

            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var req = _dbContext.Query<DEVICE_CARD>();
            //if (!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId)
            //{
            //    req = req.Where(x => _userSession.Corp.CorpID == x.DEPT_ID);
            //}
            var query = await req.Where(t => t.STATUS == "1" && t.TYPE_ID == "1")
                .Select(a => new
                {
                    a.DEVICE_ID,
                    DEPT_NAME = a.DEVICE_NAME,
                    a.DEPT_ID
                })
                .GroupBy(t => t.DEPT_ID)
                .AndBy(t => t.DEVICE_ID)
                .AndBy(t => t.DEPT_NAME)
                .Select(t => new CostReportRes
                {
                    DEPT_NAME = t.DEPT_NAME,
                    DEPT_ID = t.DEPT_ID,
                    DEVICE_ID = t.DEVICE_ID
                })
                .ToListAsync();

            if (query.Count > 0)
            {
                foreach (var item in query)
                {
                    //维修
                    item.REP = _dbContext.Query<REP_PLAN_EXE>()
                        .Where(a => a.DEVICE_ID == item.DEVICE_ID && a.AUDITING_D == "1" && a.ACT_START_DATE >= b_time && a.ACT_START_DATE <= e_time)
                        .Sum(t => t.ACT_MONEY);

                    //维保
                    item.PM = _dbContext.Query<PM_PLAN_EXE>().LeftJoin<PM_PLAN_SP>((a, b) => a.EXE_ID == b.EXE_ID)
                        .Where((a, b) => a.DEVICE_ID == item.DEVICE_ID && a.AUDITING_EXE == "1" && a.BEGIN_DATE >= b_time && a.BEGIN_DATE <= e_time)
                        .Select((a, b) => new
                        {
                            TAX_MONEY = b.TAX_MONEY.HasValue ? b.TAX_MONEY : 0
                        })
                        .Sum(t => t.TAX_MONEY);

                    //维修和维保
                    item.REP_AND_PM = item.REP + item.PM;

                    //订单
                    item.ORDER = _dbContext.Query<SP_ORDER>()
                       .Where(a => a.DEPT_ID == item.DEPT_ID && a.AUDITING == "1" && a.ORDER_DATE >= b_time && a.ORDER_DATE <= e_time)
                       .Sum(t => t.ORDER_MONEY);

                    //物资消耗
                    item.OUTSTORE = _dbContext.Query<SP_OUTSTORE>()
                       .Where(a => a.DEPT_ID == item.DEPT_ID && a.AUDITING_A == "1" && a.OUT_DATE >= b_time && a.OUT_DATE <= e_time)
                       .Sum(t => t.SUM_MONEY);

                    //耗能
                    var cost = _dbContext.Query<BUILD_COUNT>()
                       .Where(a => a.DEVICE_ID == item.DEVICE_ID && a.STARTDATE >= b_time && a.STARTDATE <= e_time)
                       .GroupBy(t => t.DEVICE_ID)
                       .Select(t => new
                       {
                           DAILYCONSUMPTION = Sql.Sum(t.DAILYCONSUMPTION),
                           MASTER = Sql.Sum(t.MASTER),
                           LUBRICATE = Sql.Sum(t.LUBRICATE),
                       }).FirstOrDefault();

                    item.DAILYCONSUMPTION = cost?.DAILYCONSUMPTION;
                    item.MASTER = cost?.MASTER;
                    item.LUBRICATE = cost?.LUBRICATE;
                }
            }
            return new GridData
            {
                Rows = query,
                Total = query.Count
            };
        }
    }
}