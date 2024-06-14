using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IReportService : IService
    {

        Task<GridData> CostReportAsync(string dateFrom, string dateTo);
    }
}