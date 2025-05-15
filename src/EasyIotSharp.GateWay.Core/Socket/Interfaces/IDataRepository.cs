using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.GateWay.Core.Socket.Interfaces
{
    public interface IDataRepository
    {
        Task SaveDataPointsAsync(string measurementName, string tenantDatabase, IEnumerable<Dictionary<string, object>> dataPoints);
    }
}
