using EasyIotSharp.Core.Repositories.Influxdb;
using EasyIotSharp.GateWay.Core.Socket.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.GateWay.Core.Services
{
    public class InfluxDataRepository : IDataRepository
    {
        public async Task SaveDataPointsAsync(string measurementName, string tenantDatabase, IEnumerable<Dictionary<string, object>> dataPoints)
        {
            var repository = InfluxdbRepositoryFactory.Create<Dictionary<string, object>>(
                measurementName: measurementName,
                tenantDatabase: tenantDatabase
            );

            await repository.BulkInsertAsync(dataPoints);
        }
    }
}
