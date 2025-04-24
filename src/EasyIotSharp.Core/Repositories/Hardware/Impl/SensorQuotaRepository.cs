using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Validators.PredicateValidator;

namespace EasyIotSharp.Core.Repositories.Hardware.Impl
{
    public class SensorQuotaRepository : MySqlRepositoryBase<SensorQuota, string>, ISensorQuotaRepository
    {
        public SensorQuotaRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }
        public async Task<(int totalCount, List<SensorQuota> items)> Query(int tenantNumId,
                                                                               string sensorId,
                                                                               string keyword,
                                                                               DataTypeMenu dataType,
                                                                               int pageIndex,
                                                                               int pageSize,
                                                                               bool isPage)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<SensorQuota>(t => t.IsDelete == false);
            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }
            if (!string.IsNullOrWhiteSpace(sensorId))
            {
                predicate = predicate.And(t => t.SensorId.Equals(sensorId));
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                predicate = predicate.And(t => t.Name.Contains(keyword) || t.Identifier.Contains(keyword));
            }
            if (dataType!= DataTypeMenu.None)
            {
                predicate = predicate.And(t => t.DataType.Equals(dataType));
            }


            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<SensorQuota>());
            }

            if (isPage == true)
            {
                var query = Client.Queryable<SensorQuota>().Where(predicate)
                                  .OrderBy(t => t.Sort,OrderByType.Desc)
                                  .OrderBy(m => m.CreationTime, OrderByType.Desc)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
            else
            {
                var query = Client.Queryable<SensorQuota>().Where(predicate)
                                  .OrderBy(t => t.Sort, OrderByType.Desc)
                                  .OrderBy(m => m.CreationTime, OrderByType.Desc);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
        }
        /// <summary>
        /// 根据id获取单条指标记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SensorQuota> GetSensorQuotaList(string id)
        {
            return Client.Queryable<SensorQuota>().Where(w => w.SensorId.Equals(id))
                                 .OrderBy(t => t.Sort, OrderByType.Desc).ToList();
        }
    }
}
