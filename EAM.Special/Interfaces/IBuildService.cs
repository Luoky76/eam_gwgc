using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Special.Interfaces
{
    public interface IBuildService : IService
    {
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> GetAsync(string ID);

        Task<AjaxResult> Save(SaveRequest<BUILD_COUNT> request);

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile);
    }
}
