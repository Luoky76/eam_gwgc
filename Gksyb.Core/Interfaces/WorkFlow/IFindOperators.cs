using Gksyb.Core.Interfaces.Auth;

namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 自定义处理人
    /// </summary>
    public interface IFindOperators : IBaseService
    {
        /// <summary>
        /// 获取自定义处理人
        /// </summary>
        /// <param name="taskInfo"></param>
        public Task<List<UserInfo>> Find(FlowExecuteInfo taskInfo);
    }
}