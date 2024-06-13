using Gksyb.Core.Interfaces.WorkFlow;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    [ServiceLifetime]
    public class BpmnUserTaskService : BpmnNodeService, IBaseService
    {
        public BpmnUserTaskService(IDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {
            _isTask = true;
        }

        protected override async Task Exec(FlowExecuteInfo info)
        {
            await AddTask(info);
            await ComplatePreviousTask(info, Inputs);
        }
    }
}