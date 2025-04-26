using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Rule
{
    public interface IRuleChainRepository : IMySqlRepositoryBase<RuleChain, string>
    {
        /// <summary>
        /// 查询规则链列表
        /// </summary>
        Task<(int totalCount, List<RuleChainDto> items)> Query(
            string keyword,
            string projectId,
            bool isPage,
            int pageIndex,
            int pageSize);

        /// <summary>
        /// 获取所有场景联动数据
        /// </summary>
        /// <returns></returns>
        Task<List<RuleChainDto>> QueryRuleChain();
    }
}