using Chloe;
using EAM.Special.DTO;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace EAM.Special.Services
{
    public class BuildService : IBuildService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly UserSession _userSession;

        public BuildService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxDataService.Get(new Dictionary<string, object>(){
                { "ShipInfo",null},
            });
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {/*
            var ship = await _dbContext.Query<BC_CODE>().Where(a => a.CODE_TYPE == "shipdepartmentpermission")
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.CODE_CN })
                .FirstOrDefaultAsync();*/
            var list = await _dbContext.Query<BUILD_COUNT>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    b.SEC_DEPTID,
                    a.BUILD_ID,
                    b.DEPT_ID,
                    a.DEVICE_ID,
                    a.DEVICE_NAME,
                    a.STARTDATE,
                    a.SHIPTIMES,
                    a.SHIPNUM,
                    a.CONPLAN,
                    a.DREDGETIME,
                    a.SAILTIME,
                    a.REPAIRTIME,
                    a.WEATHEREFFECT,
                    a.OTHERSTOP,
                    a.DAILYCONSUMPTION,
                    a.SUPPLEMENT,
                    a.STOCK,
                    a.MASTER,
                    a.AUXILIARY,
                    a.PUMP,
                    a.SUBTOTAL,
                    a.SUPPLEMENT2,
                    a.STOCK2,
                    a.LUBRICATE,
                    a.LUBRICATE_SUPPLEMENT,
                    a.LUBRICATE_STOCK,
                    a.MEMO,
                    a.WAIT_WORK,
                    a.WORK_TIME,
                    a.ANCHOR_TIME,
                    a.MAIN_RUNTIME,
                    a.MAIN_CUMTIME,
                    a.MOORING_RUNTIME,
                    a.MOORING_CUMTIME,
                    a.MAIN_ENGINE_RUNTIME,
                    a.MAIN_ENGINE_CUMTIME
                })
                .OrderByDesc(a => a.STARTDATE)
                .GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var list = await _dbContext.Query<BUILD_COUNT>(x => x.BUILD_ID == ID).ToListAsync();

            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.BUILD_ID,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.STARTDATE,
                    c.SHIPTIMES,
                    c.SHIPNUM,
                    c.CONPLAN,
                    c.DREDGETIME,
                    c.SAILTIME,
                    c.REPAIRTIME,
                    c.WEATHEREFFECT,
                    c.OTHERSTOP,
                    c.DAILYCONSUMPTION,
                    c.SUPPLEMENT,
                    c.STOCK,
                    c.MASTER,
                    c.AUXILIARY,
                    c.PUMP,
                    c.SUBTOTAL,
                    c.SUPPLEMENT2,
                    c.STOCK2,
                    c.LUBRICATE,
                    c.LUBRICATE_SUPPLEMENT,
                    c.LUBRICATE_STOCK,
                    c.MEMO,
                    c.WAIT_WORK,
                    c.WORK_TIME,
                    c.ANCHOR_TIME,
                    c.MAIN_RUNTIME,
                    c.MAIN_CUMTIME,
                    c.MOORING_RUNTIME,
                    c.MOORING_CUMTIME,
                    c.MAIN_ENGINE_RUNTIME,
                    c.MAIN_ENGINE_CUMTIME
                },
                c => a => a.BUILD_ID == c.BUILD_ID, BeforeAdd, BeforeUpdate, BeforeDelete, false);
        }

        /// <summary>
        /// 自动计算的字段
        /// </summary>
        /// <returns></returns>
        private async Task Calc(BUILD_COUNT entity)
        {
            //将上次填报的淡水、柴油、滑油库存数据带入：本次库存 = 上次库存 - 本次消耗 + 本次补充
            //主机累计时间、主发电机累计时间、停泊发电机累计时间自动计算，每年清零
            var last_data = await _dbContext.Query<BUILD_COUNT>
                (a => a.STARTDATE < entity.STARTDATE && a.DEVICE_ID == entity.DEVICE_ID)
                .Select(a => new
                {
                    a.STARTDATE,
                    a.STOCK,
                    a.STOCK2,
                    a.LUBRICATE_STOCK,
                    a.MAIN_ENGINE_CUMTIME,
                    a.MAIN_CUMTIME,
                    a.MOORING_CUMTIME
                })
                .OrderByDesc(b => b.STARTDATE)
                .FirstAsync();

            entity.STOCK = (last_data?.STOCK ?? 0) - (entity.DAILYCONSUMPTION ?? 0) + (entity.SUPPLEMENT ?? 0);
            entity.STOCK2 = (last_data?.STOCK2 ?? 0) - (entity.SUBTOTAL ?? 0) + (entity.SUPPLEMENT2 ?? 0);
            entity.LUBRICATE_STOCK = (last_data?.LUBRICATE_STOCK ?? 0) - (entity.LUBRICATE ?? 0) + (entity.LUBRICATE_SUPPLEMENT ?? 0);

            //判断是否跨年，新年则将累计时间清零
            if ((last_data?.STARTDATE.Year ?? 0) < entity.STARTDATE.Year)
            {
                entity.MAIN_ENGINE_CUMTIME = 0;
                entity.MAIN_CUMTIME = 0;
                entity.MOORING_CUMTIME = 0;
            }
            else
            {
                entity.MAIN_ENGINE_CUMTIME = (last_data?.MAIN_ENGINE_CUMTIME ?? 0) + entity.MAIN_ENGINE_RUNTIME;
                entity.MAIN_CUMTIME = (last_data?.MAIN_CUMTIME ?? 0) + entity.MAIN_RUNTIME;
                entity.MOORING_CUMTIME = (last_data?.MOORING_CUMTIME ?? 0) + entity.MOORING_RUNTIME;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(BUILD_COUNT entity)
        {
            //若实体无船舶信息，则根据登录用户部门查找船舶
            if (entity.DEVICE_ID.IsNullOrEmpty())
            {
                var card = _dbContext.Query<DEVICE_CARD>()
                .Select(b => new { b.DEVICE_NAME, b.DEVICE_ID, b.DEPT_ID })
                .Where(x => _userSession.Corp.CorpID == x.DEPT_ID).FirstOrDefault();
                if (card == null)
                {
                    throw new MessageException("未找到船舶信息！");
                }
                entity.DEVICE_ID = card.DEVICE_ID;
                entity.DEVICE_NAME = card.DEVICE_NAME;
            }

            entity.BUILD_ID = GuidHelper.NewSnowflakeId().ToString();

            await Calc(entity);

            var isex = await _dbContext.Query<BUILD_COUNT>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new { b.DEPT_ID, a.STARTDATE })
                .Where(x => x.STARTDATE == entity.STARTDATE && _userSession.Corp.CorpID == x.DEPT_ID).ToListAsync();
            if (isex.Count() > 0)
            {
                throw new MessageException("已存在此日期数据，无法重复添加！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(BUILD_COUNT entity)
        {
            await Calc(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(BUILD_COUNT request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="MessageException"></exception>
        public async Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            try
            {
                _dbContext.Session.BeginTransaction();
                //获取导入数据
                await formFile.Import<BuildImportDto>(async c =>
                {
                    //判断是否已存在此日期
                    var data = await _dbContext.Query<BUILD_COUNT>()
                    .Where(a => a.STARTDATE == c.STARTDATE)
                    .FirstOrDefaultAsync();

                    if (data != null)
                    {
                        throw new MessageException("已存在" + c.STARTDATE.ToString("yyyy-MM-dd") + "日期数据，无法重复添加！");
                    }

                    if (string.IsNullOrWhiteSpace(c.DEVICE_NAME))
                    {
                        throw new MessageException("船舶名称不能为空！");
                    }


                    var device = await _dbContext.Query<DEVICE_CARD>(x => x.DEVICE_NAME == c.DEVICE_NAME).FirstOrDefaultAsync();

                    BUILD_COUNT dto = c.MapTo<BUILD_COUNT>();
                    dto.BUILD_ID = GuidHelper.NewSnowflakeId().ToString();
                    dto.DEVICE_ID = device.DEVICE_ID;

                    await _dbContext.InsertAsync<BUILD_COUNT>(dto);

                });
                _dbContext.Session.CommitTransaction();
            }
            catch (Exception e)
            {
                _dbContext.Session.RollbackTransaction();
                throw new MessageException(e.Message);
            }

            return AjaxResult.Success(1);
        }

        public class BUILDCOUNT : BUILD_COUNT
        {
            public string START_DATE { get; set; }
            public string DEPT_ID { get; set; }
            public int YEAR { get; set; }
            public int MONTH { get; set; }
        }
        /// <summary>
        /// 年度报表 按月汇总
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> QryYearAsync(GridRequest request, string startdate, string enddate)
        {
            DateTime b_time = Convert.ToDateTime(startdate);
            DateTime e_time = Convert.ToDateTime(enddate);
            var filterData = await _dbContext.Query<BUILD_COUNT>()
                .Where(c => c.STARTDATE >= b_time && c.STARTDATE < e_time)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    b.DEPT_ID,
                    a.DEVICE_NAME,
                    YEAR = a.STARTDATE.Year,
                    MONTH = a.STARTDATE.Month,
                    a.STARTDATE,
                    a.SHIPTIMES,
                    a.SHIPNUM,
                    a.WORK_TIME,
                    a.ANCHOR_TIME,
                    a.CONPLAN,
                    a.DREDGETIME,
                    a.SAILTIME,
                    a.REPAIRTIME,
                    a.WEATHEREFFECT,
                    a.OTHERSTOP,
                    a.WAIT_WORK,
                    a.DAILYCONSUMPTION,
                    a.SUPPLEMENT,
                    a.STOCK,
                    a.MASTER,
                    a.AUXILIARY,
                    a.PUMP,
                    a.SUBTOTAL,
                    a.SUPPLEMENT2,
                    a.STOCK2,
                    a.LUBRICATE,
                    a.LUBRICATE_SUPPLEMENT,
                    a.LUBRICATE_STOCK,
                    a.MAIN_ENGINE_RUNTIME,
                    a.MAIN_ENGINE_CUMTIME,
                    a.MAIN_RUNTIME,
                    a.MAIN_CUMTIME,
                    a.MOORING_RUNTIME,
                    a.MOORING_CUMTIME,
                }).GetGridData(request);
            var dataList = JsonConvert.DeserializeObject<List<BUILDCOUNT>>(filterData.Rows.ToJson());

            var returnList = dataList.GroupBy(a => new
            {
                a.MONTH,
                a.YEAR,
                a.DEVICE_NAME,
                a.DEPT_ID,
            })
            .Select(c => new BUILDCOUNT
            {
                MONTH = c.Key.MONTH,
                YEAR = c.Key.YEAR,
                START_DATE = $"{c.Key.YEAR}-{c.Key.MONTH:D2}",
                DEVICE_NAME = c.Key.DEVICE_NAME,
                SHIPTIMES = c.Sum(item => item.SHIPTIMES ?? 0),
                SHIPNUM = c.Sum(item => item.SHIPNUM ?? 0m),
                WORK_TIME = c.Sum(item => item.WORK_TIME ?? 0m),
                ANCHOR_TIME = c.Sum(item => item.ANCHOR_TIME ?? 0m),
                CONPLAN = c.Sum(item => item.CONPLAN ?? 0m),
                DREDGETIME = c.Sum(item => item.DREDGETIME ?? 0m),
                SAILTIME = c.Sum(item => item.SAILTIME ?? 0m),
                REPAIRTIME = c.Sum(item => item.REPAIRTIME ?? 0m),
                WEATHEREFFECT = c.Sum(item => item.WEATHEREFFECT ?? 0m),
                OTHERSTOP = c.Sum(item => item.OTHERSTOP ?? 0m),
                WAIT_WORK = c.Sum(item => item.WAIT_WORK ?? 0m),
                DAILYCONSUMPTION = c.Sum(item => item.DAILYCONSUMPTION ?? 0m),
                SUPPLEMENT = c.Sum(item => item.SUPPLEMENT ?? 0m),
                STOCK = c.Sum(item => item.STOCK ?? 0m),
                MASTER = c.Sum(item => item.MASTER ?? 0m),
                AUXILIARY = c.Sum(item => item.AUXILIARY ?? 0m),
                PUMP = c.Sum(item => item.PUMP ?? 0m),
                SUBTOTAL = c.Sum(item => item.SUBTOTAL ?? 0m),
                SUPPLEMENT2 = c.Sum(item => item.SUPPLEMENT2 ?? 0m),
                STOCK2 = c.Sum(item => item.STOCK2 ?? 0m),
                LUBRICATE = c.Sum(item => item.LUBRICATE ?? 0m),
                LUBRICATE_SUPPLEMENT = c.Sum(item => item.LUBRICATE_SUPPLEMENT ?? 0m),
                LUBRICATE_STOCK = c.Sum(item => item.LUBRICATE_STOCK ?? 0m),
                MAIN_ENGINE_RUNTIME = c.Sum(item => item.MAIN_ENGINE_RUNTIME ?? 0m),
                MAIN_RUNTIME = c.Sum(item => item.MAIN_RUNTIME ?? 0m),
                MOORING_RUNTIME = c.Sum(item => item.MOORING_RUNTIME ?? 0m)
            }).ToList();
            GridData gridData = new GridData()
            {
                Rows = returnList
            };
            return gridData;
        }

        /// <summary>
        /// 导出年度模板数据
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ExportYearListAsync(string year)
        {
            var res = await _dbContext.Query<BUILD_COUNT>()
                .Where(x => x.STARTDATE.Year.Equals(year))
                .Select(t => new BuildExportData
                {
                    DEVICE_NAME = t.DEVICE_NAME,
                    SHIPTIMES = t.SHIPTIMES,
                    ZYTIME = t.DREDGETIME + t.SAILTIME,
                    STOPTIME = t.REPAIRTIME + t.WEATHEREFFECT + t.OTHERSTOP,
                    DAILYCONSUMPTION = t.DAILYCONSUMPTION,
                    MASTER = t.MASTER,
                    AUXILIARY = t.AUXILIARY,
                    LUBRICATE = t.LUBRICATE,
                    PUMP = t.PUMP,
                })
                .GetGridData(null);
            return res;
        }

        public async Task<List<BuildMonthExportData>> ExportMonthListAsync(string year)
        {
            var monthlyData = await _dbContext.Query<BUILD_COUNT>()
                .Where(x => x.STARTDATE.Year.ToString() == year)
                .GroupBy(x => x.STARTDATE.Month)  // 按月份分组
                .Select(group => new BuildMonthExportData
                {
                    Month = group.STARTDATE.Month,
                    SHIPTIMES = Sql.Sum(group.SHIPTIMES),
                    ZYTIME = Sql.Sum(group.DREDGETIME + group.SAILTIME + group.CONPLAN),
                    STOPTIME = Sql.Sum(group.REPAIRTIME + group.WEATHEREFFECT + group.OTHERSTOP),
                    DAILYCONSUMPTION = Sql.Sum(group.DAILYCONSUMPTION),
                    MASTER = Sql.Sum(group.MASTER),
                    AUXILIARY = Sql.Sum(group.AUXILIARY),
                    LUBRICATE = Sql.Sum(group.LUBRICATE),
                    PUMP = Sql.Sum(group.PUMP),
                })
                .OrderBy(x => x.Month)  // 按月份排序
                .ToListAsync();

            var allMonths = Enumerable.Range(1, 12);
            var monthNames = new string[]
    {
        "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"
    };

            var dataDictionary = monthlyData.ToDictionary(x => x.Month);

            var result = new List<BuildMonthExportData>();
            var boat = _dbContext.Query<BUILD_COUNT>().Select(c => c.DEVICE_NAME).ToList();
            foreach (var month in allMonths)
            {
                if (dataDictionary.TryGetValue(month, out var data))
                {
                    data.DEVICE_NAME = boat[0] ?? "";
                    data.MonthName = monthNames[month - 1];
                    result.Add(data);
                }
                else
                {
                    result.Add(new BuildMonthExportData
                    {
                        Month = month,
                        MonthName = monthNames[month - 1]
                    });
                }
            }

            return result;
        }
    }
}
