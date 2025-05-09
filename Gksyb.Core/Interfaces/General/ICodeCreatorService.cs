using System.Linq.Expressions;

namespace Gksyb.Core.Interfaces.General
{
    public interface ICodeCreatorService : IService
    {
        /// <summary>
        /// 生成以headCode开头的单号
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="headCode">单号前缀</param>
        /// <param name="selectExp">单号字段</param>
        /// <returns>生成的单号</returns>
        Task<string> CreateCodeAsync<T>(string headCode, Expression<Func<T, string>> selectExp);
    }
}