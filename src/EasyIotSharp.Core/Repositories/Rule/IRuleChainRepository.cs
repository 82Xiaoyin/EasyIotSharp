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
            int pageIndex,
            int pageSize);
    }
}