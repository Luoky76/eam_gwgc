namespace Gksyb.Common.Quartz.Dtos
{
    /// <summary>
    /// 任务调度
    /// </summary>
    public class QuartzTask
    {
        /// <summary>
        /// 任务调用方法
        /// </summary>
        public long TaskID { get; set; }

        /// <summary>
        /// 任务调用方法
        /// </summary>
        public string TaskMethod { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string TaskGroup { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string TaskDesc { get; set; }

        /// <summary>
        ///任务间隔（cron表达式）
        /// </summary>
        public string TaskCron { get; set; }

        /// <summary>
        ///任务数据
        /// </summary>
        public string TaskData { get; set; }

        /// <summary>
        ///任务视图
        /// </summary>
        public string TaskView { get; set; }

        /// <summary>
        ///服务器IP
        /// </summary>
        public string TaskIP { get; set; }

        /// <summary>
        ///错误匹配
        /// </summary>
        public string TaskErrorMatch { get; set; }

        /// <summary>
        ///错误回调
        /// </summary>
        public string TaskErrorMethod { get; set; }

        /// <summary>
        ///最后运行时间
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        ///最后运行结果
        /// </summary>
        public string LastRunResult { get; set; }

        /// <summary>
        ///最后运行IP
        /// </summary>
        public string LastRunIP { get; set; }

        /// <summary>
        ///最后执行的数据标识
        /// </summary>
        public string LastKey { get; set; }

        /// <summary>
        ///耗时(毫秒)
        /// </summary>
        public int? ElapsedTime { get; set; }

        /// <summary>
        ///运行状态
        /// </summary>
        public string RunStatus { get; set; }

        /// <summary>
        /// 是否停止
        /// </summary>
        public bool IsStop { get; set; }

        /// <summary>
        /// 本次是否执行
        /// </summary>
        public bool IsExcuted { get; set; }
    }
}