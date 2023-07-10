using Chloe;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Device.services
{
    public class DeviceConsTreeService : BaseService, IDeviceConsTreeService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxService;
        private DateTime? _Sysdate;

        public DeviceConsTreeService(IDbContext dbContext, UserSession userSession, IComboxDataService comboxService)
        {
            _dbContext = dbContext;
            _userSession = userSession;
            _comboxService = comboxService;
        }

        public async Task<AjaxResult> ComboxData()
        {
            var data = await _comboxService.Get(new Dictionary<string, object>(){
                { "DeviceComposeCode",(Expression<Func <BASE_DEVICE_COMPOSE, bool>>)null },
                { "DeviceComposeName", (Expression<Func <BASE_DEVICE_COMPOSE, bool>>)null},
                { "DeviceTypeName", (Expression<Func<BASE_DEVICETYPE, bool>>)null},
                { "ConsType", (Expression<Func <BC_CODE, bool>>)null},
            });

            return AjaxResult.Success(data);
        }
        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var composeData = await _dbContext.Query<BASE_DEVICE_COMPOSE>().ToListAsync();
            var composeList = composeData.Select(c => new
            {
                c.COMPOSE_CODE,
                c.COMPOSE_NAME,
                c.COMPOSE_ID,
                c.TYPE_NAME,
                c.TYPE_ID,
                TYPE = "1",
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_COMPOSEID)) ? c.TYPE_ID : c.PRE_COMPOSEID,
                ICON = "fa fa-group"
            }).OrderBy(c => c.COMPOSE_CODE).ToList();

            var typeData = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.STATUS == "1").ToListAsync();
            var typeList = typeData.Select(c => new
            {
                COMPOSE_CODE = c.TYPE_CODE,
                COMPOSE_NAME = c.TYPE_NAME,
                COMPOSE_ID = c.TYPE_ID,
                c.TYPE_NAME,
                c.TYPE_ID,
                TYPE = "0",
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_TYPEID) || c.PRE_TYPEID == "0") ? "ROOT" : c.PRE_TYPEID,
                ICON = "fa fa-cog"
            }).OrderBy(c => c.COMPOSE_CODE).ToList();

            composeList = composeList.Concat(typeList).ToList();

            composeList.Add(new
            {
                COMPOSE_CODE = "ROOT",
                COMPOSE_NAME = "设备构造树",
                COMPOSE_ID = "ROOT",
                TYPE_NAME = "",
                TYPE_ID = "",
                TYPE = "-1",
                PARENTID = "",
                ICON = "fa fa-sitemap"
            });
            return AjaxResult.Success(composeList, "成功");
        }


        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BASE_DEVICE_COMPOSE> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<BASE_DEVICE_COMPOSE>().Where(c => c.COMPOSE_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BASE_DEVICE_COMPOSE>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_NAME,
                c.COMPOSE_ID,
                c.COMPOSE_NAME,
                c.COMPOSE_CODE,
                c.COMPOSE_TYPE,
                c.PRE_COMPOSEID,
                PRE_COMPOSE_NAME = _dbContext.Query<BASE_DEVICE_COMPOSE>().Where(r => r.COMPOSE_ID == c.PRE_COMPOSEID).First().COMPOSE_NAME,
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_DEVICE_COMPOSE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.COMPOSE_ID,
                    c.COMPOSE_NAME,
                    c.COMPOSE_CODE,
                    c.COMPOSE_TYPE,
                    c.PRE_COMPOSEID,
                    c.MEMO,
                    c.MODIFY_DATE,
                    c.MODIFY_USERID,
                    c.ADD_DATE,
                    c.ADD_USERID,
                },
                c => a => a.COMPOSE_ID == c.COMPOSE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(BASE_DEVICE_COMPOSE entity)
        {
            entity.COMPOSE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.ADD_DATE = Sysdate;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            var query = await _dbContext.Query<BASE_DEVICE_COMPOSE>()
                .Where(c => c.COMPOSE_ID == entity.PRE_COMPOSEID && c.PRE_COMPOSEID == entity.COMPOSE_ID || c.COMPOSE_ID == entity.PRE_COMPOSEID && c.COMPOSE_ID == entity.COMPOSE_ID)
                .FirstOrDefaultAsync();
            if (query != null)
            {
                throw new MessageException("上级节点只能为父节点！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(BASE_DEVICE_COMPOSE entity)
        {
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            var query = await _dbContext.Query<BASE_DEVICE_COMPOSE>()
                .Where(c => c.COMPOSE_ID == entity.PRE_COMPOSEID && c.PRE_COMPOSEID == entity.COMPOSE_ID || c.COMPOSE_ID == entity.PRE_COMPOSEID && c.COMPOSE_ID == entity.COMPOSE_ID)
                .FirstOrDefaultAsync();
            if (query != null)
            {
                throw new MessageException("上级节点只能为父节点！");
            }
            else
            {
                var queryType = await _dbContext.Query<BASE_DEVICE_COMPOSE>()
                    .Where(c => c.COMPOSE_ID == entity.PRE_COMPOSEID).FirstOrDefaultAsync();
                entity.TYPE_ID = queryType.TYPE_ID;

            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(BASE_DEVICE_COMPOSE entity)
        {
            //验证是否存在下属节点
            var query = await _dbContext.Query<BASE_DEVICE_COMPOSE>().Where(c => c.PRE_COMPOSEID == entity.COMPOSE_ID).ToListAsync();
            if (query.Count > 0) throw new MessageException("该节点存在有效下级节点，不能删除！");

            await Task.CompletedTask;
        }
        private async Task AfterSave(List<BASE_DEVICE_COMPOSE> added, List<BASE_DEVICE_COMPOSE> updated, List<BASE_DEVICE_COMPOSE> deleted)
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
