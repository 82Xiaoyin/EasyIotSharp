using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule
{
    /// <summary>
    /// 场景管理服务接口
    /// </summary>
    public interface ISceneManagementService
    {
        /// <summary>
        /// 查询场景列表
        /// </summary>
        Task<PagedResultDto<SceneManagementDto>> QuerySceneManagement(SceneManagementInput input);

        /// <summary>
        /// 新增场景
        /// </summary>
        Task InsertSceneManagement(InsertSceneManagement input);

        /// <summary>
        /// 修改场景
        /// </summary>
        Task UpdateSceneManagement(InsertSceneManagement input);

        /// <summary>
        /// 删除场景
        /// </summary>
        Task DeleteSceneManagement(DeleteInput input);
    }
}