using Chloe;
using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Linq.Expressions;

namespace EAM.Device.Services
{
    public class DeviceTypeService : BaseService, IDeviceTypeService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxService;
        private DateTime? _Sysdate;

        public DeviceTypeService(IDbContext dbContext, UserSession userSession, IComboxDataService comboxService)
        {
            _dbContext = dbContext;
            _userSession = userSession;
            _comboxService = comboxService;
        }

        public async Task<AjaxResult> ComboxData()
        {
            var data = await _comboxService.Get(new Dictionary<string, object>(){
                { "DeviceTypeCode",null},
                { "DeviceTypeName", (Expression<Func<BASE_DEVICETYPE, bool>>)null}
            });

            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.STATUS == "1").ToListAsync();
            var data = list.Select(c => new
            {
                TYPE_CODE = c.TYPE_CODE,
                TYPE_NAME = c.TYPE_NAME,
                TYPE_ID = c.TYPE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_TYPEID) || c.PRE_TYPEID == "0") ? "ROOT" : c.PRE_TYPEID,
                ICON = "fa fa-group"
            }).OrderBy(c => c.TYPE_CODE).ToList();
            data.Add(new { TYPE_CODE = "ROOT", TYPE_NAME = "设备分类", TYPE_ID = "ROOT", PARENTID = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }


        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BASE_DEVICETYPE> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.TYPE_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BASE_DEVICETYPE>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.STATUS,
                c.IS_SPEC,
                c.TYPE_DESC,
                c.WORK_YEAR,
                c.TYPE_LEVEL,
                c.PRE_TYPEID,
                PRE_TYPENAME = _dbContext.Query<BASE_DEVICETYPE>().Where(r => r.TYPE_ID == c.PRE_TYPEID).First().TYPE_NAME,
                c.MEMO,
                c.MODIFY_DATE,
                c.MODIFY_USERID,
                c.ADD_DATE,
                c.ADD_USERID,

            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_DEVICETYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.STATUS,
                    c.IS_SPEC,
                    c.TYPE_DESC,
                    c.WORK_YEAR,
                    c.TYPE_LEVEL,
                    c.PRE_TYPEID,
                    c.MEMO,
                    c.MODIFY_DATE,
                    c.MODIFY_USERID,
                    c.ADD_DATE,
                    c.ADD_USERID,
                },
                c => a => a.TYPE_ID == c.TYPE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(BASE_DEVICETYPE entity)
        {
            entity.TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.ADD_DATE = Sysdate;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(BASE_DEVICETYPE entity)
        {
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(BASE_DEVICETYPE entity)
        {
            //验证是否存在下属节点
            var query = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.STATUS == "1" && c.PRE_TYPEID == entity.TYPE_ID).ToListAsync();
            if (query.Count()>0) throw new MessageException("该节点存在有效下级节点，不能删除！");

            await Task.CompletedTask;
        }
        private async Task AfterSave(List<BASE_DEVICETYPE> added, List<BASE_DEVICETYPE> updated, List<BASE_DEVICETYPE> deleted)
        {

            await Task.CompletedTask;
        }



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
    }
}
