using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Validators.PredicateValidator;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    public class SensorPointRepository : MySqlRepositoryBase<SensorPoint, string>, ISensorPointRepository
    {
        public SensorPointRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public async Task<(int totalCount, List<SensorPoint> items)> Query(int tenantNumId,
                                                                           string keyword,
                                                                           string projectId,
                                                                           string classificationId,
                                                                           string gatewayId,
                                                                           string sensorId,
                                                                           int pageIndex,
                                                                           int pageSize,
                                                                           bool isPage)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<SensorPoint>(t => t.IsDelete == false);

            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                predicate = predicate.And(t => t.Name.Contains(keyword));
            }
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId.Equals(projectId));
            }
            if (!string.IsNullOrWhiteSpace(classificationId))
            {
                predicate = predicate.And(t => t.ClassificationId.Equals(classificationId));
            }
            if (!string.IsNullOrWhiteSpace(gatewayId))
            {
                predicate = predicate.And(t => t.GatewayId.Equals(gatewayId));
            }

            if (!string.IsNullOrWhiteSpace(sensorId))
            {
                predicate = predicate.And(t => t.SensorId.Equals(sensorId));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<SensorPoint>());
            }

            if (isPage == true)
            {
                var query = Client.Queryable<SensorPoint>().Where(predicate)
                                  .OrderBy(m => m.CreationTime, OrderByType.Desc)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
            else
            {
                var query = Client.Queryable<SensorPoint>().Where(predicate)
                  .OrderBy(m => m.CreationTime, OrderByType.Desc);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
        }

        /// <summary>
        /// 获取全部数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<SensorPoint>> QueryList(string projectId)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<SensorPoint>(t => t.IsDelete == false);
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId.Equals(projectId));
            }

            return await Client.Queryable<SensorPoint>().Where(predicate).ToListAsync();
        }
    }
}
