using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule
{
    public interface ISceneManagementCacheService : ICacheService
    {
        /// <summary>
        /// 场景管理
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<SceneManagementDto>> QuerySceneManagement(SceneManagementInput input, Func<Task<PagedResultDto<SceneManagementDto>>> action);
    }
}
