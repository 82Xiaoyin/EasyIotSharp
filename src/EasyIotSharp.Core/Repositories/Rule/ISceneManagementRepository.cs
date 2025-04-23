using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Rule
{
    public interface ISceneManagementRepository : IMySqlRepositoryBase<SceneManagement, string>
    {
        /// <summary>
        /// 分页查询场景列表
        /// </summary>
        /// <param name="tenantNumId">租户编号</param>
        /// <param name="keyword">关键字</param>
        /// <param name="type">场景类型</param>
        /// <param name="triggerType">触发类型</param>
        /// <param name="isEnable">是否启用（-1:全部）</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns></returns>
        Task<(int totalCount, List<SceneManagementDto> items)> Query(
            string keyword,
            int pageIndex,
            int pageSize);
    }
}