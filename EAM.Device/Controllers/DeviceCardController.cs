using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.Controller
{
    [GksybAuthorize(true)]
    public class DeviceCardController : AreaController
    {
        private readonly IDeviceCardService _service;

        public DeviceCardController(IDeviceCardService service)
        {
            _service = service;
        }

        #region 设备卡片
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }
        /// <summary>
        /// 树形
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult<GridData>.Success(result);
        }

        [HttpPost]
        public async Task<AjaxResult> GetAsync(string id)
        {
            return await _service.GetAsync(id);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion

        #region 设备参数
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ParamListAsync(GridRequest request)
        {
            return await _service.ParamListAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveParamAsync(SaveRequest<DEVICE_PARAM> request)
        {
            return await _service.SaveParamAsync(request);
        }
        #endregion

        #region 设备随机资料

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> DocListAsync(GridRequest request)
        {
            return await _service.DocListAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveDocAsync(SaveRequest<DEVICE_DOC> request)
        {
            return await _service.SaveDocAsync(request);
        }
        #endregion

        #region 重大改造履历

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> RemListAsync(GridRequest request)
        {
            return await _service.RemListAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveRemAsync(SaveRequest<DEVICE_REMOULD> request)
        {
            return await _service.SaveRemAsync(request);
        }
        #endregion

        #region 设备台账

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> DeviceListAllAsync(GridRequest request)
        {
            var result = await _service.DeviceListAllAsync(request);
            return AjaxResult<GridData>.Success(result);
        }
        #endregion

        #region 维保设备

        [HttpPost]
        public async Task<AjaxResult<GridData>> PmListAsync(GridRequest request)
        {
            var result = await _service.PmListAsync(request);
            return AjaxResult<GridData>.Success(result);
        }

        #endregion
    }
}
