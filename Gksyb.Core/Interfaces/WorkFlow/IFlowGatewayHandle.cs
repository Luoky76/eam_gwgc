namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 网关处理接口
    /// </summary>
    public interface IFlowGatewayHandle : IBaseService
    {
        /// <summary>
        /// 执行网关处理
        /// </summary>
        /// <param name="taskInfo"></param>
        public Task<string> Eval(FlowExecuteInfo taskInfo);
    }
}