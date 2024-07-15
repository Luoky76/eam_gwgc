using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface IEnergyReportService : IService
    {
        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<REPORT_ENERGY> GetAsync(object id);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> GridListAsync(GridRequest request);

        Task<AjaxResult> SaveAsync(SaveRequest<REPORT_ENERGY> request);

        void SetUser(UserSession currentUser);

    }
}
