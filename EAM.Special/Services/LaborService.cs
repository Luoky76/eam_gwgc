using Chloe;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{

    public class LaborService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly UserSession _userSession;
        private string errMsg = string.Empty;

        public LaborService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService, ICodeCreatorService codeCreatorService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "Auditing", null },
                    { "User", null },
                    { "RentState", null }
                });
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
        #region 劳保人员尺码
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> LaborSizeListAsync(string userID)
        {
            var list = await _dbContext.Query<LABOR_SIZE>(x => x.USER_ID == userID).ToListAsync();
            return new GridData { Rows = list, Total = list.Count() };
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveSizeAsync(SaveRequest<LABOR_SIZE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.SIZE_NAME,
                    c.SIZE_ID,
                    c.USER_ID,
                    c.TYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE

                },
                c => a => a.SIZE_ID == c.SIZE_ID
                , BeforeAdd, null, null, false, null, null);
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAdd(LABOR_SIZE entity)
        {
            entity.SIZE_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        #endregion


        #region 劳保人员清单

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> LaborUserCataLogList(string code)
        {

            var list = await _dbContext.Query<BASE_SPCATALOG>(x => x.TYPE_CODE.StartsWith(code)).ToListAsync();

            return new GridData { Rows = list, Total = list.Count };
        }


        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> laborUserListAsync(GridRequest request)
        {
            //               //
            var list = await _dbContext.Query<LABOR_USER>()
                              .LeftJoin<LABOR_SIZE>((a, b) => a.USER_ID == b.USER_ID)
                              .Select((a, b) => new
                              {
                                  a.USER_CODE,
                                  a.USER_NAME,
                                  a.DEPT_ID,
                                  a.DEPT_NAME,
                                  a.USER_ID,
                                  a.SEX,
                                  a.BIRTHDAY,
                                  a.IS_NOVALID,
                                  b.SIZE_ID
                              }).GroupBy(g => new
                              {
                                  g.USER_CODE,
                                  g.USER_NAME,
                                  g.DEPT_ID,
                                  g.DEPT_NAME,
                                  g.USER_ID,
                                  g.SEX,
                                  g.BIRTHDAY,
                                  g.IS_NOVALID,
                              }).Select(x => new
                              {
                                  x.USER_CODE,
                                  x.USER_NAME,
                                  x.DEPT_ID,
                                  x.DEPT_NAME,
                                  x.USER_ID,
                                  x.SEX,
                                  x.BIRTHDAY,
                                  x.IS_NOVALID,
                                  Count = Sql.Count(x.SIZE_ID)
                              }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.USER_SID,
                    c.USER_NAME,
                    c.USER_ID,
                    c.USER_CODE,
                    c.SEX,
                    c.MODIFYDATE,
                    c.MODIFY_USERID,
                    c.IS_NOVALID,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.CREATEDATE,
                    c.CREATE_USERID,
                    c.BIRTHDAY,
                },
                c => a => a.USER_ID == c.USER_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_USER entity)
        {
            entity.USER_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_USER entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_USER entity)
        {
            var sizeList = await _dbContext.DeleteAsync<LABOR_SIZE>(x => x.USER_ID == entity.USER_ID);

            await Task.CompletedTask;

        }


        #endregion

        #region 劳保需求申请
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> laborrequestListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> laborrequestdetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST_DET>().GetGridData(request);
            return list;

        }
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> laborrequestListListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_REQUEST_LIST>().GetGridData(request);
            return list;

        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.REQUEST_CODE,
                    c.REQUEST_DATE,
                    c.REQUEST_MONTH,
                    c.REQUEST_YEAR,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SHIP_NAME,
                    c.SHIP_ID,
                    c.SHIP_CODE,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.REQUEST_TYPE,
                    c.FORM_ID,
                    c.REQUEST_SPTYPE,
                    c.SRC_CODE,
                    c.REQUEST_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_REQUEST entity)
        {
            entity.REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_REQUEST entity)
        {
            await Task.CompletedTask;
        }


        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_REQUEST_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_STATUS,
                    c.SP_CODE,
                    c.SP_DAIMA,
                    c.SP_NAME,
                    c.SP_ENGNAME,
                    c.SP_SIZE,
                    c.SP_TUHAO,
                    c.OTHER_CODE,
                    c.PRODUCE,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.CAN_OUT_NUM,
                    c.MEMO,
                    c.STOCK_ID,
                    c.TYPE_CODE,
                    c.STOCK_NAME,
                    c.STOCK_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.APPLY_USER,
                    c.APPLY_USERID,
                    c.APPLY_ID,
                    c.PURPOSE,
                    c.REQUEST_DET_ID,
                    c.REQUEST_ID,
                    c.SP_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.REQUEST_LIST_ID,
                    c.DEPT_CODE,
                    c.DEPT_NAME,
                    c.USER_CODE,
                    c.USER_NAME,
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_REQUEST_DET entity)
        {
            entity.REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        #endregion

        #region 劳保采购计划
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> laborcollectListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_COLLECT>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_COLLECT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.COLLECT_CODE,
                    c.COLLECT_DATE,
                    c.COLLECT_USER,
                    c.COLLECT_USERID,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.COLLECT_METHOD,
                    c.MEMO,
                    c.COLLECT_PRICE,
                    c.RATIO,
                    c.TAX_MONEY,
                    c.NOTAX_MONEY,
                    c.PROVIDER_CODE,
                    c.PROVIDER_ID,
                    c.PROVIDER_NAME,
                    c.CONSULT_PROVIDER,
                    c.COLLECT_SPTYPE,
                    c.BD_NO,
                    c.COLLECT_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,

                },
                c => a => a.COLLECT_ID == c.COLLECT_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_COLLECT entity)
        {
            entity.COLLECT_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_COLLECT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_COLLECT entity)
        {
            await Task.CompletedTask;
        }


        #endregion


        #region 劳保用品退换
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> LaborExchangeListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<LABOR_EXCHANGE>().GetGridData(request);
            return list;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> GetLaborExchangeAppDetList(string id)
        {
            var result = await _dbContext.Query<LABOR_EXCHANGE_APPDET>(x => x.EXCHANGE_ID.Equals(id)).ToListAsync();
            GridData data = new GridData
            {
                Rows = result,
                Total = result.Count
            };
            return data;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> LaborExchangeSave(SaveRequest<LABOR_EXCHANGE> request, SaveRequest<LABOR_EXCHANGE_APPDET> requestdet)
        {
            //添加主子表新增记录的关联键值
            if (request.Added != null && request.Added.Any())
            {
                string exchange_id;
                if (request.Added[0].EXCHANGE_ID.IsNullOrEmpty())
                {
                    exchange_id = request.Added[0].EXCHANGE_ID = GuidHelper.NewSnowflakeId().ToString();
                }
                else
                {
                    exchange_id = request.Added[0].EXCHANGE_ID;
                }
                foreach (var entity in requestdet.Added)
                {
                    if (entity.EXCHANGE_ID.IsNullOrEmpty())
                    {
                        entity.EXCHANGE_ID = exchange_id;
                    }
                }
            }

            await _dbContext.UseTransactionAsync(async () =>
            {
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.EXCHANGE_CODE,
                         c.EXCHANGE_DATE,
                         c.EXCHANGE_TYPE,
                         c.EXCHANGE_USER,
                         c.EXCHANGE_DEPT,
                         c.MEMO,
                         c.EXCHANGE_ID,
                         c.EXCHANGE_USERID,
                         c.EXCHANGE_DEPTID,
                         c.AUDIT_USERID,
                         c.AUDIT_DEPTID,
                         c.CREATE_USERID,
                         c.CREATEDATE,
                         c.MODIFY_USERID,
                         c.MODIFYDATE,
                         c.EXCHANGE_REASON,
                     },
                     c => a => a.EXCHANGE_ID == c.EXCHANGE_ID
                     , LaborExchangeBeforAdd, LaborExchangeBeforUpdate, LaborExchangeBeforDelete, false, null, null);

                if (execResult.IsError)
                {
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    throw new Exception(errMsg);
                }

                requestdet = requestdet ?? new SaveRequest<LABOR_EXCHANGE_APPDET>();

                execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                     c => new
                     {
                         c.SP_CODE,
                         c.SP_DAIMA,
                         c.SP_NAME,
                         c.SP_SIZE,
                         c.PRODUCE,
                         c.UNIT,
                         c.FACTORY,
                         c.OTHER_CODE,
                         c.EXCHANGE_NUM,
                         c.TYPE_CODE,
                         c.TYPE_NAME,
                         c.PURPOSE,
                         c.MEMO,
                         c.EXCHANGE_APPDET_ID,
                         c.EXCHANGE_ID,
                         c.TYPE_ID,
                         c.SP_ID,
                         c.CREATE_USERID,
                         c.CREATEDATE,
                         c.MODIFY_USERID,
                         c.MODIFYDATE,
                         c.STORE_ID,
                         c.OUT_DET_ID
                     },
                     c => a => a.EXCHANGE_APPDET_ID == c.EXCHANGE_APPDET_ID
                     , LaborExchangeAppDetBeforAdd, LaborExchangeAppDetBeforUpdate, null, false, null, null);

                if (execResult.IsError)
                {
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    throw new Exception(errMsg);
                }
            });
            return AjaxResult.Success("保存成功");
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task LaborExchangeBeforAdd(LABOR_EXCHANGE entity)
        {
            if (entity.EXCHANGE_ID.IsNullOrWhiteSpace())
            {
                entity.EXCHANGE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            var sysDate = await _dbContext.GetSysdate();

            if (entity.EXCHANGE_CODE.IsNullOrWhiteSpace())
            {
                entity.EXCHANGE_CODE = await _codeCreatorService.CreateCodeAsync<LABOR_EXCHANGE>("LBZJ", a => a.EXCHANGE_CODE);
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }

            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task LaborExchangeBeforUpdate(LABOR_EXCHANGE entity)
        {
            var olddata = _dbContext.QueryByKey<LABOR_EXCHANGE>(entity.EXCHANGE_ID);
            if (olddata.AUDITING.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }
        /// <summary>
        /// 删除前处理
        /// </summary>
        private async Task LaborExchangeBeforDelete(LABOR_EXCHANGE entity)
        {
            if (entity.AUDITING.Equals("0"))
                await _dbContext.DeleteAsync<LABOR_RENT_DET>(x => x.RENT_ID.Equals(entity.EXCHANGE_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task LaborExchangeAppDetBeforAdd(LABOR_EXCHANGE_APPDET entity)
        {
            if (entity.EXCHANGE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXCHANGE_ID 为空！");
            }
            if (entity.EXCHANGE_APPDET_ID.IsNullOrWhiteSpace())
            {
                entity.EXCHANGE_APPDET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            var sysDate = await _dbContext.GetSysdate();
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task LaborExchangeAppDetBeforUpdate(LABOR_EXCHANGE_APPDET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysDate;
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<AjaxResult> LaboExchangeGet(string id)
        {
            var mainData = await _dbContext.QueryByKeyAsync<LABOR_EXCHANGE>(id);
            var detData = await _dbContext.Query<LABOR_EXCHANGE_APPDET>(x => x.EXCHANGE_ID.Equals(id)).ToListAsync();
            var result = new
            {
                MainData = mainData,
                DetData = new GridData { Rows = detData, Total = detData.Count }
            };
            return AjaxResult.Success(result);
        }

        #endregion

        #region 劳保用品租借

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> LaborRentList(GridRequest request)
        {

            return await _dbContext.Query<LABOR_RENT>().GetGridData(request);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> GetLaborRentDetList(string rentId)
        {
            var result = await _dbContext.Query<LABOR_RENT_DET>(x => x.RENT_ID.Equals(rentId)).ToListAsync();
            GridData data = new GridData
            {
                Rows = result,
                Total = result.Count
            };
            return data;
        }
        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<AjaxResult> LaborRentGet(string rentId)
        {
            var mainData = await _dbContext.QueryByKeyAsync<LABOR_RENT>(rentId);
            var detData = await _dbContext.Query<LABOR_RENT_DET>(x => x.RENT_ID.Equals(rentId)).ToListAsync();
            var result = new
            {
                MainData = mainData,
                DetData = new GridData { Rows = detData, Total = detData.Count }
            };
            return AjaxResult.Success(result);
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> LaborStoreList(GridRequest request)
        {
            var result = await _dbContext.Query<SP_STORE>().GetGridData(request);
            return result;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet)
        {
            //添加主子表新增记录的关联键值
            if (request.Added != null && request.Added.Any())
            {
                string rent_id;
                if (request.Added[0].RENT_ID.IsNullOrEmpty())
                {
                    rent_id = request.Added[0].RENT_ID = GuidHelper.NewSnowflakeId().ToString();
                }
                else
                {
                    rent_id = request.Added[0].RENT_ID;
                }
                foreach (var entity in requestdet.Added)
                {
                    if (entity.RENT_ID.IsNullOrEmpty())
                    {
                        entity.RENT_ID = rent_id;
                    }
                }
            }

            await _dbContext.UseTransactionAsync(async () =>
            {
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.RENT_CODE,
                         c.RENT_DATE,
                         c.RENT_DEPT,
                         c.RENT_USER,
                         c.DEPT_NAME,
                         c.USER_NAME,
                         c.BEGIN_DATE,
                         c.END_DATE,
                         c.RENT_REASON,
                         c.MEMO,
                         c.RENT_ID,
                         c.RENT_DEPTID,
                         c.RENT_USERID,
                         c.DEPT_ID,
                         c.USER_ID,
                         c.EXPEND_DATE,
                         c.RENT_STATUS
                     },
                     c => a => a.RENT_ID == c.RENT_ID
                     , LaborRentBeforAdd, LaborRentBeforUpdate, LaborRentBeforDelete, false, null, null);

                if (execResult.IsError)
                {
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    throw new Exception(errMsg);
                }

                requestdet = requestdet ?? new SaveRequest<LABOR_RENT_DET>();

                execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                     c => new
                     {
                         c.SP_CODE,
                         c.SP_DAIMA,
                         c.SP_NAME,
                         c.SP_SIZE,
                         c.PRODUCE,
                         c.UNIT,
                         c.FACTORY,
                         c.OTHER_CODE,
                         c.RENT_NUM,
                         c.TYPE_CODE,
                         c.TYPE_NAME,
                         c.MEMO,
                         c.RENT_DET_ID,
                         c.RENT_ID,
                         c.TYPE_ID,
                         c.SP_ID,
                         c.STORE_ID,
                         c.HOUSE_ID
                     },
                     c => a => a.RENT_DET_ID == c.RENT_DET_ID
                     , LaborRentDetBeforAdd, LaborRentDetBeforUpdate, null, false, null, null);

                if (execResult.IsError)
                {
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    throw new Exception(errMsg);
                }
            });
            return AjaxResult.Success("保存成功");
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task LaborRentBeforAdd(LABOR_RENT entity)
        {
            if (entity.RENT_ID.IsNullOrWhiteSpace())
            {
                entity.RENT_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            var sysDate = await _dbContext.GetSysdate();

            if (entity.RENT_CODE.IsNullOrWhiteSpace())
            {
                entity.RENT_CODE = await _codeCreatorService.CreateCodeAsync<LABOR_RENT>("LBZJ", a => a.RENT_CODE);
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            if (entity.RENT_STATUS.IsNullOrWhiteSpace())
            {
                entity.RENT_STATUS = "0";
            }
            if (entity.USER_ID.IsNullOrWhiteSpace())
            {
                entity.USER_ID = _userSession.UserID.ToString();
                entity.USER_NAME = _userSession.RealName;
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.Corp.CorpID;
                entity.DEPT_NAME = _userSession.Corp.CName;
            }
            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task LaborRentBeforUpdate(LABOR_RENT entity)
        {
            var model = await _dbContext.QueryByKeyAsync<LABOR_RENT>(entity.RENT_ID);
            if (model.AUDITING.Equals("0"))
            {
                var sysDate = await _dbContext.GetSysdate();
                entity.MODIFY_USERID = _userSession.UserID.ToString();
                entity.MODIFYDATE = sysDate;
            }
            else
            {
                errMsg = "未提交的状态下才能修改";
                throw new MessageException("未提交的状态下才能修改");
            }
        }
        /// <summary>
        /// 删除前处理
        /// </summary>
        private async Task LaborRentBeforDelete(LABOR_RENT entity)
        {
            if (entity.AUDITING.Equals("0"))
                await _dbContext.DeleteAsync<LABOR_RENT_DET>(x => x.RENT_ID.Equals(entity.RENT_ID));
            else
            {
                errMsg = "未提交的状态下才能删除";
                throw new MessageException("未提交的状态下才能删除");
            }
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task LaborRentDetBeforAdd(LABOR_RENT_DET entity)
        {
            if (entity.RENT_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 RENT_ID 为空！");
            }
            if (entity.RENT_DET_ID.IsNullOrWhiteSpace())
            {
                entity.RENT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            var sysDate = await _dbContext.GetSysdate();

            entity.CREATE_USERID = entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = entity.MODIFYDATE = sysDate;
        }
        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task LaborRentDetBeforUpdate(LABOR_RENT_DET entity)
        {
            var sysDate = await _dbContext.GetSysdate();
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = sysDate;
        }
        #endregion
    }
}
