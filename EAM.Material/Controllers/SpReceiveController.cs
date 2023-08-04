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
    public class SpReceiveController : AreaController
    {
        private readonly ISpReceiveService _service;
        public SpReceiveController(ISpReceiveService service)
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

        public async Task<AjaxResult> GetAsync(string ID)
        {
            return await _service.GetAsync(ID);
        }

        public async Task<AjaxResult> Save(SaveRequest<SP_RECEIVE> request)
        {
            return await _service.Save(request);
        }

        public async Task<AjaxResult> SaveDet(SaveRequest<SP_RECEIVE_DET> request)
        {
            return await _service.SaveDet(request);
        }
        public async Task<AjaxResult> OrderList()
        {
            return await _service.OrderList();
        }
    }
}
