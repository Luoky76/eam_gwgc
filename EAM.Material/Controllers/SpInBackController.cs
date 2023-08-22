using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Gksyb.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpInBackController : AreaController
    {
        private readonly ISpInBackService _service;
        public SpInBackController(ISpInBackService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        public async Task<AjaxResult> InListAsync()
        {
            return await _service.InListAsync();
        }

        public async Task<AjaxResult> DetListAsync(GridRequest request)
        {
            var result = await _service.DetListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> Save(SaveRequest<SP_IN_BACK> request, SaveRequest<SP_INBACK_DET> requestdet)
        {
            return await _service.Save(request, requestdet);
        }

    }
}
