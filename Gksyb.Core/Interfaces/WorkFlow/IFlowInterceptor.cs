namespace Gksyb.Core.Interfaces.WorkFlow
{
    public interface IFlowInterceptor : IBaseService
    {
        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="taskInfo"></param>
        public Task Intercept(FlowExecuteInfo taskInfo);
    }
}