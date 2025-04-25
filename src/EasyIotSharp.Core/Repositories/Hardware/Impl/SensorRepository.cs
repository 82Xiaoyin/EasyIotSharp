using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Validators.PredicateValidator;

namespace EasyIotSharp.Core.Repositories.Hardware.Impl
{
    public class SensorRepository : MySqlRepositoryBase<Sensor, string>, ISensorRepository
    {
        public SensorRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public async Task<(int totalCount, List<Sensor> items)> Query(int tenantNumId,
                                                                      string keyword,
                                                                      int pageIndex,
                                                                      int pageSize,
                                                                      bool isPage)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<Sensor>(t => t.IsDelete == false);
            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }
            if (!string.IsNullOrWhiteSpace(keyword)) 
            {
                predicate = predicate.And(t => t.Name.Contains(keyword) || t.BriefName.Contains(keyword) || t.Supplier.Contains(keyword));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<Sensor>());
            }

            if (isPage == true)
            {
                var query = Client.Queryable<Sensor>().Where(predicate)
                                  .OrderBy(m => m.CreationTime, OrderByType.Desc)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
            else
            {
                var query = Client.Queryable<Sensor>().Where(predicate)
                  .OrderBy(m => m.CreationTime, OrderByType.Desc);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
        }

        public async Task<List<Sensor>> QueryByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<Sensor>();
            }

            // 使用表达式构建查询条件
            var predicate = PredicateBuilder.New<Sensor>(false); // 初始化为空条件
            foreach (var id in ids)
            {
                var tempId = id; // 避免闭包问题
                predicate = predicate.Or(m => m.Id == tempId);
            }
            predicate = predicate.And(m => m.IsDelete == false); // 是否删除 = false

            // 查询数据
            var items = await GetListAsync(predicate);
            return items.ToList();
        }

        /// <summary>
        /// 根据传感器ID获取单条记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Sensor GetBySensor(string id)
        {
            var item = Client.Queryable<Sensor>().Where(w => w.Id.Equals(id)).First();
            return item;
        }

        /// <summary>
        /// 传感器列表
        /// </summary>
        /// <returns></returns>
        public List<Sensor> GetSensorList()
        {
            return Client.Queryable<Sensor>().ToList();
        }
    }
}
