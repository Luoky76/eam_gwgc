using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "labor")]
    public class LaborController : AreaController
    {
        private readonly ILaborService _service;

        public LaborController(ILaborService laborService) { 
        
        _service= laborService;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }
        #region 劳保人员清单
        [HttpPost]
        public async Task<GridData> laborUserListAsync(GridRequest request)
        {
            return await _service.laborUserListAsync(request);
        }

        [HttpPost]
        public async Task<GridData> laborrequestdetListAsync(GridRequest request)
        {
            return await _service.laborrequestdetListAsync(request);
        }
        [HttpPost]
        public async Task<GridData> laborrequestListListAsync(GridRequest request)
        {
            return await _service.laborrequestListListAsync(request);
        }

        [HttpPost]
        public async Task<AjaxResult> laborUserSaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion
        #region 劳保需求申请
        [HttpPost]
        public async Task<GridData> laborrequestListAsync(GridRequest request)
        {
            return await _service.laborrequestListAsync(request);
        }



        [HttpPost]
        public async Task<AjaxResult> laborrequestSaveAsync(SaveRequest<LABOR_REQUEST> request)
        {
            return await _service.SaveAsync(request);
        }

        #endregion

        #region 劳保用品租借
        [HttpPost]
        public async Task<AjaxResult> LaborRentList(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborRentList(request));
        }
        [HttpPost]
        public async Task<AjaxResult> GetLaborRentDetList(string rentId)
        {
            return AjaxResult.Success(await _service.GetLaborRentDetList(rentId));
        }
        [HttpPost]
        public async Task<AjaxResult> LaborRentGet(string rentId)
        {
            return await _service.LaborRentGet(rentId);
        }
        [HttpPost]
        public async Task<AjaxResult> LaborStoreList(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborStoreList(request));
        }
        [HttpPost]
        public async Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet)
        {
            return await _service.LaborRentSave(request, requestdet);
        }
        #endregion
        #region 劳保采购计划
        [HttpPost]
        public async Task<GridData> laborcollectListAsync(GridRequest request)
        {
            return await _service.laborcollectListAsync(request);
        }
        [HttpPost]
        public async Task<AjaxResult> laborcollectSaveAsync(SaveRequest<LABOR_COLLECT> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion

        #region 劳动用品退换
        [HttpPost]
        public async Task<AjaxResult> LaborExchangeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.LaborExchangeListAsync(request));
        }
        [HttpPost]
        public async Task<AjaxResult> GetLaborExchangeAppDetList(string id)
        {
            return AjaxResult.Success(await _service.GetLaborExchangeAppDetList(id));
        }
        [HttpPost]
        public async Task<AjaxResult> LaboExchangeGet(string id)
        {
            return await _service.LaboExchangeGet(id);
        }
     
        [HttpPost]
        public async Task<AjaxResult> LaborExchangeSave(SaveRequest<LABOR_EXCHANGE> request, SaveRequest<LABOR_EXCHANGE_APPDET> requestdet)
        {
            return await _service.LaborExchangeSave(request, requestdet);
        }
        #endregion

    }
}
