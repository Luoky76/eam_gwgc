using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "labor")]
    public class LaborController : AreaController
    {
        private readonly LaborService _service;

        public LaborController(LaborService laborService)
        {

            _service = laborService;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }
        #region 劳保人员清单

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> LaborUserCataLogList(string code)
        {
            return await _service.LaborUserCataLogList(code);
        }


        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> LaborSizeListAsync(string userID)
        {
            return await _service.LaborSizeListAsync(userID);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> laborUserListAsync(GridRequest request)
        {
            return await _service.laborUserListAsync(request);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> laborrequestdetListAsync(GridRequest request)
        {
            return await _service.laborrequestdetListAsync(request);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> laborrequestListListAsync(GridRequest request)
        {
            return await _service.laborrequestListListAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> laborUserSaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SaveSizeAsync(SaveRequest<LABOR_SIZE> request)
        {
            return await _service.SaveSizeAsync(request);
        }

        #endregion
        #region 劳保需求申请
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> laborrequestListAsync(GridRequest request)
        {
            return await _service.laborrequestListAsync(request);
        }



        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> laborrequestSaveAsync(SaveRequest<LABOR_REQUEST> request)
        {
            return await _service.SaveAsync(request);
        }

        #endregion

        #region 劳保用品租借
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborRentList(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborRentList(request));
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetLaborRentDetList(string rentId)
        {
            return AjaxResult.Success(await _service.GetLaborRentDetList(rentId));
        }
        /// <summary>
        /// 获取记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborRentGet(string rentId)
        {
            return await _service.LaborRentGet(rentId);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborStoreList(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborStoreList(request));
        }
        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet)
        {
            return await _service.LaborRentSave(request, requestdet);
        }
        #endregion
        #region 劳保采购计划
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<GridData> laborcollectListAsync(GridRequest request)
        {
            return await _service.laborcollectListAsync(request);
        }
        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> laborcollectSaveAsync(SaveRequest<LABOR_COLLECT> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion

        #region 劳动用品退换
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborExchangeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborExchangeListAsync(request));
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetLaborExchangeAppDetList(string id)
        {
            return AjaxResult.Success(await _service.GetLaborExchangeAppDetList(id));
        }
        /// <summary>
        /// 获取记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaboExchangeGet(string id)
        {
            return await _service.LaboExchangeGet(id);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LaborExchangeSave(SaveRequest<LABOR_EXCHANGE> request, SaveRequest<LABOR_EXCHANGE_APPDET> requestdet)
        {
            return await _service.LaborExchangeSave(request, requestdet);
        }
        #endregion

    }
}
