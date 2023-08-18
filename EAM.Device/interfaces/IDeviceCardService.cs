using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Device.Interfaces
{
    public interface IDeviceCardService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        Task<GridData> DeviceListAsync(GridRequest request);

        Task<DEVICE_CARD> GetAsync(string id);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request);
    }
}
