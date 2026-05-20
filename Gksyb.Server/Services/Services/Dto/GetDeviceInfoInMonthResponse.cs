namespace Gksyb.Server.Services.Services.Dto
{
    public class GetDeviceInfoInMonthResponse
    {
        public List<string> DeviceNameList { get; set; }
        public List<int> RepairList { get; set; }
        public List<decimal> HourList { get; set; }
    }
}
