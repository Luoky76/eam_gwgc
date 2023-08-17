using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// 获取出库冲红记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutBackListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutBackList(request), "成功");
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutBackListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<PROVIDER_ASSESS_BASE>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetSpOutBackListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理出库冲红记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageSpOutBackAsync(List<SP_OUTSTORE> request)
        {
            return await _service.ManageSpOutBack(request);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitSpOutBackAsync(string sid)
        {
            return AjaxResult.Success(await _service.SubmitSpOutBack(sid), "成功");
        }

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportList(request), "成功");
        }

        /// <summary>
        /// 保存冲红
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveSpBackAsync(SaveRequest<SP_OUT_BACK> request)
        {
            return await _service.SaveSpBack(request);
        }

        /// <summary>
        /// 获取物料出库明细记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutStoreDetailListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutStoreDetailList(request), "成功");
        }

        /// <summary>
        /// 获取物料冲红明细记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetSpOutBackDetailListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetSpOutBackDetailList(request), "成功");
        }
    }
}