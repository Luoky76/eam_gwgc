using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Special.Controller
{
    [GksybAuthorize(true)]
    public class BuildController : AreaController
    {
        private readonly IBuildService _service;

        public BuildController(IBuildService service)
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

        public async Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            return await _service.ImportAsync(formFile);
        }
    }
}
