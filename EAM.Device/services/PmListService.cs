using Chloe;
using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.Services
{
    public class PmListService : IPmListService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;

        public PmListService(IDbContext dbContext, IComboxDataService comboxService)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
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
            });
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

    }
}