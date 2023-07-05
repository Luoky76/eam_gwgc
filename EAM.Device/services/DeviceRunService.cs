using Chloe;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gksyb.Core.Grid;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.services
{
    public class DeviceRunService : BaseService, IDeviceRunService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public DeviceRunService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "RunStatus",null},
                { "DeviceInfo",null},
            });
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRun(GridRequest request)
        {
            var qry = _dbContext.Query<RUN_TRANS>();
            if (!_userSession.IsAdmin)
            {
                qry = qry.Where(c => _userSession.Corp.CorpID == c.SEC_DEPTID)
                    .OrderByDesc(c => c.AUDITING)
                    .ThenByDesc(c => c.TRANS_DATE);
            }
            else
            {
                qry = qry.OrderByDesc(c => c.AUDITING)
                        .ThenByDesc(c => c.TRANS_DATE);
            }
            return await qry.GetGridData(request);
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Manage(SaveRequest<RUN_TRANS> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.TRANS_ID,
                    c.TRANS_DATE,
                    c.DEVICE_NO,
                    c.DEVICE_NAME,
                    c.RUN_STATUS,
                    c.NEW_RUN_STATUS,
                    c.TRANS_MEMO,
                    c.DEPT_NAME,
                    c.SEC_DEPT,
                },
                c => a => a.TRANS_ID == c.TRANS_ID, BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        private async Task BeforeAdd(RUN_TRANS entity)
        {
            entity.SEC_DEPTID = _userSession.Corp.CorpID;
            entity.SEC_DEPT = _userSession.Corp.CName;
            entity.DEPT_ID = _userSession.Corp.DeptId;
            entity.DEPT_NAME = _userSession.Corp.DeptName;
            entity.AUDITING = "0";
            entity.TRANS_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.ADD_DATE = await _dbContext.GetSysdate();
        }
        private async Task BeforeUpdate(RUN_TRANS entity)
        {
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFY_DATE = await _dbContext.GetSysdate();
        }
        private async Task BeforeDelete(RUN_TRANS entity)
        {

        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Submit(string sids, string deid, string newStatus)
        {
            var changeAuditing = await _dbContext.UpdateAsync<RUN_TRANS>(x => x.TRANS_ID==sids,
                x => new RUN_TRANS
                {
                    AUDITING = "1",
                });

            var changestatus = await _dbContext.UpdateAsync<DEVICE_CARD>(x => x.DEVICE_ID==deid,
                x => new DEVICE_CARD
                {
                    STATUS = newStatus,
                });
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetAllRun(GridRequest request)
        {
            if (!_userSession.IsAdmin)
            {
                var detail = _dbContext.Query<RUN_TRANS>().Select(x => new
                {
                    x.DEVICE_ID,
                    x.ADD_DATE,
                }).GroupBy(x => new
                {
                    x.DEVICE_ID,
                }).Select(x => new
                {
                    x.DEVICE_ID,
                    ADD_DATE = Sql.Max(x.ADD_DATE),
                });
                var qry = _dbContext.Query<DEVICE_CARD>()
                     .LeftJoin(detail, (a, b) => a.DEVICE_ID ==b.DEVICE_ID && a.AUDITING == "1")
                     .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID ==c.DEVICE_ID&&b.ADD_DATE==c.ADD_DATE)
                     .Where((a, b, c) => _userSession.Corp.CorpID == a.SEC_DEPTID)
                     .Select((a, b, c) => new
                     {
                         STATUS = a.STATUS == null ? "正常" : a.STATUS,
                         DEVICE_NO = a.DEVICE_NO,
                         DEVICE_NAME = a.DEVICE_NAME,
                         TYPE_NAME = a.TYPE_NAME,
                         TRANS_MEMO = c.TRANS_MEMO,
                         SEC_DEPT = a.SEC_DEPT,
                         DEPT_NAME = a.DEPT_NAME,
                         ADD_DATE = b.ADD_DATE,
                     })
                .OrderBy(a =>
                    Case.When(a.STATUS.Equals("停机")).Then("1")
                        .When(a.STATUS.Equals("维修")).Then("2")
                        .When(a.STATUS.Equals("事故")).Then("3")
                        .When(a.STATUS.Equals("保养")).Then("4")
                        .When(a.STATUS.Equals("检查")).Then("5")
                        .When(a.STATUS.Equals("正常")).Then("6")
                        .When(a.STATUS.Equals("备用")).Then("7").Else("")
                    )
                    .ThenBy(c => c.DEVICE_NO);

                return await qry.GetGridData(request);
            }
            else
            {
                var detail = _dbContext.Query<RUN_TRANS>().Select(x => new
                {
                    x.DEVICE_ID,
                    x.ADD_DATE,
                }).GroupBy(x => new
                {
                    x.DEVICE_ID,
                }).Select(x => new
                {
                    x.DEVICE_ID,
                    ADD_DATE = Sql.Max(x.ADD_DATE),
                });
                var qry = _dbContext.Query<DEVICE_CARD>()
                     .LeftJoin(detail, (a, b) => a.DEVICE_ID ==b.DEVICE_ID && a.AUDITING == "1")
                     .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID ==c.DEVICE_ID&&b.ADD_DATE==c.ADD_DATE)
                     .Select((a, b, c) => new
                     {
                         STATUS = a.STATUS == null ? "正常" : a.STATUS,
                         DEVICE_NO = a.DEVICE_NO,
                         DEVICE_NAME = a.DEVICE_NAME,
                         TYPE_NAME = a.TYPE_NAME,
                         TRANS_MEMO = c.TRANS_MEMO,
                         SEC_DEPT = a.SEC_DEPT,
                         DEPT_NAME = a.DEPT_NAME,
                         ADD_DATE = b.ADD_DATE,
                     })
                .OrderBy(a =>
                    Case.When(a.STATUS.Equals("停机")).Then("1")
                        .When(a.STATUS.Equals("维修")).Then("2")
                        .When(a.STATUS.Equals("事故")).Then("3")
                        .When(a.STATUS.Equals("保养")).Then("4")
                        .When(a.STATUS.Equals("检查")).Then("5")
                        .When(a.STATUS.Equals("正常")).Then("6")
                        .When(a.STATUS.Equals("备用")).Then("7").Else("")
                    )
                    .ThenBy(c => c.DEVICE_NO);
                return await qry.GetGridData(request);
            }

        }

    }
}
