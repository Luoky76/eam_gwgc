using Gksyb.Server.Services.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Interfaces.Welcome
{
    public interface IWelcomeService:IService
    {
        #region 顶部数据
        Task<GetDeviceRepairCountResponse> GetDeviceRepairCount(DateTime datetime);

        Task<GetTodoListDataCountResponse> GetTodoListData();

        #endregion
        #region Echart图表数据
        Task<GetDeviceRepairInfoEchartResponse> GetDeviceRepairInfoEchart();

        Task<GetDeviceInfoInMonthResponse> GetDeviceInfoInMonth(int month);

        Task<GetConstructionEchartDataResponse> GetConstructionEchartData();
        #endregion
    }
}
