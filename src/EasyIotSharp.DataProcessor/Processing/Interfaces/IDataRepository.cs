using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    public interface IDataRepository
    {
        Task SaveDataPointsAsync(string measurementName, string tenantDatabase, IEnumerable<Dictionary<string, object>> dataPoints);
    }
}