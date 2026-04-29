#pragma warning disable CA1822 // 将成员标记为 static

using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(MenuNo = "EmployeeManage")]
    public class EmployeeController : BaseController
    {
        private readonly EmployeeService _employeeService;
        private readonly UserService _service;

        public EmployeeController(EmployeeService employeeService, UserService userService)
        {
            _employeeService = employeeService;
            _employeeService.AddParentCompanys().Result();
            _service = userService;
        }

        /// <summary>
        /// 树形结构
        /// </summary>

        public async Task<AjaxResult> TreeAsync()
        {
            var data = await _employeeService.TreeAsync();
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxData([FromServices] IBCCodeService codeService)
        {
            return AjaxResult.Success(new
            {
                role = (await _service.RoleData()).Rows,
                corp = (await _service.CorpData()).Rows,
                station = await codeService.Get("岗位")
            });
        }

        /// <summary>
        /// 角色数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> RoleData(GridRequest request)
        {
            return AjaxResult.Success(await _service.RoleData(request));
        }

        /// <summary>
        /// 公司数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> CorpData(GridRequest request)
        {
            return AjaxResult.Success(await _service.CorpData(request));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult<GridData>> ListAsync(UserRequest user, GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(user, request));
        }

        /// <summary>
        /// 初始化密码
        /// </summary>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> DoInitPassword(long? id)
        {
            return AjaxResult.Success(await _service.DoInitPassword(id), default);
        }

        /// <summary>
        /// 获取同名用户信息
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> GetSameEmployeeAsync(UserRequest request)
        {
            var gridData = await _employeeService.GetSameEmployeeAsync(request);
            var ids = gridData.Rows.Select(c => c.CORP).Join(",").Split(',').DistinctAndOrderBy().ToList();
            var corps = await _employeeService.CorpsAsync(ids);
            var data = new Dictionary<string, object>()
            {
                { "Employees",gridData},
                { "Corps",corps}
            };
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> SaveAsync([FromBody] SaveRequest<UserRequest> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }
    }
}