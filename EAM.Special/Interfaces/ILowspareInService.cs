using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface ILowspareInService : IService
    {


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);


        /// <summary>
        /// 通过ID查询记录
        /// </summary>
        /// <returns></returns>
        public Task<SPEC_LOWSPARE_IN> GetAsync(string sdid);


        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SaveAsync(SaveRequest<SPEC_LOWSPARE_IN> request);
    }
}
