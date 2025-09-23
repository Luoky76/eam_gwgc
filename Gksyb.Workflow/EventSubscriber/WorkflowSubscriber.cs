using Gksyb.Common.EventBus;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.WorkFlow;

namespace Gksyb.Workflow.EventSubscriber
{
    public class WorkflowSubscriber : IEventSubscriber
    {
        private readonly IMessageCenterService _service;

        public WorkflowSubscriber(IMessageCenterService service)
        {
            _service = service;
        }

        [EventSubscribe(WorkflowEventAction.AddNode)]
        public async Task AddToDoAsync(FlowEventInfo info)
        {
            if (info.NodeInfos == null || info.NodeInfos.Count < 1) return;
            foreach (var node in info.NodeInfos)
            {
                await _service.SendAsync(new MessageInfo
                {
                    Code = "ToDo",
                    Title = "您有一条新的待办",
                    Content = info.Title,
                    Key = node.Id,
                    Receives = new List<string>() { node.NodeUserName },
                    Data = node,
                    Appname = info.AppName
                }, true);
            }
        }

        [EventSubscribe(WorkflowEventAction.CompleteTask)]
        public async Task ComplateTaskAsync(FlowEventInfo info)
        {
            if (info.NodeInfos == null || info.NodeInfos.Count < 1) return;
            foreach (var node in info.NodeInfos)
            {
                await _service.SendAsync(new MessageInfo
                {
                    Code = "ToDoComplate",
                    Title = info.FlowName,
                    Content = $"您的《{info.Title}》申请已审批，结果为{NodeStatus.GetDesc(node?.NodeStatus)}",
                    Key = node.Id,
                    Receives = new List<string>() { node.NodeUserName },
                    Data = node,
                    Appname = info.AppName
                }, true);
            }
        }

        [EventSubscribe(WorkflowEventAction.AddShare)]
        public async Task AddShareAsync(FlowEventInfo info)
        {
            if (info.NodeInfos == null || info.NodeInfos.Count < 1) return;
            foreach (var node in info.NodeInfos)
            {
                await _service.SendAsync(new MessageInfo
                {
                    Code = "ToRead",
                    Title = info.FlowName,
                    Content = "您有一条新的待阅",
                    Key = node.Id,
                    Receives = new List<string>() { node.NodeUserName },
                    Data = node,
                    Appname = info.AppName
                }, true);
            }
        }
    }
}