using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class BaseSpCatalogService : IBaseSpCatalogService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public BaseSpCatalogService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
            return await _dbContext.Query<BASE_SPCATALOG>().GetGridData(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            var row = await _dbContext.Query<BASE_SPCATALOG>().Where(c => c.SP_ID == id).FirstAsync();
            return AjaxResult.Success(row);
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
                    //{ "TerminalData", "码头" },
                    //{ "DeviceTypeData", "设备类型" }
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
    }
}
