using Gksyb.Core.Interfaces.General;
using System.Linq.Expressions;

namespace Gksyb.Core.Services.General
{
    public class CodeCreatorService : IBaseService, ICodeCreatorService
    {
        private readonly IDbContext _dbContext;

        public CodeCreatorService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 生成以headCode开头的单号
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="headCode">单号前缀</param>
        /// <param name="selectExp">单号字段</param>
        /// <returns>生成的单号</returns>
        public async Task<string> CreateCodeAsync<T>(string headCode, Expression<Func<T, string>> selectExp)
        {
            var sysdate = await _dbContext.GetSysdate();
            string dateCode = sysdate.Value.ToString("yyyyMM");
            string newCode = headCode + dateCode + "0001";
            string model = await _dbContext.Query<T>()
                .Select(selectExp)
                .Where(x => x.StartsWith(headCode + dateCode))
                .MaxAsync(x => x);
            if (model.IsNullOrEmpty() || model[..(headCode.Length + dateCode.Length)] != headCode + dateCode)
            {
                return newCode;
            }
            return headCode + dateCode + (long.Parse(model[(headCode.Length + dateCode.Length)..]) + 1).ToString("D4");
        }
    }
}