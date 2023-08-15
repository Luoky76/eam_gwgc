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
    public class SpOutController : AreaController
    {
        private readonly ISpOutService _service;

        public SpOutController(ISpOutService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                spapplyType = comboxData["SpapplyType"],
                auditing = comboxData["Auditing"],
            }, "成功");
        }

        /// <summary>
        /// 导入物料功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportSpListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportSpList(request), "成功");
        }

        /// <summary>
        /// 获取物料领用申请记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutAppListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutAppList(request), "成功");
        }

        /// <summary>
        /// 根据物料领用申请ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutAppListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<SP_OUT_APP>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetSpOutAppListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理物料领用申请记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageSpOutAppAsync(SaveRequest<SP_OUT_APP> request, SaveRequest<SP_OUTAPP_DET> requestdet)
        {
            return await _service.ManageSpOutApp(request, requestdet);
        }

        /// <summary>
        /// 提交物料领用申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitSpOutAppAsync(string sid)
        {
            return AjaxResult.Success(await _service.SubmitSpOutApp(sid), "成功");
        }
        /// <summary>
        /// 获取物料领用申请明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutAppdetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutAppdetList(request), "成功");
        }

        /// <summary>
        /// 获取物料领用出库记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutStoreListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutStoreList(request), "成功");
        }

        /// <summary>
        /// 根据物料领用出库ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutStoreListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<SP_OUTSTORE>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetSpOutStoreListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理物料领用出库记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageSpOutStoreAsync(SaveRequest<SP_OUTSTORE> request, SaveRequest<SP_OUTSTORE_DET> requestdet)
        {
            return await _service.ManageSpOutStore(request, requestdet);
        }

        /// <summary>
        /// 提交物料领用出库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitSpOutStoreAsync(string sid)
        {
            return AjaxResult.Success(await _service.SubmitSpOutStore(sid), "成功");
        }

        /// <summary>
        /// 注销物料领用出库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> UnSubmitSpOutStoreAsync(string sid)
        {
            return AjaxResult.Success(await _service.UnSubmitSpOutStore(sid), "成功");
        }

        /// <summary>
        /// 获取物料领用出库明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutStoredetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutStoredetList(request), "成功");
        }

    }
}
