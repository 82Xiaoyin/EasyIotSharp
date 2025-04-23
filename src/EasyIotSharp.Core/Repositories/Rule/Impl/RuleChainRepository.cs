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
            int pageIndex,
            int pageSize)
        {
            var predicate = PredicateBuilder.New<RuleChain>(x => x.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                predicate = predicate.And(x => x.Name.Contains(keyword));
            }

            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<RuleChainDto>());
            }

            var items = await Client.Queryable<RuleChain>()
                .Where(predicate)
                .OrderByDescending(x => x.CreationTime)
                .Select<RuleChainDto>()
                .ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }
    }
}