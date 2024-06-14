using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 基础服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public class BaseController<T> : BaseController
    {
        private IService<T> _service;

        protected IService<T> Service
        {
            get
            {
                _service ??= HttpContext.RequestServices.GetRequiredService<IService<T>>();
                return _service;
            }
            set
            {
                _service = value;
            }
        }

        public BaseController(IService<T> service = null)
        {
            _service = service;
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public virtual async Task<AjaxResult<T>> GetAsync(string id)
        {
            return AjaxResult<T>.Success(await Service.GetAsync(id));
        }

        [HttpPost]
        public virtual async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await Service.ListAsync(request));
        }

        [HttpPost]
        [JsToken]
        public virtual async Task<AjaxResult> SaveAsync(SaveRequest<T> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await Service.SaveAsync(request);
        }
    }

    [Route("")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private UserSession _user;

        /// <summary>
        /// 获取登陆用户信息
        /// </summary>
        public UserSession CurrentUser
        {
            get
            {
                if (_user != null) return _user;
                _user = HttpContext.GetCurrentUserAsync().Result();
                return _user;
            }
        }

        private DateTime? _sysdate;

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        protected DateTime? Sysdate
        {
            get
            {
                if (_sysdate.HasValue) return _sysdate;
                _sysdate = HttpContext.RequestServices.GetService<IDbContext>().GetSysdate().Result();
                return _sysdate;
            }
        }

        /// <summary>
        /// 保存验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="menuNo"></param>
        /// <returns></returns>
        public async Task<AjaxResult> ValidSaveAsync<T>(SaveRequest<T> request, string menuNo = null)
        {
            bool isValid;
            var authorize = new GksybAuthorizeAttribute(menuNo);
            if (string.IsNullOrWhiteSpace(menuNo))
            {
                var attribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetAttribute<GksybAuthorizeAttribute>();
                if (attribute != null)
                {
                    authorize = attribute;
                }
                else
                {
                    menuNo = ControllerContext.ActionDescriptor.ControllerTypeInfo.Name;
                    authorize.MenuNo = Regex.Replace(menuNo, @"Controller$", "", RegexOptions.IgnoreCase);
                }
            }
            if (request.Added?.Count > 0)
            {
                authorize.BtnNo = GksybAuthorizeAttribute.AddBtn;
                isValid = await authorize.ValidAsync(HttpContext);
                if (!isValid) return AjaxResult.Error(ForbidText);
            }
            if (request.Updated?.Count > 0)
            {
                authorize.BtnNo = GksybAuthorizeAttribute.UpdateBtn;
                isValid = await authorize.ValidAsync(HttpContext);
                if (!isValid) return AjaxResult.Error(ForbidText);
            }
            if (request.Deleted?.Count > 0)
            {
                authorize.BtnNo = GksybAuthorizeAttribute.DeleteBtn;
                isValid = await authorize.ValidAsync(HttpContext);
                if (!isValid) return AjaxResult.Error(ForbidText);
            }
            return AjaxResult.Success();
        }

        /// <summary>
        /// 禁止提示
        /// </summary>
        private const string ForbidText = "您无权进行此操作";
    }
}