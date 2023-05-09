using Gksyb.Common.EventBus;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Workflow.EventSubscriber.Dtos;

namespace Gksyb.Workflow.EventSubscriber
{
    public class WorkflowSubscriber : IEventSubscriber
    {
        private readonly IMessageCenterService _service;

        public WorkflowSubscriber(IMessageCenterService service)
        {
            _service = service;
        }

        [EventSubscribe(WorkflowEventAction.AddTask)]
        public async Task AddTaskAsync(MessageInfo info)
        {
            info.Code = "ToDo";
            info.Title = $"您有一条新的待办";
            await _service.SendAsync(info, true);
        }

        [EventSubscribe(WorkflowEventAction.ComplateTask)]
        public async Task ComplateTaskAsync(MessageInfo info)
        {
            info.Code = "ToDoComplate";
            info.Content = $"您的《{info.Title}》申请已审批，结果为{info.Data}";
            await _service.SendAsync(info, true);
        }

        [EventSubscribe(WorkflowEventAction.AddShare)]
        public async Task AddShareAsync(MessageInfo info)
        {
            info.Code = "ToRead";
            info.Content = $"您有一条新的待阅";
            await _service.SendAsync(info, true);
        }
    }
}