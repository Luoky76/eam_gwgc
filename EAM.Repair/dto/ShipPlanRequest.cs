using System;

namespace EAM.Repair.dto
{
    public class ShipPlanRequest
    {
        //public REP_PLAN REP_PLAN { get; set; }
        //public DEVICE_CARD DEVICE_CARD { get; set; }

        public string AUDITING { get; set; }
        public string PLAN_CODE { get; set; }
        public string PLAN_STATE { get; set; }
        public string DEVICE_NAME { get; set; }
        public string DEVICE_CODE { get; set; }
        public string DEVICE_TYPE { get; set; }
        public string ASSET_CODE { get; set; }
        public string DEPT_NAME { get; set; }
        public string WSEC_DEPT { get; set; }
        public string MAINT_TYPE { get; set; }
        public string DEAL_TYPE { get; set; }
        public string INSTALL_SITE { get; set; }
        public string FAULT_DESCRIBE { get; set; }
        public string PLAN_MEMO { get; set; }
        public DateTime? PLAN_START_DATE { get; set; }
        public DateTime? PLAN_END_DATE { get; set; }
        public decimal? PLAN_STOP_TIME { get; set; }
        public string CHARGE_USER { get; set; }
        public string COLLECT_METHOD { get; set; }
        public decimal? PLAN_MONEY { get; set; }
        public string REPAIR_MEMO { get; set; }
    }
}
