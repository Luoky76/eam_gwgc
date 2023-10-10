using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Services.Dto
{
    public class GetDeviceInfoInMonthResponse
    {
        public List<string> DeviceNameList { get; set; }
        public List<int> RepairList { get; set; }
        public List<decimal> HourList { get; set; }
    }
}
