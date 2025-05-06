using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    /// <summary>
    /// 分类仓储实现类
    /// </summary>
    public class ClassificationRepository : MySqlRepositoryBase<Classification, string>, IClassificationRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseProvider">数据库提供者</param>
        public ClassificationRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 查询分类列表
        /// </summary>
        /// <param name="tenantNumId">租户编号</param>
        /// <param name="projectId">项目ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="isPage">是否分页</param>
        /// <returns>总数和分类列表</returns>
        /// <remarks>
        /// 查询条件：
        /// 1. 未删除的记录
        /// 2. 指定租户的记录（如果提供）
        /// 3. 指定项目的记录（如果提供）
        /// 
        /// 排序方式：
        /// 1. 优先按Sort字段降序
        /// 2. 其次按创建时间降序
        /// 
        /// 分页处理：
        /// - 当isPage为true时执行分页查询
        /// - 当isPage为false时返回所有结果
        /// </remarks>
        public async Task<(int totalCount, List<Classification> items)> Query(int tenantNumId,
                                                                              string projectId,
                                                                              int pageIndex,
                                                                              int pageSize,
                                                                              bool isPage)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<Classification>(t => t.IsDelete == false);
            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId.Equals(projectId));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<Classification>());
            }

            if (isPage==true)
            {
                var query = Client.Queryable<Classification>().Where(predicate)
                                  .OrderBy(x => x.Sort, OrderByType.Desc)
                                  .OrderBy(m => m.CreationTime, OrderByType.Desc)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
            else
            {
                var query = Client.Queryable<Classification>().Where(predicate)
                  .OrderBy(x => x.Sort, OrderByType.Desc)
                  .OrderBy(m => m.CreationTime, OrderByType.Desc);
                // 查询数据
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
        }

        /// <summary>
        /// 根据ID列表批量查询分类
        /// </summary>
        /// <param name="ids">分类ID列表</param>
        /// <returns>分类列表</returns>
        /// <remarks>
        /// 查询特点：
        /// 1. 使用 LinqKit 的 PredicateBuilder 构建动态查询条件
        /// 2. 使用 OR 条件组合多个ID查询
        /// 3. 排除已删除的记录
        /// 4. 使用临时变量避免闭包问题
        /// 
        /// 空值处理：
        /// - 当传入的ID列表为空时返回空列表
        /// </remarks>
        public async Task<List<Classification>> QueryByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<Classification>();
            }

            // 使用表达式构建查询条件
            var predicate = PredicateBuilder.New<Classification>(false); // 初始化为空条件
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
    }
}
