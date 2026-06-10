using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpOutController : AreaController
    {
        private readonly SpOutService _service;

        public SpOutController(SpOutService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return AjaxResult.Success(await _service.ComboxData());
        }

        /// <summary>
        /// 根据ID获取出库记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> Get(string outId)
        {
            if (outId.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(outId));
        }

        /// <summary>
        /// 获取出库列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> List(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 保存主表
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_OUTSTORE> request)
        {
            var result = await _service.SaveAsync(request);
            if (result.IsError) return result;
            return result;
        }

        /// <summary>
        /// 获取明细列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> DetList(GridRequest request)
        {
            return AjaxResult.Success(await _service.DetListAsync(request));
        }

        /// <summary>
        /// 保存明细
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> DetSave(SaveRequest<SP_OUTSTORE_DET> request)
        {
            return await _service.DetSaveAsync(request);
        }

        /// <summary>
        /// 子表导入批次
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> DetImport(string outId, List<string> store_ids)
        {
            return await _service.DetImportAsync(outId, store_ids);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> SaveAll(
            SaveRequest<SP_OUTSTORE> request1, SaveRequest<SP_OUTSTORE_DET> request2)
        {
            request1.Added ??= new List<SP_OUTSTORE>();
            request1.Updated ??= new List<SP_OUTSTORE>();
            request1.Deleted ??= new List<SP_OUTSTORE>();
            if (request1.Added.Count + request1.Updated.Count != 1)
            {
                return AjaxResult.Error("主表修改记录有且只能有一条");
            }
            if (request1.Deleted.Any())
            {
                return AjaxResult.Error("同时保存方法不能删除主表");
            }
            return await _service.SaveAllAsync(request1, request2);
        }

        /// <summary>
        /// 同时保存主子表并提交
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> SaveAllThenSubmit(
            SaveRequest<SP_OUTSTORE> request1, SaveRequest<SP_OUTSTORE_DET> request2)
        {
            request1.Added ??= new List<SP_OUTSTORE>();
            request1.Updated ??= new List<SP_OUTSTORE>();
            request1.Deleted ??= new List<SP_OUTSTORE>();
            if (request1.Added.Count + request1.Updated.Count != 1)
            {
                return AjaxResult.Error("主表修改记录有且只能有一条");
            }
            if (request1.Deleted.Any())
            {
                return AjaxResult.Error("同时保存方法不能删除主表");
            }
            var out_id = request1.Added.Any() ? request1.Added.First().OUT_ID : request1.Updated.First().OUT_ID;
            var saveResult = await _service.SaveAllAsync(request1, request2);
            if (saveResult.IsError) { return saveResult; }
            var submitResult = await _service.SubmitAsync(out_id);
            if (submitResult.IsError) { return submitResult; }
            return AjaxResult.Success("保存并提交成功");
        }

        /// <summary>
        /// 提交出库
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> Submit(string outId)
        {
            return await _service.SubmitAsync(outId);
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> Revoke(string outId)
        {
            return await _service.RevokeAsync(outId);
        }

        /// <summary>
        /// 出库冲红
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> OutRed(string outId)
        {
            await _service.OutRedAsync(outId);
            return AjaxResult.Success("冲红成功");
        }

        /// <summary>
        /// 获取审批任务ID
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> GetTaskId(string outId)
        {
            var taskId = await _service.GetTaskIdAsync(outId);
            if (taskId.IsNullOrWhiteSpace())
            {
                return AjaxResult.Error("未找到对应的审批任务ID");
            }
            return AjaxResult.Success(taskId, "获取审批任务ID成功");
        }

        #region 出库冲红

        /// <summary>
        /// 获取冲红记录列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> OutBackList(GridRequest request)
        {
            return AjaxResult.Success(await _service.OutBackListAsync(request));
        }

        /// <summary>
        /// 根据ID获取冲红记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> OutBackDetail(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.OutBackDetailAsync(id));
        }

        /// <summary>
        /// 保存冲红记录
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> SaveOutBack(SaveRequest<SP_OUT_BACK> request)
        {
            return await _service.SaveOutBackAsync(request);
        }

        /// <summary>
        /// 管理冲红导入
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> ManageOutBack(List<SP_OUTSTORE> request)
        {
            return await _service.ManageOutBackAsync(request);
        }

        /// <summary>
        /// 提交冲红
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> SubmitOutBack(string sid)
        {
            return AjaxResult.Success(await _service.SubmitOutBackAsync(sid));
        }

        /// <summary>
        /// 撤销冲红
        /// </summary>
        [HttpPost, JsToken]
        public async Task<AjaxResult> UnSubmitOutBack(string sid)
        {
            return AjaxResult.Success(await _service.UnSubmitOutBackAsync(sid));
        }

        /// <summary>
        /// 导入冲红（已提交的出库单列表）
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ImportOutBackList(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportOutBackListAsync(request));
        }

        /// <summary>
        /// 获取冲红明细列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetOutBackDetList(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetOutBackDetListAsync(request));
        }

        #endregion 出库冲红

        #region 出库明细汇总

        /// <summary>
        /// 获取出库明细汇总记录
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> GetOutStoreDetailList(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetOutStoreDetailListAsync(request));
        }

        #endregion
    }
}
