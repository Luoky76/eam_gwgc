using Gksyb.Common.EventBus;
using Gksyb.Common.Static;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.WorkFlow.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程节点执行信息
    /// </summary>
    public partial class FlowExecuteInfo
    {
        /// <summary>
        /// 流程名称
        /// </summary>
        [JsonIgnore]
        public string FlowName { get; set; }

        /// <summary>
        /// 所属组 
        /// </summary>
        [JsonIgnore]
        public string FlowGroup { get; set; }

        /// <summary>
        /// 流程表单主键名称
        /// </summary>
        [JsonIgnore]
        public string KeyName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [JsonIgnore]
        public string Title { get; set; }

        /// <summary>
        /// 节点处理人
        /// </summary>
        [JsonIgnore]
        public long? NodeUserId { get; set; }

        /// <summary>
        /// 临时目标节点信息，自动流转用
        /// </summary>
        [JsonIgnore]
        public List<NodeInfo> ToNodeInfos { get; set; } = new List<NodeInfo>();

        /// <summary>
        /// 目标节点
        /// </summary>
        [JsonIgnore]
        public string ToNode { get; set; }

        /// <summary>
        /// 是否拥有目标节点,简化写法
        /// </summary>
        [JsonIgnore]
        public bool ToNodeIsEmpty => string.IsNullOrWhiteSpace(ToNode);

        /// <summary>
        /// 下一节点处理人
        /// </summary>
        [JsonIgnore]
        public List<UserInfo> Users = null;

        /// <summary>
        /// 程序名
        /// </summary>
        [JsonIgnore]
        public string AppName { get; set; }

        /// <summary>
        /// 实际标题（用于通知标题的展示）
        /// </summary>
        [JsonIgnore]
        public string RealTitle { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        [JsonIgnore]
        public string Creator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonIgnore]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 本次执行的事件
        /// </summary>
        [JsonIgnore]
        public List<ActionData> Events { get; set; } = new List<ActionData>();

        /// <summary>
        /// 待办列表
        /// </summary>
        [JsonIgnore]
        public List<NodeInfo> ToDos { get; set; } = new List<NodeInfo>();

        /// <summary>
        /// 已办列表
        /// </summary>
        [JsonIgnore]
        public List<NodeInfo> Dones { get; set; } = new List<NodeInfo>();

        /// <summary>
        /// 获取任务主键
        /// </summary>
        public string GetTaskKey(string defaultId = null)
        {
            defaultId ??= TaskId;
            if (FormData == null) return defaultId;
            var key = string.IsNullOrWhiteSpace(KeyName) ? "key" : KeyName;
            key = FormData.ContainsKey(key) ? key :
                FormData.ContainsKey("key") ? "key" : "id";
            if (!FormData.ContainsKey(key)) return defaultId;
            var value = (FormData[key] ?? "").CastTo<string>();
            return string.IsNullOrWhiteSpace(value) ? defaultId : value;
        }

        /// <summary>
        /// 需要通知的待办（去除在已办列表的数据）
        /// </summary>
        /// <returns></returns>
        public List<NodeInfo> NoticeToDos() => ToDos.Where(c => !Dones.Any(a => a.Id == c.Id)).ToList();

        /// <summary>
        /// 需要通知的已办（去除在待办列表的数据）
        /// </summary>
        /// <returns></returns>
        public List<NodeInfo> NoticeDones() => Dones.Where(c => !ToDos.Any(a => a.Id == c.Id)).ToList();

        /// <summary>
        /// 发布事件
        /// </summary>
        public async Task PublishEvent()
        {
            Events ??= new List<ActionData> { };
            ToDos ??= new List<NodeInfo>();
            Dones ??= new List<NodeInfo>();
            var eventPublisher = HttpContext.RequestServices.GetService<IEventPublisher>();
            foreach (var item in Events)
            {
                await eventPublisher.PublishAsync(item);
            }
            var info = FlowEventInfo.FromFlowExecuteInfo(this);
            info.NodeInfos = NoticeToDos();
            if (info.NodeInfos.Count > 0)
            {
                await eventPublisher.PublishAsync(new ActionData<FlowEventInfo>()
                {
                    Action = WorkflowEventAction.AddNode,
                    Data = info
                });
                await eventPublisher.PublishAsync(new ActionData<FlowEventInfo>()
                {
                    Action = WorkflowEventAction.ToDo,
                    Data = info
                });
            }
            info.NodeInfos = NoticeDones();
            if (info.NodeInfos.Count > 0)
            {
                await eventPublisher.PublishAsync(new ActionData<FlowEventInfo>()
                {
                    Action = WorkflowEventAction.Done,
                    Data = info
                });
            }
        }
    }
}