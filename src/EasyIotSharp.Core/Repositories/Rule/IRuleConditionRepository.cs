using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Rule
{
    public interface IRuleConditionRepository : IMySqlRepositoryBase<RuleCondition, string>
    {
        /// <summary>
        /// 条件配置列表
        /// </summary>
        /// <param name="ruleCode"></param>
        /// <param name="keyword"></param>
        /// <param name="isEnable"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(int totalCount, List<RuleConditionDto> items)> Query(
            string ruleCode,
            string keyword,
            int isEnable,
            int pageIndex,
            int pageSize);
    }
}