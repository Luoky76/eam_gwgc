using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Services.Dto
{
    public  class GetTodoListDataCountResponse
    {


        /// <summary>
        /// 维修计划(维修实施 未提交数据) REP_PLAN_EXE
        /// </summary>
        public int exe { get; set; }
        public string exeTitle { get; set; }
        public List<todolist> exeList { get; set; }
        /// <summary>
        /// 维修计划验收  REP_PLAN_EXE  
        /// </summary>
        public int check { get; set; }
        public string checkTitle { get; set; }
        public List<todolist> checkList { get; set; }
        /// <summary>
        /// 委外维修确认 REP_OUT
        /// </summary>
        public int ExtCheck { get; set; }
        public string ExtCheckTitle { get; set; }
        public List<todolist> ExtCheckList { get; set; }
        /// <summary>
        /// 委外维修验收 REP_OUT  auting =1
        /// </summary>
        public int ExtMainteCheck { get; set; }

        public string ExtMainteCheckTitle { get; set; }
        public List<todolist> ExtMainteCheckList { get; set; }
        /// <summary>
        /// 码头维修实施 REP_DOCK_PLAN  c.AUDITING_PLAN=="1"
        /// </summary>
        public int RepDockExe { get; set; }
        public string RepDockExeTitle { get; set; }
        public List<todolist> RepDockExeList { get; set; }
        /// <summary>
        /// 码头维修确认  REP_DOCK_CHECK
        /// </summary>
        public int RepDockConfirm { get; set; }
        public string RepDockConfirmTitle { get; set; }
        public List<todolist> RepDockConfirmList { get; set; }
        /// <summary>
        /// 维保实施 PM_PLAN_EXE AUDITING=="1"
        /// </summary>
        public int PmPlanExe { get; set; }
        public string PmPlanExeTitle { get; set; }
        public List<todolist> PmPlanExeList { get; set; }
    }
}
