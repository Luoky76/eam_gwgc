using Gksyb.Common;
using Gksyb.Model.Grid;

namespace EAM.Device.interfaces
{
    public interface IReportService : IService
    {

        Task<GridData> CostReportAsync(string dateFrom, string dateTo);
    }
}