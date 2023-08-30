using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Device.Interfaces
{
    public interface IDeviceCardService : IService
    {
        #region 设备卡片
        
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> ComboxData();
        Task<AjaxResult> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request);

        #endregion

        #region 设备参数

        Task<GridData> ParamListAsync(GridRequest request);

        Task<AjaxResult> SaveParamAsync(SaveRequest<DEVICE_PARAM> request);

        #endregion

        #region 设备随机资料

        Task<GridData> DocListAsync(GridRequest request);

        Task<AjaxResult> SaveDocAsync(SaveRequest<DEVICE_DOC> request);

        #endregion

        #region 重大改造履历

        Task<GridData> RemListAsync(GridRequest request);

        Task<AjaxResult> SaveRemAsync(SaveRequest<DEVICE_REMOULD> request);

        #endregion

        #region 设备台账

        Task<GridData> DeviceListAllAsync(GridRequest request);

        #endregion

        #region 维保设备

        Task<GridData> PmListAsync(GridRequest request);

        #endregion
    }
}
