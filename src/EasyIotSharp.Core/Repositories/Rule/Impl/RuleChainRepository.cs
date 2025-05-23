using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Rule.Impl
{
    public class RuleChainRepository : MySqlRepositoryBase<RuleChain, string>, IRuleChainRepository
    {
        public RuleChainRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 场景联动列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(int totalCount, List<RuleChainDto> items)> Query(
            string keyword,
            string projectId,
            bool isPage,
            int pageIndex,
            int pageSize)
        {
            var predicate = PredicateBuilder.New<RuleChain>(x => x.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                predicate = predicate.And(x => x.Name.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(x => x.ProjectId.Equals(projectId));
            }

            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<RuleChainDto>());
            }
            var items = new List<RuleChainDto>();
            if (isPage == true)
            {
                items = await Client.Queryable<RuleChain>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<RuleChainDto>()
                   .ToPageListAsync(pageIndex, pageSize);
            }
            else
            {
                items = await Client.Queryable<RuleChain>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<RuleChainDto>()
                   .ToListAsync();
            }

            return (totalCount, items);
        }

        /// <summary>
        /// 场景联动列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<RuleChainDto>> QueryRuleChain()
        {
            var predicate = PredicateBuilder.New<RuleChain>(x => x.IsDelete == false);

            var items = await Client.Queryable<RuleChain>()
                .Where(predicate)
                .OrderByDescending(x => x.CreationTime)
                .Select<RuleChainDto>().ToListAsync();

            return items;
        }
    }
}