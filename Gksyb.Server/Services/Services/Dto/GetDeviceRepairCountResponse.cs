namespace Gksyb.Server.Services.Services.Dto
{
    public class GetDeviceRepairCountResponse
    {
        /// <summary>
        /// 设备数量
        /// </summary>
        public int deviceCount { get; set; }

        /// <summary>
        /// 维修数量
        /// </summary>
        public int repairCount { get; set; }

        /// <summary>
        /// 码头维修数量
        /// </summary>
        public int shiprepairCount { get; set; }

    }
}
