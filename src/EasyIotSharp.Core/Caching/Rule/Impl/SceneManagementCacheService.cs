using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule.Impl
{
    public class SceneManagementCacheService : CachingServiceBase, ISceneManagementCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Rule}ISceneManagementCacheService");
        public const string KEY_RULE_SCENEMANAGEMENT_QUERY = "Rule:SceneManagement-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<SceneManagementDto>> QuerySceneManagement(SceneManagementInput input, Func<Task<PagedResultDto<SceneManagementDto>>> action)
        {
            var key = KEY_RULE_SCENEMANAGEMENT_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
