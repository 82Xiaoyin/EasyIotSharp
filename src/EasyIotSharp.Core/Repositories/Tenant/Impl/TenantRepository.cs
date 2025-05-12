using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EasyIotSharp.Core.Extensions;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;

namespace EasyIotSharp.Core.Repositories.Tenant.Impl
{
    /// <summary>
    /// 租户仓储实现类
    /// </summary>
    public class TenantRepository : MySqlRepositoryBase<EasyIotSharp.Core.Domain.Tenant.Tenant, string>, ITenantRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseProvider">数据库提供者</param>
        public TenantRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 查询租户列表
        /// </summary>
        /// <param name="keyword">关键字（租户名称）</param>
        /// <param name="expiredType">过期类型（0:待授权, 1:生效中, 2:已过期）</param>
        /// <param name="contractStartTime">合同开始时间</param>
        /// <param name="contractEndTime">合同结束时间</param>
        /// <param name="isFreeze">冻结状态（-1:全部, 0:未冻结, 1:已冻结）</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>总数和租户列表</returns>
        /// <remarks>
        /// 查询条件：
        /// 1. 未删除的记录
        /// 2. 名称包含关键字（如果提供）
        /// 3. 根据过期类型筛选：
        ///    - 待授权：合同开始时间大于当前时间
        ///    - 生效中：当前时间在合同期限内
        ///    - 已过期：合同结束时间小于当前时间
        /// 4. 合同时间范围筛选（如果提供）
        /// 5. 冻结状态筛选（如果提供）
        /// 
        /// 分页处理：
        /// - 先获取总记录数
        /// - 当总记录数为0时返回空列表
        /// - 使用基类的分页查询方法获取数据
        /// </remarks>
        public async Task<(int totalCount, List<EasyIotSharp.Core.Domain.Tenant.Tenant> items)> Query(
            string keyword,
            int expiredType,
            DateTime? contractStartTime,
            DateTime? contractEndTime,
            int isFreeze,
            int pageIndex,
            int pageSize)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<EasyIotSharp.Core.Domain.Tenant.Tenant>(t => t.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                predicate = predicate.And(t => t.Name.Contains(keyword));
            }

            switch (expiredType)
            {
                case 0: // 待授权
                    predicate = predicate.And(t => t.ContractStartTime > DateTime.Now);
                    break;
                case 1: // 生效中
                    predicate = predicate.And(t => t.ContractStartTime <= DateTime.Now && t.ContractEndTime > DateTime.Now);
                    break;
                case 2: // 已过期
                    predicate = predicate.And(t => t.ContractEndTime <= DateTime.Now);
                    break;
            }

            if (contractStartTime.HasValue && contractEndTime.HasValue)
            {
                predicate = predicate.And(t => t.ContractStartTime >= contractStartTime.Value && t.ContractEndTime <= contractEndTime.Value);
            }
            else
            {
                if (contractStartTime.HasValue)
                {
                    predicate = predicate.And(t => t.ContractStartTime >= contractStartTime.Value);
                }
                if (contractEndTime.HasValue)
                {
                    predicate = predicate.And(t => t.ContractEndTime <= contractEndTime.Value);
                }
            }

            if (isFreeze > -1)
            {
                predicate = predicate.And(t => t.IsFreeze == (isFreeze == 1));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<EasyIotSharp.Core.Domain.Tenant.Tenant>());
            }

            // 分页查询
            var items = await GetPagedListAsync(predicate, pageIndex, pageSize);

            return (totalCount, items);
        }

        /// <summary>
        /// 获取所有租户
        /// </summary>
        /// <returns></returns>
        public async Task<List<EasyIotSharp.Core.Domain.Tenant.Tenant>> GetTenantList()
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<EasyIotSharp.Core.Domain.Tenant.Tenant>(t => t.IsDelete == false);
            return await GetListAsync(predicate);
        }
    }
}