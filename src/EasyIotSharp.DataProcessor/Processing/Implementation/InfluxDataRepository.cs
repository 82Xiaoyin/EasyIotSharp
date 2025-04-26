using EasyIotSharp.Core.Repositories.Influxdb;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
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