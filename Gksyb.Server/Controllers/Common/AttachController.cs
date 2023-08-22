using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Common;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gksyb.Server.Controllers.Auth
{
#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问

    [GksybAuthorize(true)]
    public class AttachController : BaseController
    {
        private readonly IAttachService _attachService;

        public AttachController(IAttachService attachService)
        {
            _attachService = attachService;
        }

        /// <summary>
        /// 附件服务
        /// </summary>
        private IAttachService Service
        {
            get
            {
                _attachService.SetUser(CurrentUser);
                return _attachService;
            }
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        [HttpPost, 
         DisableRequestSizeLimit,
         RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        public async Task<AjaxResult> Upload([FileOptions("gif,jpg,jpeg,bmp,png,pdf,xlsx,xls,doc,docx", 200)] IFormFile formFile, string parms)
        {
            return await Service.Upload(formFile, parms);
        }

        /// <summary>
        /// 获取某单据关联的各类附件数量
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetAttachFieldStat(string tableName, string dataId)
        {
            return await Service.GetAttachFieldStat(tableName, dataId);
        }

        /// <summary>
        /// 删除附件（根据附件ID）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> DeleteAttachByAttachId(string id)
        {
            return await Service.DeleteAttachByAttachId(id);
        }

        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await Service.ListAsync(request), "成功");
        }

    }
}