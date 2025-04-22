using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqKit;
using EasyIotSharp.Core.Dto.Rule;

namespace EasyIotSharp.Core.Repositories.Rule.Impl
{
    public class RuleConditionRepository : MySqlRepositoryBase<RuleCondition, string>, IRuleConditionRepository
    {
        public RuleConditionRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 条件配置列表
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <param name="keyword"></param>
        /// <param name="isEnable"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(int totalCount, List<RuleConditionDto> items)> Query(
            string ruleCode,
            string keyword,
            int isEnable,
            int pageIndex,
            int pageSize)
        {
            var predicate = PredicateBuilder.New<RuleCondition>(r => r.IsDelete == false);

            if (!string.IsNullOrWhiteSpace(ruleCode))
            {
                ruleCode = ruleCode.Trim();
                predicate = predicate.And(r => r.RuleConditionCode.Contains(ruleCode));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                predicate = predicate.And(r => r.Label.Contains(keyword) || r.Tag.Contains(keyword));
            }

            if (isEnable > -1)
            {
                predicate = predicate.And(r => r.IsEnable == (isEnable == 0 ? false : true));
            }

            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<RuleConditionDto>());
            }

            var items = await Client.Queryable<RuleCondition>()
                .Where(predicate)
                .OrderByDescending(r => r.CreationTime)
                .Select<RuleConditionDto>()
                .ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }
    }
}