using Gksyb.Core.Auth;
using Gksyb.Model.WorkFlow;

namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public static class NodeInfoExtensions
    {
        public static IQuery<NodeInfo> SelectNodeInfo<T1, T2>(this IJoinQuery<T1, T2> source) where T1 : WF_NODE where T2 : WF_TASK =>
            source.Select((node, task) => new NodeInfo
            {
                Id = node.ID,
                NodeName = node.NODE_TITLE,
                NodeStatus = node.NODE_STATUS,
                NodeReason = node.NODE_REASON,
                StartDate = node.CREATEDATE,
                ViewDate = node.VIEWDATE,
                FinishDate = node.FINISHDATE,
                Operator = node.NODE_USER,

                TaskId = task.ID,
                Creator = task.CREATEUSER,
                CreateDate = task.CREATEDATE,
                Title = task.FLOW_TITLE,
                TaskFinishFlag = task.FINISHDATE.HasValue ? "1" : "0",
                TaskFinishDate = task.FINISHDATE
            });

        public static IJoinQuery<T1, T2> CorpFilter<T1, T2>(this IJoinQuery<T1, T2> source, UserSession user, bool fromMe = false) where T1 : WF_NODE where T2 : WF_TASK
        {
            var corpId = (user.ParentCompany ?? user.Corp).CorpID;
            return source.Where(user.IsSuper ?
                ((node, task) => task.COMPANY == corpId) :
                fromMe ? ((node, task) => node.NODE_USERID == user.UserID || task.CREATEUSERID == user.UserID) :
                ((node, task) => node.NODE_USERID == user.UserID));
        }
    }
}