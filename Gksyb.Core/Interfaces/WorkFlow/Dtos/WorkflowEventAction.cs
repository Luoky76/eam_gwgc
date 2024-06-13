namespace Gksyb.Core.Interfaces.WorkFlow
{
    public static class WorkflowEventAction
    {
        /// <summary>
        /// 新增节点
        /// </summary>
        public const string AddNode = "WorkflowAddNode";

        /// <summary>
        /// 完成流程任务
        /// </summary>
        public const string ComplateTask = "WorkflowComplateTask";

        /// <summary>
        /// 待办事件
        /// </summary>
        public const string ToDo = "WorkflowToDo";

        /// <summary>
        /// 已办事件
        /// </summary>
        public const string Done = "WorkflowDone";

        /// <summary>
        /// 新增待阅
        /// </summary>
        public const string AddShare = "WorkflowAddShare";
    }
}