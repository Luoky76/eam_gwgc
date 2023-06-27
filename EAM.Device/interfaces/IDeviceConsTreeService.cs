using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.interfaces
{
    public interface IDeviceConsTreeService : IService
    {
        /// <summary>
        /// 下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();
        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> TreeAsync();


        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<BASE_DEVICE_COMPOSE> GetAsync(object id);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<BASE_DEVICE_COMPOSE> request);

    }
}
