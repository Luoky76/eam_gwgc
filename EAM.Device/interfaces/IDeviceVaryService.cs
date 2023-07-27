using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.Interfaces
{
    public interface IDeviceVaryService : IService
    {
        Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        Task<GridData> ListAsync(GridRequest request);
    }
}
