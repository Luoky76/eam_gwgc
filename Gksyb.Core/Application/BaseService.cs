using Chloe.Descriptors;
using Chloe.Infrastructure;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Grid;

namespace Gksyb.Core.Application
{
    public class BaseService<T> : IService<T>
    {
        protected readonly IDbContext _dbContext;
        private DateTime? _sysdate;

        public BaseService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public virtual async Task<T> GetAsync(object id)
        {
            var typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));
            if (typeDescriptor.PrimaryKeys.Count != 1) throw new MessageException($"{nameof(T)}主键不唯一");
            PrimitivePropertyDescriptor keyDescriptor = typeDescriptor.PrimaryKeys[0];
            var value = id.CastTo(keyDescriptor.PropertyType);
            return await _dbContext.QueryByKeyAsync<T>(value);
        }

        /// <inheritdoc />
        public virtual async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<T>().GetGridData(request);
        }

        /// <inheritdoc />
        public virtual Task<AjaxResult> SaveAsync(SaveRequest<T> request)
        {
            throw new MessageException("请实现保存方法");
        }

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        public DateTime? Sysdate
        {
            get
            {
                if (!_sysdate.HasValue) _sysdate = _dbContext.GetSysdate().Result();
                return _sysdate;
            }
        }
    }

    /// <summary>
    /// 基础服务
    /// </summary>
    public class BaseService : IBaseService
    {
        protected UserSession CurrentUser;

        public void SetUser(UserSession user)
        {
            CurrentUser = user;
        }
    }
}