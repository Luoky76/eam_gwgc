using EAM.Material.DTO;
using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpareApplyService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICodeCreatorService _codeCreatorService;

        public SpareApplyService(IDbContext dbContext, UserSession userSession, IComboxDataService comboxDataService, ICodeCreatorService codeCreatorService)
        {
            _dbContext = dbContext;
            _userSession = userSession;
            _comboxDataService = comboxDataService;
            _codeCreatorService = codeCreatorService;
        }

        #region 物资编码申请
        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "SpUnit", (Expression<Func<SP_UNIT, bool>>)null},
                    { "BaseSpType", (Expression<Func<BASE_SPTYPE, bool>>)null},
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        class SpareApplyRes : SPARE_APPLY
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public int DETAILCOUNT;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SPARE_APPLY>()
                 .Select(c => new SpareApplyRes
                 {
                     APPLY_ID = c.APPLY_ID,
                     AUDITING = c.AUDITING,
                     APPLY_CODE = c.APPLY_CODE,
                     MEMO = c.MEMO,
                     APPLY_DATE = c.APPLY_DATE,
                     EDIT_USERID = c.EDIT_USERID,
                     EDIT_USER = c.EDIT_USER,
                     DEPT_ID = c.DEPT_ID,
                     DEPT_NAME = c.DEPT_NAME,
                     SEC_DEPTID = c.SEC_DEPTID,
                     SEC_DEPT = c.SEC_DEPT,
                     CREATE_USERID = c.CREATE_USERID,
                     CREATE_DATE = c.CREATE_DATE,
                     MODIFY_USERID = c.MODIFY_USERID,
                     MODIFY_DATE = c.MODIFY_DATE
                 })
                 .GetGridData(request);
            foreach (var item in (List<SpareApplyRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SPARE_APPLY_DET>().Where(t => t.APPLY_ID == item.APPLY_ID).Count();
            }
            return res;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<string> ApplySave(string memo)
        {
            var entity = new SPARE_APPLY
            {
                MEMO = memo
            };
            await BeforeAdd(entity);
            _dbContext.Insert(entity);
            return entity.APPLY_ID;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SPARE_APPLY> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.APPLY_ID,
                    c.AUDITING,
                    c.APPLY_CODE,
                    c.MEMO,
                    c.APPLY_DATE,
                    c.EDIT_USERID,
                    c.EDIT_USER,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.CREATE_USERID,
                    c.CREATE_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE
                },
                c => a => a.APPLY_ID == c.APPLY_ID, BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAdd(SPARE_APPLY entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.APPLY_ID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            entity.APPLY_CODE = await _codeCreatorService.CreateCodeAsync<SPARE_APPLY>("SQ", a => a.APPLY_CODE);
            entity.APPLY_DATE = dt;
            if (entity.SEC_DEPTID.IsNullOrWhiteSpace())
            {
                entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
                entity.SEC_DEPT = _userSession.ParentCompany.CName;
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.CName;
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            if (entity.EDIT_USERID.IsNullOrWhiteSpace())
            {
                entity.EDIT_USERID = _userSession.UserID.ToString();
                entity.EDIT_USER = _userSession.RealName;
            }
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task BeforeUpdate(SPARE_APPLY entity)
        {
        }

        /// <summary>
        /// 删除前处理
        /// </summary>
        private async Task BeforeDelete(SPARE_APPLY entity)
        {
            await _dbContext.DeleteAsync<SPARE_APPLY_DET>(x => x.APPLY_ID == entity.APPLY_ID);
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<int> Submit(List<string> sids)
        {
            var list = _dbContext.Query<SPARE_APPLY>().Where(t => sids.Contains(t.APPLY_ID)).ToList();
            var det = _dbContext.Query<SPARE_APPLY_DET>().Where(t => sids.Contains(t.APPLY_ID)).ToList();

            var importResult = new List<BASE_SPCATALOG>();
            foreach (var item in list)
            {
                var dets = det.Where(t => t.APPLY_ID == item.APPLY_ID).ToList();
                foreach (var d in dets)
                {
                    if (_dbContext.Query<BASE_SPCATALOG>().Any(t => t.SP_CODE == d.SP_CODE))
                    {
                        throw new MessageException("物资编码不可重复！");
                    }
                    var data = new BASE_SPCATALOG
                    {
                        SP_CODE = d.SP_CODE,
                        SP_NAME = d.SP_NAME,
                        MEMO = d.MEMO,
                        SP_SIZE = d.SP_SIZE,
                        DRAWING_NO = d.DRAWING_NO,
                        TYPE_NAME = d.TYPE_NAME,
                        TYPE_ID = d.TYPE_ID,
                        TYPE_CODE = d.TYPE_CODE,
                        STUFF = d.STUFF,
                        UNIT = d.UNIT,
                        IS_SPECIAL = d.IS_SPECIAL,
                        IS_WORK = d.IS_WORK,
                        IS_STANDARD = d.IS_STANDARD,
                        IS_RECOVERY = d.IS_RECOVERY,
                        PRODUCE = d.PRODUCE,
                        SP_ID = d.SP_ID,
                        IS_CANCEL = "0",
                        EDIT_USERID = item.EDIT_USERID,
                        DEPT_ID = item.DEPT_ID,
                        DEPT_NAME = item.DEPT_NAME,
                        EDIT_USER = item.EDIT_USER,
                        SEC_DEPTID = item.SEC_DEPTID,
                        SEC_DEPT = item.SEC_DEPT,
                        PURTYPE_ID = d.PURTYPE_ID,
                        PURTYPE_NAME = d.PURTYPE_NAME,
                        IS_RIGGING = d.IS_RIGGING
                    };
                    importResult.Add(data);
                }
            }

            if (importResult.Count > 0)
            {
                _dbContext.InsertRange(importResult);
            }
            await _dbContext.UpdateAsync<SPARE_APPLY>(c => sids.Contains(c.APPLY_ID), c => new SPARE_APPLY
            {
                AUDITING = "1"
            });
            return list.Count;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="folder"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public async Task<AjaxResult> ImportInDetail([FileOptions("xlsx,xls")] IFormFile formFile, string folder, string sid)
        {
            var apply = _dbContext.QueryByKey<SPARE_APPLY>(sid);
            if (apply == null)
            {
                return AjaxResult.Error("参数错误");
            }

            var importResult = new List<SPARE_APPLY_DET>();

            try
            {
                var type = _dbContext.Query<BASE_SPTYPE>().Where(t => t.TYPE_NAME == "临时类别").FirstOrDefault();

                await formFile.Import<SpDetailExportData>(async c =>
                {
                    var temp = c.MapTo<SPARE_APPLY_DET>();
                    if (string.IsNullOrEmpty(c.TYPE_NAME))
                    {
                        temp.TYPE_NAME = type.TYPE_NAME;
                        temp.TYPE_ID = type.TYPE_ID;
                        temp.TYPE_CODE = type.TYPE_CODE;
                    }
                    else
                    {
                        var tp = await _dbContext.Query<BASE_SPTYPE>().Where(t => t.TYPE_NAME == temp.TYPE_NAME).FirstOrDefaultAsync();
                        if (tp == null)
                        {
                            throw new MessageException(temp.TYPE_NAME + " 物资分类不存在，请检查!");

                        }
                        temp.TYPE_NAME = tp.TYPE_NAME;
                        temp.TYPE_ID = tp.TYPE_ID;
                        temp.TYPE_CODE = tp.TYPE_CODE;
                    }

                    temp.MEMO = c.MEMO;
                    temp.APPLY_ID = apply.APPLY_ID;
                    temp.IS_RECOVERY = "0";

                    importResult.Add(temp);

                });
                if (importResult.Count > 0)
                {
                    foreach (var item in importResult)
                    {
                        await BeforeAddDet(item);
                        await Task.CompletedTask;
                        _dbContext.Insert(item);
                    }
                }

                return AjaxResult.Success("成功");
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.Message);
            }

        }

        /// <summary>
        /// 明细-列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SPARE_APPLY_DET>().GetGridData(request);
        }

        /// <summary>
        /// 明细-保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DetailSave(SaveRequest<SPARE_APPLY_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.APPLY_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.MEMO,
                    c.SP_SIZE,
                    c.DRAWING_NO,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.STUFF,
                    c.UNIT,
                    c.IS_SPECIAL,
                    c.IS_WORK,
                    c.IS_STANDARD,
                    c.IS_RECOVERY,
                    c.PRODUCE,
                    c.SP_ID,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.EDIT_USERID,
                    c.EDIT_USER,
                    c.EDIT_DATE,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.IS_STOP,
                    c.IS_RIGGING,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE
                },
                c => a => a.SP_ID == c.SP_ID, BeforeAddDet, BeforeUpdateDet);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAddDet(SPARE_APPLY_DET entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.SP_ID = GuidHelper.NewSnowflakeId().ToString();
            if (string.IsNullOrEmpty(entity.SP_CODE))
            {
                var model = await _dbContext.Query<SPARE_APPLY_DET>(x => x.TYPE_ID == entity.TYPE_ID).Select(x => Sql.Max(x.SP_CODE)).FirstOrDefaultAsync();
                var index = string.IsNullOrEmpty(model) ? 1 : model.Substring(model.Length - 4).CastTo<int>() + 1;
                entity.SP_CODE = $"{entity.TYPE_CODE}-{index.ToString("D4")}";
            }

            if (entity.EDIT_USERID.IsNullOrWhiteSpace())
            {
                entity.EDIT_USERID = _userSession.UserID.ToString();
                entity.EDIT_USER = _userSession.RealName;
            }
            entity.EDIT_DATE = dt;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.ADD_DATE = dt;
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task BeforeUpdateDet(SPARE_APPLY_DET entity)
        {
        }
        #endregion

        #region 物资编码禁用
        /// <summary>
        /// 物资列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SpcatalogListAsync(GridRequest request)
        {
            return await _dbContext.Query<BASE_SPCATALOG>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_NAME,
                c.SP_ID,
                c.SP_NAME,
                c.SP_CODE,
                c.SP_SIZE,
                c.PURTYPE_NAME,
                c.MEMO,
                c.UNIT,
                c.PURTYPE_ID,
                c.PRODUCE,
                c.WARRANTY,
                c.TYPE_CODE,
                c.IS_RECOVERY,
                c.IS_CANCEL,
                c.CREATEDATE,
                c.LAST_PROVIDERID,
                c.LAST_PROVIDER,
                c.STORE_NUM,
                c.STORE_PRICE,
                SEARCH = c.SP_CODE + c.SP_NAME + c.SP_SIZE + c.PRODUCE + c.UNIT + c.TYPE_NAME
            }).GetGridData(request);
        }

        class SpDisableRes : SP_DISABLE
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public int DETAILCOUNT;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SpDisableListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_DISABLE>()
                 .Select(c => new SpDisableRes
                 {
                     DISABLE_ID = c.DISABLE_ID,
                     AUDITING = c.AUDITING,
                     DISABLE_CODE = c.DISABLE_CODE,
                     MEMO = c.MEMO,
                     DISABLE_DATE = c.DISABLE_DATE,
                     EDIT_USERID = c.EDIT_USERID,
                     EDIT_USER = c.EDIT_USER,
                     DEPT_ID = c.DEPT_ID,
                     DEPT_NAME = c.DEPT_NAME,
                     SEC_DEPTID = c.SEC_DEPTID,
                     SEC_DEPT = c.SEC_DEPT,
                     CREATE_USERID = c.CREATE_USERID,
                     CREATEDATE = c.CREATEDATE,
                     MODIFY_USERID = c.MODIFY_USERID,
                     MODIFYDATE = c.MODIFYDATE
                 })
                 .GetGridData(request);
            foreach (var item in (List<SpDisableRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_DISABLE_DET>().Where(t => t.DISABLE_ID == item.DISABLE_ID).Count();
            }
            return res;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SpDisableSave(SaveRequest<SP_DISABLE> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
               c => new
               {
                   c.DISABLE_ID,
                   c.DISABLE_CODE,
                   c.MEMO,
                   c.DISABLE_DATE,
                   c.SEC_DEPTID,
                   c.SEC_DEPT,
                   c.DEPT_ID,
                   c.DEPT_NAME,
                   c.AUDITING,
                   c.EDIT_USER,
                   c.EDIT_USERID,
                   c.CREATE_USERID,
                   c.CREATEDATE,
                   c.MODIFY_USERID,
                   c.MODIFYDATE
               },
               c => a => a.DISABLE_ID == c.DISABLE_ID, SpDisableBeforeAdd, SpDisableBeforeUpdate);
            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].DISABLE_ID;

            return AjaxResult.Success(id);
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task SpDisableBeforeAdd(SP_DISABLE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.DISABLE_ID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            entity.DISABLE_CODE = await _codeCreatorService.CreateCodeAsync<SP_DISABLE>("JY", a => a.DISABLE_CODE);
            if (entity.SEC_DEPTID.IsNullOrWhiteSpace())
            {
                entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
                entity.SEC_DEPT = _userSession.ParentCompany.CName;
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.CName;
            }
            if (!entity.DISABLE_DATE.HasValue)
            {
                entity.DISABLE_DATE = dt;
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            if (entity.EDIT_USERID.IsNullOrWhiteSpace())
            {
                entity.EDIT_USERID = _userSession.UserID.ToString();
                entity.EDIT_USER = _userSession.RealName;
            }
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task SpDisableBeforeUpdate(SP_DISABLE entity)
        {
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<int> SpDisableSubmit(List<string> sids)
        {
            var det = _dbContext.Query<SP_DISABLE_DET>().Where(t => sids.Contains(t.DISABLE_ID)).ToList();
            foreach (var d in det)
            {
                await _dbContext.UpdateAsync<BASE_SPCATALOG>(x => x.SP_ID == d.SP_ID,
                x => new BASE_SPCATALOG
                {
                    IS_CANCEL = "1",
                });
            }

            var updateCount = await _dbContext.UpdateAsync<SP_DISABLE>(x => sids.Contains(x.DISABLE_ID),
                  x => new SP_DISABLE
                  {
                      AUDITING = "1"
                  });
            return updateCount;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> SpDisableDetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_DISABLE_DET>()
                .LeftJoin<BASE_SPCATALOG>((a, b) => a.SP_ID == b.SP_ID).Select((a, b) => new
                {
                    a.SP_ID,
                    a.MEMO,
                    a.DISABLE_DET_ID,
                    a.DISABLE_ID,
                    a.CREATE_USERID,
                    a.CREATEDATE,
                    a.MODIFY_USERID,
                    a.MODIFYDATE,
                    b.SP_CODE,
                    b.SP_NAME,
                    b.SP_SIZE,
                    b.PRODUCE,
                    b.UNIT,
                    b.TYPE_ID
                })
                .GetGridData(request);
        }
        /// <summary>
        /// 禁用明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SpDisableDetailSave(SaveRequest<SP_DISABLE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DISABLE_DET_ID,
                    c.SP_ID,
                    c.MEMO,
                    c.DISABLE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.DISABLE_DET_ID == c.DISABLE_DET_ID, SpDisableBeforeAddDet, SpDisableBeforeUpdateDet);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task SpDisableBeforeAddDet(SP_DISABLE_DET entity)
        {
            entity.DISABLE_DET_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task SpDisableBeforeUpdateDet(SP_DISABLE_DET entity)
        {
        }
        #endregion

        #region 物资编码启用
        class SpEnableRes : SP_ENABLE
        {
            /// <summary>
            /// 填写的明细数量
            /// </summary>
            public int DETAILCOUNT;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SpEnableListAsync(GridRequest request)
        {
            var res = await _dbContext.Query<SP_ENABLE>()
                 .Select(c => new SpEnableRes
                 {
                     ENABLE_ID = c.ENABLE_ID,
                     AUDITING = c.AUDITING,
                     ENABLE_CODE = c.ENABLE_CODE,
                     MEMO = c.MEMO,
                     ENABLE_DATE = c.ENABLE_DATE,
                     EDIT_USERID = c.EDIT_USERID,
                     EDIT_USER = c.EDIT_USER,
                     DEPT_ID = c.DEPT_ID,
                     DEPT_NAME = c.DEPT_NAME,
                     SEC_DEPTID = c.SEC_DEPTID,
                     SEC_DEPT = c.SEC_DEPT,
                     CREATE_USERID = c.CREATE_USERID,
                     CREATEDATE = c.CREATEDATE,
                     MODIFY_USERID = c.MODIFY_USERID,
                     MODIFYDATE = c.MODIFYDATE
                 })
                 .GetGridData(request);
            foreach (var item in (List<SpEnableRes>)res.Rows)
            {
                item.DETAILCOUNT = _dbContext.Query<SP_ENABLE_DET>().Where(t => t.ENABLE_ID == item.ENABLE_ID).Count();
            }
            return res;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SpEnableSave(SaveRequest<SP_ENABLE> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
                 c => new
                 {
                     c.ENABLE_ID,
                     c.ENABLE_CODE,
                     c.MEMO,
                     c.ENABLE_DATE,
                     c.SEC_DEPTID,
                     c.SEC_DEPT,
                     c.DEPT_ID,
                     c.DEPT_NAME,
                     c.AUDITING,
                     c.EDIT_USER,
                     c.EDIT_USERID,
                     c.CREATE_USERID,
                     c.CREATEDATE,
                     c.MODIFY_USERID,
                     c.MODIFYDATE
                 },
                 c => a => a.ENABLE_ID == c.ENABLE_ID, SpEnableBeforeAdd, SpEnableBeforeUpdate);
            var id = "";
            if (request.Added?.Count > 0)
                id = request.Added[0].ENABLE_ID;

            return AjaxResult.Success(id);
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task SpEnableBeforeAdd(SP_ENABLE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.ENABLE_ID = GuidHelper.NewSnowflakeId().ToString();
            //单号
            entity.ENABLE_CODE = await _codeCreatorService.CreateCodeAsync<SP_ENABLE>("QY", a => a.ENABLE_CODE);
            if (entity.SEC_DEPTID.IsNullOrWhiteSpace())
            {
                entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
                entity.SEC_DEPT = _userSession.ParentCompany.CName;
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.CName;
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            if (!entity.ENABLE_DATE.HasValue)
            {
                entity.ENABLE_DATE = dt;
            }
            if (entity.EDIT_USERID.IsNullOrWhiteSpace())
            {
                entity.EDIT_USERID = _userSession.UserID.ToString();
                entity.EDIT_USER = _userSession.RealName;
            }
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task SpEnableBeforeUpdate(SP_ENABLE entity)
        {
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<int> SpEnableSubmit(List<string> sids)
        {
            var det = _dbContext.Query<SP_ENABLE_DET>().Where(t => sids.Contains(t.ENABLE_ID)).ToList();
            foreach (var d in det)
            {
                await _dbContext.UpdateAsync<BASE_SPCATALOG>(x => x.SP_ID == d.SP_ID,
                x => new BASE_SPCATALOG
                {
                    IS_CANCEL = "0",
                });
            }

            var updateCount = await _dbContext.UpdateAsync<SP_ENABLE>(x => sids.Contains(x.ENABLE_ID),
                  x => new SP_ENABLE
                  {
                      AUDITING = "1"
                  });
            return updateCount;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> SpEnableDetailListAsync(GridRequest request)
        {
            return await _dbContext.Query<SP_ENABLE_DET>()
              .LeftJoin<BASE_SPCATALOG>((a, b) => a.SP_ID == b.SP_ID).Select((a, b) => new
              {
                  a.SP_ID,
                  a.MEMO,
                  a.ENABLE_ID,
                  a.ENABLE_DET_ID,
                  a.CREATE_USERID,
                  a.CREATEDATE,
                  a.MODIFY_USERID,
                  a.MODIFYDATE,
                  b.SP_CODE,
                  b.SP_NAME,
                  b.SP_SIZE,
                  b.PRODUCE,
                  b.UNIT,
                  b.TYPE_ID
              })
              .GetGridData(request);
        }
        /// <summary>
        /// 启用明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SpEnableDetailSave(SaveRequest<SP_ENABLE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ENABLE_DET_ID,
                    c.SP_ID,
                    c.MEMO,
                    c.ENABLE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.ENABLE_DET_ID == c.ENABLE_DET_ID, SpEnableBeforeAddDet, SpEnableBeforeUpdateDet);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task SpEnableBeforeAddDet(SP_ENABLE_DET entity)
        {
            entity.ENABLE_DET_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task SpEnableBeforeUpdateDet(SP_ENABLE_DET entity)
        {
        }
        #endregion
    }
}
