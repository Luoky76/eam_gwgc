using Chloe;
using EAM.Device.services.Dto;
using Gksyb.Common;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EAM.Device.Services
{
    public class PmListService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        private DateTime? Sysdate
        {
            get
            {
                if (!_Sysdate.HasValue)
                {
                    _Sysdate = _dbContext.GetSysdate().Result();
                }
                return _Sysdate;
            }
        }


        public PmListService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxService.Get(new Dictionary<string, object>(){
                { "MaintDept",null},
                { "PmcycleUnit",null},
                { "PmShippost",null},
                { "DeviceInfo",(Expression<Func<DEVICE_CARD, bool>>)(c => (c.TYPE_ID == "1"))},
            });
        }


        public async Task<AjaxResult> ImportPmAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            //获取导入数据
            _ = await formFile.Import<PmImportDto>(async c =>
            {
                var zhouqi = "";
                var is_fj = "";
                if (c.CYCLE != null)
                {
                    zhouqi = c.CYCLE switch
                    {
                        "每周" => "每周",
                        "月度" => "月度",
                        "季度" => "季度",
                        "半年" => "半年",
                        "年度" => "年度",
                        "2.5年" => "2.5年",
                        "5年" => "5年",
                        _ => throw new MessageException($"序号为 {c.ID} 的周期数据异常"),
                    };
                }
                if (c.IS_ATTACH != null)
                {
                    is_fj = c.IS_ATTACH switch
                    {
                        "是" => "1",
                        "否" => "0",
                        _ => throw new MessageException($"序号为 {c.ID} 的是否附件判断数据异常"),
                    };
                }

                var list = await _dbContext.Query<BC_CODE>(d => d.CODE_TYPE == "ship_dept" && d.CODE_CN == c.DEPARTMENT)
                    .ToListAsync();
                if (!list.Any())
                {
                    throw new MessageException($"序号为 {c.ID} 的部门数据异常");
                }

                //主键生成
                //var mainKey = GuidHelper.NewSnowflakeId().ToString();
                //查船舶的ID
                var deviceId = await _dbContext.Query<DEVICE_CARD>(b => c.DEVICE_CODE == b.DEVICE_NO).Select(b => b.DEVICE_ID).FirstOrDefaultAsync() ?? throw new MessageException($"序号为 {c.ID} 的船舶编号异常");
                var pmlists = c.MapTo<PM_STD_LIST>();
                pmlists.CYCLE = zhouqi;
                pmlists.IS_ATTACH = is_fj;
                pmlists.DEVICE_ID = deviceId;
                pmlists.PM_STD_LIST_ID = GuidHelper.NewSnowflakeId().ToString();
                pmlists.CREATE_USERID = _userSession.UserID.ToString();
                pmlists.CREATEDATE = Sysdate;
                pmlists.MODIFY_USERID = _userSession.UserID.ToString();
                pmlists.MODIFYDATE = Sysdate;
                await _dbContext.InsertAsync(pmlists);
            });
            return AjaxResult.Success(1);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PM_STD_LIST>().GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> SaveAsync(SaveRequest<PM_STD_LIST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.STD_CODE,
                    c.PART_NAME,
                    c.CONTENT,
                    c.EXE_USER,
                    c.CHK_USER,
                    c.CYCLE,
                    c.PM_STD_LIST_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.DEPARTMENT,
                    c.MEMO,
                    c.IS_ATTACH,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.DEVICE_CODE,
                },
                c => a => a.PM_STD_LIST_ID == c.PM_STD_LIST_ID
                , BeforeAdd);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PM_STD_LIST> GetAsync(string id)
        {
            return await _dbContext.Query<PM_STD_LIST>().Where(c => c.PM_STD_LIST_ID == id).FirstAsync();
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PM_STD_LIST entity)
        {
            entity.PM_STD_LIST_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 周期定时器
        /// </summary>
        /// <returns></returns>
        public async Task WeekTimer()
        {
            //保养计划的id
            string aa = "BYJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            //取当前月份
            var currentMonth = DateTime.Now.Month;
            //保养计划的临时数据
            var cardPmList = new List<PM_PLAN_EXE>();
            //保养计划明细的临时数据
            var pmplandetList = new List<PM_PLAN_DONEITEM>();

            var departments = await _dbContext.Query<BC_CODE>(d => d.CODE_TYPE == "ship_dept")
                .Select(c => c.CODE_EN)
                .ToListAsync();

            foreach (var department in departments)
            {
                var shipDept = department;
                //获取当前周期部门设备卡片基础数据
                var qrycards = await _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "每周" && c.DEPARTMENT == department)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_CODE == b.DEVICE_NO)
                .Select((a, b) => new
                {
                    b.DEVICE_ID,
                    b.DEVICE_NAME,
                    a.DEVICE_CODE,
                    b.DEVICE_TYPE,
                    b.ASSET_CODE,
                    b.INSTALL_SITE,
                    b.DEPT_NAME,
                    b.DEPT_ID,
                    b.CARD_USER,
                    b.CARD_USERID,
                })
                .Distinct()
                .ToListAsync();

                if (!qrycards.Any())
                    continue;

                //获取维保项目清单基础数据
                var qryPmlistCards = _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "每周" && c.DEPARTMENT == department);
                foreach (var qrycard in qrycards)
                {
                    var index = model.SubStr(10, 4).CastTo<int>() + cardPmList.Count + 1;
                    var scandet = new PM_PLAN_EXE()
                    {
                        EXE_ID = GuidHelper.NewSnowflakeId().ToString(),
                        PLAN_CODE = aa + index.ToString("D4"),
                        AUDITING = "0",
                        DEVICE_ID = qrycard.DEVICE_ID ?? "",
                        DEVICE_NAME = qrycard.DEVICE_NAME ?? "",
                        DEVICE_CODE = qrycard.DEVICE_CODE ?? "",
                        ASSET_CODE = qrycard.ASSET_CODE ?? "",
                        DEPT_NAME = qrycard.DEPT_NAME ?? "",
                        DEPT_ID = qrycard.DEPT_ID ?? "",
                        SHIP_DEPT = shipDept,
                        WDEPT_ID = shipDept,
                        EXE_USER = qrycard.CARD_USER ?? "",
                        EXE_USERID = qrycard.CARD_USERID ?? "",
                        SOURCE = "1",
                        PM_TYPE = "20",
                    };
                    cardPmList.Add(scandet);
                    var qryPmlists = qryPmlistCards.Where(c => c.DEVICE_ID == qrycard.DEVICE_ID).ToList();
                    foreach (var qryPmlist in qryPmlists)
                    {
                        var pmplandon = new PM_PLAN_DONEITEM()
                        {
                            DONEITEM_ID = GuidHelper.NewSnowflakeId().ToString(),
                            STD_CODE = qryPmlist.STD_CODE ?? "",
                            OBJECT_NAME = qryPmlist.PART_NAME ?? "",
                            CONTENT = qryPmlist.CONTENT ?? "",
                            STD_LEVEL = "定期保养",
                            WORK_STATE = "20",
                            MAINT_CYCLE = qryPmlist.CYCLE ?? "",
                            PLAN_MONTH = $"{currentMonth}月",
                            EXE_ID = scandet.EXE_ID,
                        };
                        pmplandetList.Add(pmplandon);
                    }
                }
            }
            await _dbContext.InsertRangeAsync(cardPmList);
            await _dbContext.InsertRangeAsync(pmplandetList);
        }

        /// <summary>
        /// 月定时器
        /// </summary>
        /// <returns></returns>
        public async Task MonthTimer()
        {
            //保养计划的id
            string aa = "BYJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            //取当前月份
            var currentMonth = DateTime.Now.Month;
            //保养计划的临时数据
            var cardPmList = new List<PM_PLAN_EXE>();
            //保养计划明细的临时数据
            var pmplandetList = new List<PM_PLAN_DONEITEM>();

            var departments = await _dbContext.Query<BC_CODE>(d => d.CODE_TYPE == "ship_dept")
                .Select(c => c.CODE_EN)
                .ToListAsync();

            foreach (var department in departments)
            {
                var shipDept = department;
                //获取当前周期部门设备卡片基础数据
                var qrycards = await _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "月度" && c.DEPARTMENT == department)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_CODE == b.DEVICE_NO)
                .Select((a, b) => new
                {
                    b.DEVICE_ID,
                    b.DEVICE_NAME,
                    a.DEVICE_CODE,
                    b.DEVICE_TYPE,
                    b.ASSET_CODE,
                    b.INSTALL_SITE,
                    b.DEPT_NAME,
                    b.DEPT_ID,
                    b.CARD_USER,
                    b.CARD_USERID,
                })
                .Distinct()
                .ToListAsync();

                if (!qrycards.Any())
                    continue;

                //获取维保项目清单基础数据
                var qryPmlistCards = _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "月度" && c.DEPARTMENT == department);
                foreach (var qrycard in qrycards)
                {
                    var index = model.SubStr(10, 4).CastTo<int>() + cardPmList.Count + 1;
                    var scandet = new PM_PLAN_EXE()
                    {
                        EXE_ID = GuidHelper.NewSnowflakeId().ToString(),
                        PLAN_CODE = aa + index.ToString("D4"),
                        AUDITING = "0",
                        DEVICE_ID = qrycard.DEVICE_ID ?? "",
                        DEVICE_NAME = qrycard.DEVICE_NAME ?? "",
                        DEPT_ID = qrycard.DEPT_ID ?? "",
                        DEVICE_CODE = qrycard.DEVICE_CODE ?? "",
                        ASSET_CODE = qrycard.ASSET_CODE ?? "",
                        DEPT_NAME = qrycard.DEPT_NAME ?? "",
                        SHIP_DEPT = shipDept,
                        WDEPT_ID = shipDept,
                        EXE_USER = qrycard.CARD_USER ?? "",
                        EXE_USERID = qrycard.CARD_USERID ?? "",
                        SOURCE = "1",
                        PM_TYPE = "20",
                    };
                    cardPmList.Add(scandet);
                    var qryPmlists = qryPmlistCards.Where(c => c.DEVICE_ID == qrycard.DEVICE_ID).ToList();
                    foreach (var qryPmlist in qryPmlists)
                    {
                        var pmplandon = new PM_PLAN_DONEITEM()
                        {
                            DONEITEM_ID = GuidHelper.NewSnowflakeId().ToString(),
                            STD_CODE = qryPmlist.STD_CODE ?? "",
                            OBJECT_NAME = qryPmlist.PART_NAME ?? "",
                            CONTENT = qryPmlist.CONTENT ?? "",
                            STD_LEVEL = "定期保养",
                            WORK_STATE = "20",
                            MAINT_CYCLE = qryPmlist.CYCLE ?? "",
                            PLAN_MONTH = $"{currentMonth}月",
                            EXE_ID = scandet.EXE_ID,
                        };
                        pmplandetList.Add(pmplandon);
                    }
                }
            }
            await _dbContext.InsertRangeAsync(cardPmList);
            await _dbContext.InsertRangeAsync(pmplandetList);
        }

        /// <summary>
        /// 季度定时器
        /// </summary>
        /// <returns></returns>
        public async Task QuarterTimer()
        {
            //保养计划的id
            string aa = "BYJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            //取当前月份
            var currentMonth = DateTime.Now.Month;
            //保养计划的临时数据
            var cardPmList = new List<PM_PLAN_EXE>();
            //保养计划明细的临时数据
            var pmplandetList = new List<PM_PLAN_DONEITEM>();

            var departments = await _dbContext.Query<BC_CODE>(d => d.CODE_TYPE == "ship_dept")
                .Select(c => c.CODE_EN)
                .ToListAsync();

            foreach (var department in departments)
            {
                var shipDept = department;
                //获取当前周期部门设备卡片基础数据
                var qrycards = await _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "季度" && c.DEPARTMENT == department)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_CODE == b.DEVICE_NO)
                .Select((a, b) => new
                {
                    b.DEVICE_ID,
                    b.DEVICE_NAME,
                    b.DEPT_ID,
                    a.DEVICE_CODE,
                    b.DEVICE_TYPE,
                    b.ASSET_CODE,
                    b.INSTALL_SITE,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.CARD_USERID,
                })
                .Distinct()
                .ToListAsync();

                if (!qrycards.Any())
                    continue;

                //获取维保项目清单基础数据
                var qryPmlistCards = _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "季度" && c.DEPARTMENT == department);
                foreach (var qrycard in qrycards)
                {
                    var index = model.SubStr(10, 4).CastTo<int>() + cardPmList.Count + 1;
                    var scandet = new PM_PLAN_EXE()
                    {
                        EXE_ID = GuidHelper.NewSnowflakeId().ToString(),
                        PLAN_CODE = aa + index.ToString("D4"),
                        AUDITING = "0",
                        DEVICE_ID = qrycard.DEVICE_ID ?? "",
                        DEVICE_NAME = qrycard.DEVICE_NAME ?? "",
                        DEPT_ID = qrycard.DEPT_ID ?? "",
                        DEVICE_CODE = qrycard.DEVICE_CODE ?? "",
                        ASSET_CODE = qrycard.ASSET_CODE ?? "",
                        DEPT_NAME = qrycard.DEPT_NAME ?? "",
                        SHIP_DEPT = shipDept,
                        WDEPT_ID = shipDept,
                        EXE_USER = qrycard.CARD_USER ?? "",
                        EXE_USERID = qrycard.CARD_USERID ?? "",
                        SOURCE = "1",
                        PM_TYPE = "20",
                    };
                    cardPmList.Add(scandet);
                    var qryPmlists = qryPmlistCards.Where(c => c.DEVICE_ID == qrycard.DEVICE_ID).ToList();
                    foreach (var qryPmlist in qryPmlists)
                    {
                        var pmplandon = new PM_PLAN_DONEITEM()
                        {
                            DONEITEM_ID = GuidHelper.NewSnowflakeId().ToString(),
                            STD_CODE = qryPmlist.STD_CODE ?? "",
                            OBJECT_NAME = qryPmlist.PART_NAME ?? "",
                            CONTENT = qryPmlist.CONTENT ?? "",
                            STD_LEVEL = "定期保养",
                            WORK_STATE = "20",
                            MAINT_CYCLE = qryPmlist.CYCLE ?? "",
                            PLAN_MONTH = $"{currentMonth}月",
                            EXE_ID = scandet.EXE_ID,
                        };
                        pmplandetList.Add(pmplandon);
                    }
                }
            }
            await _dbContext.InsertRangeAsync(cardPmList);
            await _dbContext.InsertRangeAsync(pmplandetList);
        }

        /// <summary>
        /// 年度定时器
        /// </summary>
        /// <returns></returns>
        public async Task YearTimer()
        {
            //保养计划的id
            string aa = "BYJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            //取当前月份
            var currentMonth = DateTime.Now.Month;
            //保养计划的临时数据
            var cardPmList = new List<PM_PLAN_EXE>();
            //保养计划明细的临时数据
            var pmplandetList = new List<PM_PLAN_DONEITEM>();

            var departments = await _dbContext.Query<BC_CODE>(d => d.CODE_TYPE == "ship_dept")
                .Select(c => c.CODE_EN)
                .ToListAsync();

            foreach (var department in departments)
            {
                var shipDept = department;
                //获取当前周期部门设备卡片基础数据
                var qrycards = await _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "年度" && c.DEPARTMENT == department)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_CODE == b.DEVICE_NO)
                .Select((a, b) => new
                {
                    b.DEVICE_ID,
                    b.DEVICE_NAME,
                    b.DEPT_ID,
                    a.DEVICE_CODE,
                    b.DEVICE_TYPE,
                    b.ASSET_CODE,
                    b.INSTALL_SITE,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.CARD_USERID,
                })
                .Distinct()
                .ToListAsync();

                if (!qrycards.Any())
                    continue;

                //获取维保项目清单基础数据
                var qryPmlistCards = _dbContext.Query<PM_STD_LIST>(c => c.CYCLE == "年度" && c.DEPARTMENT == department);
                foreach (var qrycard in qrycards)
                {
                    var index = model.SubStr(10, 4).CastTo<int>() + cardPmList.Count + 1;
                    var scandet = new PM_PLAN_EXE()
                    {
                        EXE_ID = GuidHelper.NewSnowflakeId().ToString(),
                        PLAN_CODE = aa + index.ToString("D4"),
                        AUDITING = "0",
                        DEVICE_ID = qrycard.DEVICE_ID ?? "",
                        DEVICE_NAME = qrycard.DEVICE_NAME ?? "",
                        DEPT_ID = qrycard.DEPT_ID ?? "",
                        DEVICE_CODE = qrycard.DEVICE_CODE ?? "",
                        ASSET_CODE = qrycard.ASSET_CODE ?? "",
                        DEPT_NAME = qrycard.DEPT_NAME ?? "",
                        SHIP_DEPT = shipDept,
                        WDEPT_ID = shipDept,
                        EXE_USER = qrycard.CARD_USER ?? "",
                        EXE_USERID = qrycard.CARD_USERID ?? "",
                        SOURCE = "1",
                        PM_TYPE = "20",
                    };
                    cardPmList.Add(scandet);
                    var qryPmlists = qryPmlistCards.Where(c => c.DEVICE_ID == qrycard.DEVICE_ID).ToList();
                    foreach (var qryPmlist in qryPmlists)
                    {
                        var pmplandon = new PM_PLAN_DONEITEM()
                        {
                            DONEITEM_ID = GuidHelper.NewSnowflakeId().ToString(),
                            STD_CODE = qryPmlist.STD_CODE ?? "",
                            OBJECT_NAME = qryPmlist.PART_NAME ?? "",
                            CONTENT = qryPmlist.CONTENT ?? "",
                            STD_LEVEL = "定期保养",
                            WORK_STATE = "20",
                            MAINT_CYCLE = qryPmlist.CYCLE ?? "",
                            PLAN_MONTH = $"{currentMonth}月",
                            EXE_ID = scandet.EXE_ID,
                        };
                        pmplandetList.Add(pmplandon);
                    }
                }
            }
            await _dbContext.InsertRangeAsync(cardPmList);
            await _dbContext.InsertRangeAsync(pmplandetList);
        }

    }
}