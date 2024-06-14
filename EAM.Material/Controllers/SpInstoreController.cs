using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpInstoreController : AreaController
    {
        private readonly ISpInstoreService _service;
        public SpInstoreController(ISpInstoreService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            var result = await _service.DetListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> DetailListAsync(GridRequest request)
        {
            var result = await _service.DetailListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        public async Task<AjaxResult> Save(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet)
        {
            return await _service.Save(request, requestdet);
        }

        public async Task<AjaxResult> HouseList()
        {
            return await _service.HouseList();
        }
    }
}
