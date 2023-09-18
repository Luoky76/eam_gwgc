using Gksyb.Core.Interfaces.WorkFlow;

namespace Gksyb.Workflow.Controllers.Api.Dtos
{
    public class FlowRequest : FlowExecuteInfo
    {
        /// <summary>
        /// 工号
        /// </summary>
        public string WorkerCode { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }
    }
}